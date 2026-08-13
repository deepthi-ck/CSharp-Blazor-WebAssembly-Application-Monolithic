using System;

namespace BlazorWasmMonolith
{
    public sealed class AppRequest
    {
        public AppRequest(string method, string key, string value, TimeSpan? ttl)
        {
            Method = method;
            Key = key;
            Value = value;
            Ttl = ttl;
        }

        public string Method { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
        public TimeSpan? Ttl { get; private set; }
    }
}
