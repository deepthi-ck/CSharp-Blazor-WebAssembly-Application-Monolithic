using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorWasmMonolith
{
    public sealed class AppClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly bool _ownsClient;

        public AppClient(string baseAddress)
            : this(new HttpClient { BaseAddress = new Uri(baseAddress, UriKind.Absolute) }, true)
        {
        }

        public AppClient(HttpClient http, bool ownsClient)
        {
            if (http == null)
            {
                throw new ArgumentNullException("http");
            }

            _http = http;
            _ownsClient = ownsClient;
        }

        public async Task<string> GetHealthAsync()
        {
            HttpResponseMessage response = await _http.GetAsync("health").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<VersionInfo> GetVersionAsync()
        {
            HttpResponseMessage response = await _http.GetAsync("version").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<VersionInfo>(json);
        }

        public async Task<string> GetStatsAsync()
        {
            HttpResponseMessage response = await _http.GetAsync("app/stats").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<AppResponse> GetAsync(string key)
        {
            HttpResponseMessage response = await _http.GetAsync("app/" + Uri.EscapeDataString(key)).ConfigureAwait(false);
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppResponse>(json);
        }

        public async Task<AppResponse> PutAsync(string key, string value, int? ttlSeconds)
        {
            string url = "app/" + Uri.EscapeDataString(key);
            if (ttlSeconds.HasValue)
            {
                url += "?ttl=" + ttlSeconds.Value;
            }

            StringContent content = new StringContent(value ?? string.Empty, Encoding.UTF8, "text/plain");
            HttpResponseMessage response = await _http.PutAsync(url, content).ConfigureAwait(false);
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppResponse>(json);
        }

        public async Task<AppResponse> DeleteAsync(string key)
        {
            HttpResponseMessage response = await _http.DeleteAsync("app/" + Uri.EscapeDataString(key)).ConfigureAwait(false);
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppResponse>(json);
        }

        public void Dispose()
        {
            if (_ownsClient)
            {
                _http.Dispose();
            }
        }
    }
}
