using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorWasmMonolith
{
    public sealed class StoreNode
    {
        public StoreNode(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Node id is required.", "id");
            }

            Id = id;
            Store = new InMemoryStore();
        }

        public string Id { get; private set; }
        public InMemoryStore Store { get; private set; }

        public bool IsHealthy
        {
            get { return true; }
        }

        public IList<string> Keys()
        {
            return Store.Snapshot().Select(e => e.Key).ToList();
        }
    }
}
