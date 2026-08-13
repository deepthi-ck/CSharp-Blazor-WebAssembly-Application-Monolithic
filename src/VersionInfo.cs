using System.Text.Json.Serialization;

namespace BlazorWasmMonolith
{
    public sealed class VersionInfo
    {
        [JsonPropertyName("application")]
        public string Application { get; set; }

        [JsonPropertyName("scenario")]
        public string Scenario { get; set; }

        [JsonPropertyName("module")]
        public string Module { get; set; }

        [JsonPropertyName("customer_version")]
        public string CustomerVersion { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("app")]
        public string App { get; set; }

        [JsonPropertyName("target_framework")]
        public string TargetFramework { get; set; }
    }
}
