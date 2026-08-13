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
                    string value = await ReadBody(request).ConfigureAwait(false);
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

            ServeUi(response, path);
        }

        private void ServeUi(HttpListenerResponse response, string path)
        {
            string relative = path == "/" ? "index.html" : path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string full = Path.Combine(_wwwroot, relative);
            if (File.Exists(full))
            {
                byte[] bytes = File.ReadAllBytes(full);
                response.StatusCode = 200;
                response.ContentType = full.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                    ? "application/javascript"
                    : "text/html; charset=utf-8";
                response.ContentLength64 = bytes.Length;
                response.OutputStream.Write(bytes, 0, bytes.Length);
                response.OutputStream.Close();
                return;
            }

            response.StatusCode = 404;
            byte[] missing = Encoding.UTF8.GetBytes("not found");
            response.OutputStream.Write(missing, 0, missing.Length);
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
