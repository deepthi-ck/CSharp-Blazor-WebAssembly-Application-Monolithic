using System;

namespace BlazorWasmMonolith
{
    public sealed class ExpirationManager
    {
        public DateTimeOffset? ComputeExpiration(TimeSpan? ttl, TimeSpan defaultTtl, DateTimeOffset utcNow)
        {
            TimeSpan effective = ttl.HasValue ? ttl.Value : defaultTtl;
            if (effective <= TimeSpan.Zero)
            {
                return null;
            }

            return utcNow.Add(effective);
        }

        public bool Sweep(InMemoryStore store, DateTimeOffset utcNow)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            bool removedAny = false;
            foreach (ResourceEntry entry in store.Snapshot())
            {
                if (entry.IsExpired(utcNow))
                {
                    store.Delete(entry.Key);
                    removedAny = true;
                }
            }

            return removedAny;
        }
    }
}
