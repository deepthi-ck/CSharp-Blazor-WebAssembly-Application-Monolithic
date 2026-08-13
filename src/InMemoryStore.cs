using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BlazorWasmMonolith
{
    public sealed class InMemoryStore
    {
        private readonly ConcurrentDictionary<string, ResourceEntry> _entries;
        private readonly Dictionary<string, DateTimeOffset> _accessOrder;
        private readonly object _gate = new object();

        public InMemoryStore()
        {
            _entries = new ConcurrentDictionary<string, ResourceEntry>(StringComparer.Ordinal);
            _accessOrder = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);
        }

        public void Put(ResourceEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            _entries[entry.Key] = entry;
            Touch(entry.Key);
        }

        public ResourceEntry Get(string key)
        {
            EnsureKey(key);
            ResourceEntry entry;
            if (!_entries.TryGetValue(key, out entry))
            {
                throw new KeyNotFoundException("Key not found: " + key);
            }

            Touch(key);
            return entry;
        }

        public bool TryGet(string key, out ResourceEntry entry)
        {
            EnsureKey(key);
            bool found = _entries.TryGetValue(key, out entry);
            if (found)
            {
                Touch(key);
            }

            return found;
        }

        public bool Contains(string key)
        {
            EnsureKey(key);
            return _entries.ContainsKey(key);
        }

        public bool Delete(string key)
        {
            EnsureKey(key);
            ResourceEntry removed;
            bool deleted = _entries.TryRemove(key, out removed);
            if (deleted)
            {
                lock (_gate)
                {
                    _accessOrder.Remove(key);
                }
            }

            return deleted;
        }

        public void Clear()
        {
            _entries.Clear();
            lock (_gate)
            {
                _accessOrder.Clear();
            }
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public IList<ResourceEntry> Snapshot()
        {
            return _entries.Values.ToList();
        }

        public IList<string> KeysByOldestAccess()
        {
            lock (_gate)
            {
                return _accessOrder
                    .OrderBy(pair => pair.Value)
                    .Select(pair => pair.Key)
                    .ToList();
            }
        }

        private void Touch(string key)
        {
            lock (_gate)
            {
                _accessOrder[key] = DateTimeOffset.UtcNow;
            }
        }

        private static void EnsureKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", "key");
            }
        }
    }
}
