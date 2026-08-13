using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorWasmMonolith
{
    public sealed class AppApi
    {
        private readonly AppService _service;
        private readonly BuildContext _build;

        public AppApi(AppService service, BuildContext build)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            if (build == null)
            {
                throw new ArgumentNullException("build");
            }

            _service = service;
            _build = build;
        }

        public AppService Service { get { return _service; } }

        public object Health()
        {
            return new HealthPayload
            {
                Status = "healthy",
                BlazorWasmApp = "available",
                Nodes = _service.Manager.NodeCount(),
                Scenario = "1-monolithic"
            };
        }

        public VersionInfo Version()
        {
            return _build.ToVersionInfo();
        }

        public object Stats()
        {
            AppStatistics stats = _service.Manager.Statistics;
            return new
            {
                hits = stats.Hits,
                misses = stats.Misses,
                puts = stats.Puts,
                gets = stats.Gets,
                deletes = stats.Deletes,
                replications = stats.Replications,
                evictions = stats.Evictions,
                expirations = stats.Expirations,
                entryCount = _service.Manager.EntryCount(),
                nodeCount = _service.Manager.NodeCount()
            };
        }

        public AppResponse Get(string key)
        {
            return _service.Get(key);
        }

        public AppResponse Put(string key, string value, TimeSpan? ttl)
        {
            return _service.Put(new AppRequest("PUT", key, value, ttl));
        }

        public AppResponse Delete(string key)
        {
            return _service.Delete(key);
        }

        public void LoadSampleData(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath) || !File.Exists(jsonPath))
            {
                return;
            }

            string json = File.ReadAllText(jsonPath);
            SampleDocument document = JsonSerializer.Deserialize<SampleDocument>(json);
            if (document == null || document.Items == null)
            {
                return;
            }

            foreach (SampleItem item in document.Items.Where(i => i != null && !string.IsNullOrWhiteSpace(i.Key)))
            {
                _service.Put(new AppRequest("PUT", item.Key, item.Value, null));
            }
        }

        public static TimeSpan? ParseTtl(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            int seconds;
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds))
            {
                return null;
            }

            return TimeSpan.FromSeconds(seconds);
        }

        private sealed class HealthPayload
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("blazor_wasm_app")]
            public string BlazorWasmApp { get; set; }

            [JsonPropertyName("nodes")]
            public int Nodes { get; set; }

            [JsonPropertyName("scenario")]
            public string Scenario { get; set; }
        }

        private sealed class SampleDocument
        {
            [JsonPropertyName("items")]
            public List<SampleItem> Items { get; set; }
        }

        private sealed class SampleItem
        {
            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
    }
}
