using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace BlazorWasmMonolith.Tests
{
    public static class UiNavigationTest
    {
        public static int Run()
        {
            BuildContext build = BuildContext.FromAssembly();
            AppConfiguration configuration = AppConfiguration.CreateDefault();
            configuration.Port = 18080;
            AppApi api = new AppApi(new AppService(new AppManager(configuration)), build);
            api.Put("product:1001", "Visvantha", null);

            string prefix = "http://127.0.0.1:18080/";
            using (MonolithHost host = new MonolithHost(api, prefix, FindWwwroot()))
            {
                host.Start();
                Thread.Sleep(250);
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(8);
                    string[] pages = new[] { "/", "/resources.html", "/stats.html", "/nodes.html", "/health.html", "/version.html" };
                    for (int i = 0; i < pages.Length; i++)
                    {
                        string html = client.GetStringAsync(prefix.TrimEnd('/') + pages[i]).GetAwaiter().GetResult();
                        Expect.True(html.IndexOf("Blazor", StringComparison.Ordinal) >= 0, "title " + pages[i]);
                        Expect.True(html.IndexOf("id=\"sidebar\"", StringComparison.Ordinal) >= 0, "nav " + pages[i]);
                    }

                    string resources = client.GetStringAsync(prefix.TrimEnd('/') + "/resources.html").GetAwaiter().GetResult();
                    Expect.True(resources.IndexOf("product:1001", StringComparison.Ordinal) >= 0, "key placeholder html");
                    Expect.True(resources.IndexOf("Visvantha", StringComparison.Ordinal) >= 0, "value placeholder html");
                    Expect.True(resources.IndexOf("placeholder=\"Name\"", StringComparison.OrdinalIgnoreCase) < 0, "no default Name");
                    Expect.True(resources.IndexOf("placeholder=\"key\"", StringComparison.OrdinalIgnoreCase) < 0, "no default key");

                    string list = client.GetStringAsync(prefix.TrimEnd('/') + "/app/resources").GetAwaiter().GetResult();
                    Expect.True(list.IndexOf("product:1001", StringComparison.Ordinal) >= 0, "sample data listed");
                    Expect.True(list.IndexOf("Visvantha", StringComparison.Ordinal) >= 0, "sample value listed");
                }
            }

            return 0;
        }

        private static string FindRoot()
        {
            string cwd = Directory.GetCurrentDirectory();
            if (Directory.Exists(Path.Combine(cwd, "src"))) { return cwd; }
            if (Directory.Exists(Path.Combine(cwd, "..", "src"))) { return Path.GetFullPath(Path.Combine(cwd, "..")); }
            return cwd;
        }

        private static string FindWwwroot()
        {
            string[] candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "wwwroot"),
                Path.Combine(FindRoot(), "src", "wwwroot")
            };
            for (int i = 0; i < candidates.Length; i++)
            {
                if (Directory.Exists(candidates[i])) { return candidates[i]; }
            }

            return Path.Combine(FindRoot(), "src", "wwwroot");
        }
    }
}
