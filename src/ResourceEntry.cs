using System;

namespace BlazorWasmMonolith
{
    public sealed class ResourceEntry
    {
        public ResourceEntry(string key, string value, DateTimeOffset created, DateTimeOffset? expiration, int revision)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", "key");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Key = key;
            Value = value;
            Created = created;
            Expiration = expiration;
            Revision = revision;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset? Expiration { get; private set; }
        public int Revision { get; private set; }

        public ResourceEntry WithValue(string value, DateTimeOffset? expiration)
        {
            return new ResourceEntry(Key, value, Created, expiration, Revision + 1);
        }

        public bool IsExpired(DateTimeOffset utcNow)
        {
            return Expiration.HasValue && utcNow >= Expiration.Value;
        }
    }
}
