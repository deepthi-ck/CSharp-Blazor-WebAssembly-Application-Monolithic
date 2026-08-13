using System;
using System.Collections.Generic;

namespace BlazorWasmMonolith.Tests
{
    public static class ReplicationManagerTest
    {
        public static int Run()
        {
            StoreNode replicaA = new StoreNode("node-2");
            StoreNode replicaB = new StoreNode("node-3");
            ReplicationManager replication = new ReplicationManager();
            ResourceEntry entry = new ResourceEntry("basket:2001", "cart", DateTimeOffset.UtcNow, null, 1);
            int copied = replication.ReplicatePut(entry, new List<StoreNode> { replicaA, replicaB });
            Expect.Equal(2, copied);
            Expect.True(replicaA.Store.Contains("basket:2001"), "replica A has key");
            Expect.True(replicaB.Store.Contains("basket:2001"), "replica B has key");
            int removed = replication.ReplicateDelete("basket:2001", new List<StoreNode> { replicaA, replicaB });
            Expect.Equal(2, removed);
            Expect.True(!replicaA.Store.Contains("basket:2001"), "replica A deleted");
            return 0;
        }
    }
}
