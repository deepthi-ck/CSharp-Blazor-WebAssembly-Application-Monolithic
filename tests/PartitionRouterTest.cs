using System.Collections.Generic;
using System.Linq;

namespace BlazorWasmMonolith.Tests
{
    public static class PartitionRouterTest
    {
        public static int Run()
        {
            List<StoreNode> nodes = new List<StoreNode>
            {
                new StoreNode("node-1"),
                new StoreNode("node-2"),
                new StoreNode("node-3")
            };
            PartitionRouter router = new PartitionRouter(nodes);
            StoreNode primary = router.PrimaryFor("product:1001");
            Expect.True(primary != null, "primary required");
            Expect.Equal(2, router.ReplicasFor("product:1001").Count);
            Expect.True(router.AllNodes().Any(n => n.Id == "node-1"), "node-1 registered");
            Expect.Equal(router.PrimaryFor("product:1001").Id, router.PrimaryFor("product:1001").Id);
            return 0;
        }
    }
}
