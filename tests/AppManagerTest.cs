using System;
using System.Threading;

namespace BlazorWasmMonolith.Tests
{
    public static class AppManagerTest
    {
        public static int Run()
        {
            AppConfiguration configuration = AppConfiguration.CreateDefault();
            configuration.Capacity = 2;
            configuration.DefaultTtlSeconds = 120;
            AppManager manager = new AppManager(configuration);

            AppResponse put = manager.Put("product:1001", "Visvantha", null);
            Expect.Equal("SUCCESS", put.Status);
            AppResponse get = manager.Get("product:1001");
            Expect.Equal("Visvantha", get.Value);

            StoreNode primary = manager.Router.PrimaryFor("product:1001");
            foreach (StoreNode replica in manager.Router.ReplicasFor("product:1001"))
            {
                Expect.True(replica.Store.Contains("product:1001"), "replicated to " + replica.Id);
            }

            manager.Put("ttl:1", "temp", TimeSpan.FromMilliseconds(50));
            Thread.Sleep(80);
            AppResponse expired = manager.Get("ttl:1");
            Expect.Equal("NOT_FOUND", expired.Status);

            AppResponse deleted = manager.Delete("product:1001");
            Expect.Equal("SUCCESS", deleted.Status);
            Expect.Equal("NOT_FOUND", manager.Get("product:1001").Status);

            AppConfiguration evictionConfig = AppConfiguration.CreateDefault();
            evictionConfig.Capacity = 2;
            AppManager evictionManager = new AppManager(evictionConfig);
            evictionManager.Put("evict:1", "a", null);
            evictionManager.Put("evict:2", "b", null);
            evictionManager.Put("evict:3", "c", null);
            foreach (StoreNode node in evictionManager.Router.AllNodes())
            {
                Expect.True(node.Store.Count <= evictionConfig.Capacity, "eviction keeps capacity on " + node.Id);
            }

            Expect.True(manager.Statistics.Puts > 0, "puts recorded");
            Expect.True(manager.Statistics.Gets > 0, "gets recorded");
            Expect.True(manager.NodeCount() == 3, "three nodes");
            Expect.True(BuildContext.FromAssembly().TargetFramework.Length > 0, "tfm present");
            return 0;
        }
    }
}
