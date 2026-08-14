using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorWasmMonolith
{
    public sealed class MonolithHost : IDisposable
    {
        private readonly AppApi _api;
        private readonly HttpListener _listener;
        private readonly string _wwwroot;
        private readonly JsonSerializerOptions _json;
        private CancellationTokenSource _cts;
        private Task _loop;

        public MonolithHost(AppApi api, string prefix, string wwwroot)
        {
            if (api == null)
            {
                throw new ArgumentNullException("api");
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Prefix is required.", "prefix");
            }

            _api = api;
            _wwwroot = wwwroot;
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            Prefix = prefix;
            _json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public string Prefix { get; private set; }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            _loop = Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }

            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }

        public void Dispose()
        {
            Stop();
            if (_cts != null)
            {
                _cts.Dispose();
            }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                try
                {
                    await Handle(context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    WriteJson(context.Response, 500, AppResponse.Error(ex.Message));
                }
            }
        }

        private async Task Handle(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string path = request.Url == null ? "/" : request.Url.AbsolutePath.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "/";
            }

            if (string.Equals(path, "/health", StringComparison.OrdinalIgnoreCase) && IsGet(request))
            {
                WriteJson(response, 200, _api.Health());
                return;
            }

            if (string.Equals(path, "/version", StringComparison.OrdinalIgnoreCase) && IsGet(request))
            {
                WriteJson(response, 200, _api.Version());
                return;
            }

            if (string.Equals(path, "/app/stats", StringComparison.OrdinalIgnoreCase) && IsGet(request))
            {
                WriteJson(response, 200, _api.Stats());
                return;
            }

            if (string.Equals(path, "/app/resources", StringComparison.OrdinalIgnoreCase) && IsGet(request))
            {
                WriteJson(response, 200, _api.ListResources());
                return;
            }

            if (string.Equals(path, "/app/nodes", StringComparison.OrdinalIgnoreCase) && IsGet(request))
            {
                WriteJson(response, 200, _api.ListNodes());
                return;
            }

            if (path.StartsWith("/app/", StringComparison.OrdinalIgnoreCase))
            {
                string key = Uri.UnescapeDataString(path.Substring("/app/".Length));
                if (IsGet(request))
                {
                    AppResponse result = _api.Get(key);
                    WriteJson(response, result.Found ? 200 : 404, result);
                    return;
                }

                if (string.Equals(request.HttpMethod, "PUT", StringComparison.OrdinalIgnoreCase))
                {
                    string body = await ReadBody(request).ConfigureAwait(false);
                    string value = body;
                    if (!string.IsNullOrWhiteSpace(body) && body.TrimStart().StartsWith("{", StringComparison.Ordinal))
                    {
                        using (JsonDocument doc = JsonDocument.Parse(body))
                        {
                            JsonElement valueElement;
                            if (doc.RootElement.TryGetProperty("value", out valueElement))
                            {
                                value = valueElement.GetString();
                            }
                        }
                    }

                    TimeSpan? ttl = AppApi.ParseTtl(request.QueryString["ttl"]);
                    AppResponse result = _api.Put(key, value, ttl);
                    WriteJson(response, 200, result);
                    return;
                }

                if (string.Equals(request.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    AppResponse result = _api.Delete(key);
                    WriteJson(response, result.Found ? 200 : 404, result);
                    return;
                }
            }

            if (IsGet(request) && TryServeUi(response, path))
            {
                return;
            }

            WriteHtml(response, 404, "<!DOCTYPE html><html><body><p>Page not found. Return to <a href=\"/\">Dashboard</a>.</p></body></html>");
        }

        private bool TryServeUi(HttpListenerResponse response, string path)
        {
            if (string.IsNullOrWhiteSpace(_wwwroot) || !Directory.Exists(_wwwroot))
            {
                return false;
            }

            string relative = string.Equals(path, "/", StringComparison.Ordinal) ? "index.html" : path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string full = Path.GetFullPath(Path.Combine(_wwwroot, relative));
            string root = Path.GetFullPath(_wwwroot);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                root = root + Path.DirectorySeparatorChar;
            }

            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                return false;
            }

            byte[] bytes = File.ReadAllBytes(full);
            response.StatusCode = 200;
            response.ContentType = ContentType(full);
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
            return true;
        }

        private static string ContentType(string full)
        {
            if (full.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                return "text/css; charset=utf-8";
            }

            if (full.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                return "application/javascript; charset=utf-8";
            }

            return "text/html; charset=utf-8";
        }

        private static void WriteHtml(HttpListenerResponse response, int status, string html)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            response.StatusCode = status;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        private void WriteJson(HttpListenerResponse response, int status, object payload)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, _json));
            response.StatusCode = status;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        private static async Task<string> ReadBody(HttpListenerRequest request)
        {
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private static bool IsGet(HttpListenerRequest request)
        {
            return string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase);
        }
    }
}
