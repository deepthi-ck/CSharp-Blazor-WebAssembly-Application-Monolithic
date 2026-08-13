using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorWasmMonolith.Tests
{
    public static class AppClientTest
    {
        public static int Run()
        {
            return RunAsync().GetAwaiter().GetResult();
        }

        private static async Task<int> RunAsync()
        {
            BuildContext build = BuildContext.FromAssembly();
            build.ValidateMonolithSameVersion();
            AppConfiguration configuration = AppConfiguration.CreateDefault();
            configuration.Port = 5088;
            AppApi api = new AppApi(new AppService(new AppManager(configuration)), build);
            string wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            Directory.CreateDirectory(wwwroot);
            string prefix = "http://127.0.0.1:" + configuration.Port + "/";

            using (MonolithHost host = new MonolithHost(api, prefix, wwwroot))
            {
                host.Start();
                Thread.Sleep(200);
                using (AppClient client = new AppClient(prefix))
                {
                    string health = await client.GetHealthAsync().ConfigureAwait(false);
                    Expect.True(health.Contains("healthy"), "health");
                    VersionInfo version = await client.GetVersionAsync().ConfigureAwait(false);
                    Expect.Equal(build.CustomerVersion, version.CustomerVersion);
                    Expect.Equal(build.Branch, version.Branch);
                    Expect.Equal("1 - Monolithic", version.Scenario);

                    AppResponse put = await client.PutAsync("product:1001", "Visvantha", null).ConfigureAwait(false);
                    Expect.Equal("SUCCESS", put.Status);
                    AppResponse get = await client.GetAsync("product:1001").ConfigureAwait(false);
                    Expect.Equal("Visvantha", get.Value);
                    await client.DeleteAsync("product:1001").ConfigureAwait(false);
                    AppResponse missing = await client.GetAsync("product:1001").ConfigureAwait(false);
                    Expect.Equal("NOT_FOUND", missing.Status);

                    string stats = await client.GetStatsAsync().ConfigureAwait(false);
                    Expect.True(stats.Contains("puts"), "stats json");
                }
            }

            return 0;
        }
    }
}
