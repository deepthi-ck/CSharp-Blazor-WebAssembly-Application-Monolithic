using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorWasmMonolith
{
    public sealed class EvictionManager
    {
        public int EvictToCapacity(InMemoryStore store, int capacity)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be positive.", "capacity");
            }

            int evicted = 0;
            IList<string> oldest = store.KeysByOldestAccess();
            while (store.Count > capacity)
            {
                string victim = oldest.FirstOrDefault(k => store.Contains(k));
                if (string.IsNullOrWhiteSpace(victim))
                {
                    break;
                }

                store.Delete(victim);
                evicted++;
            }

            return evicted;
        }
    }
}
