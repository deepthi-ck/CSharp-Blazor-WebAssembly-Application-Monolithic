using System.Text.Json.Serialization;

namespace BlazorWasmMonolith
{
    public sealed class AppResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("node")]
        public string Node { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("found")]
        public bool Found { get; set; }

        public static AppResponse Success(string key, string value, string node)
        {
            return new AppResponse
            {
                Status = "SUCCESS",
                Key = key,
                Value = value,
                Node = node,
                Found = true,
                Message = "ok"
            };
        }

        public static AppResponse NotFound(string key)
        {
            return new AppResponse
            {
                Status = "NOT_FOUND",
                Key = key,
                Found = false,
                Message = "not found"
            };
        }

        public static AppResponse Error(string message)
        {
            return new AppResponse
            {
                Status = "ERROR",
                Found = false,
                Message = message
            };
        }
    }
}
