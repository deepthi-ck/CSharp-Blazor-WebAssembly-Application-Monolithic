using System;
using System.Text.Json.Serialization;

namespace BlazorWasmMonolith
{
    public sealed class AppConfiguration
    {
        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("nodeCount")]
        public int NodeCount { get; set; }

        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [JsonPropertyName("defaultTtlSeconds")]
        public int DefaultTtlSeconds { get; set; }

        public TimeSpan DefaultTtl
        {
            get { return TimeSpan.FromSeconds(DefaultTtlSeconds); }
        }

        public static AppConfiguration CreateDefault()
        {
            return new AppConfiguration
            {
                Host = "127.0.0.1",
                Port = 5080,
                NodeCount = 3,
                Capacity = 128,
                DefaultTtlSeconds = 120
            };
        }
    }
}
