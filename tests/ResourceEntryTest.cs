using System;

namespace BlazorWasmMonolith.Tests
{
    public static class ResourceEntryTest
    {
        public static int Run()
        {
            DateTimeOffset created = DateTimeOffset.UtcNow;
            ResourceEntry entry = new ResourceEntry("product:1001", "Visvantha", created, created.Add(TimeSpan.FromSeconds(2)), 1);
            Expect.Equal("product:1001", entry.Key);
            Expect.Equal("Visvantha", entry.Value);
            Expect.True(!entry.IsExpired(created), "should not be expired at creation");
            Expect.True(entry.IsExpired(created.AddSeconds(3)), "should expire after TTL");

            try
            {
                new ResourceEntry(" ", "x", created, null, 1);
                throw new Exception("blank key should fail");
            }
            catch (ArgumentException)
            {
            }

            VersionInfo version = BuildContext.FromAssembly().ToVersionInfo();
            Expect.Equal("1 - Monolithic", version.Scenario);
            Expect.Equal("flat (single module)", version.Module);
            Expect.True(!string.IsNullOrWhiteSpace(version.CustomerVersion), "customer version required");
            return 0;
        }
    }
}
