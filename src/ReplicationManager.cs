using System;
using System.Collections.Generic;

namespace BlazorWasmMonolith
{
    public sealed class ReplicationManager
    {
        public int ReplicatePut(ResourceEntry entry, IList<StoreNode> replicas)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (replicas == null)
            {
                throw new ArgumentNullException("replicas");
            }

            int copied = 0;
            for (int i = 0; i < replicas.Count; i++)
            {
                replicas[i].Store.Put(entry);
                copied++;
            }

            return copied;
        }

        public int ReplicateDelete(string key, IList<StoreNode> replicas)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", "key");
            }

            if (replicas == null)
            {
                throw new ArgumentNullException("replicas");
            }

            int removed = 0;
            for (int i = 0; i < replicas.Count; i++)
            {
                if (replicas[i].Store.Delete(key))
                {
                    removed++;
                }
            }

            return removed;
        }
    }
}
