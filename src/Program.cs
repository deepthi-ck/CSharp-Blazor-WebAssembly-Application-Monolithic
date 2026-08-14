using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorWasmMonolith
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                return Run(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<int> Run(string[] args)
        {
            BuildContext build = BuildContext.FromAssembly();
            build.ValidateMonolithSameVersion();

            AppConfiguration configuration = LoadConfiguration();
            int portOverride;
            string portEnv = Environment.GetEnvironmentVariable("APP_PORT");
            if (!string.IsNullOrWhiteSpace(portEnv) && int.TryParse(portEnv, out portOverride))
            {
                configuration.Port = portOverride;
            }

            AppManager manager = new AppManager(configuration);
            AppService service = new AppService(manager);
            AppApi api = new AppApi(service, build);
            api.LoadSampleData(FindFile("data", "sample-app-data.json"));

            string prefix = "http://" + configuration.Host + ":" + configuration.Port + "/";
            string wwwroot = FindDirectory("wwwroot");
            using (MonolithHost host = new MonolithHost(api, prefix, wwwroot))
            {
                host.Start();
                Console.WriteLine("C# Blazor WebAssembly Application (Scenario 1 - Monolithic)");
                Console.WriteLine("Branch: " + build.Branch);
                Console.WriteLine("Customer Version: " + build.CustomerVersion);
                Console.WriteLine("TFM: " + build.TargetFramework);
                Console.WriteLine("Listening: " + prefix);
                Console.WriteLine("Operator UI: " + prefix);

                if (HasFlag(args, "--self-test"))
                {
                    int code = await SelfTest(prefix).ConfigureAwait(false);
                    if (code == 0)
                    {
                        code = await HttpUiSelfTest(prefix).ConfigureAwait(false);
                    }

                    host.Stop();
                    return code;
                }

                if (HasFlag(args, "--once"))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    host.Stop();
                    return 0;
                }

                Console.WriteLine("Press Ctrl+C to stop.");
                ManualResetEventSlim done = new ManualResetEventSlim(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    done.Set();
                };
                done.Wait();
                host.Stop();
                return 0;
            }
        }

        private static async Task<int> SelfTest(string prefix)
        {
            using (AppClient client = new AppClient(prefix))
            {
                AppResponse put = await client.PutAsync("product:1001", "Visvantha", null).ConfigureAwait(false);
                if (!string.Equals(put.Status, "SUCCESS", StringComparison.Ordinal))
                {
                    Console.Error.WriteLine("PUT failed");
                    return 1;
                }

                AppResponse get = await client.GetAsync("product:1001").ConfigureAwait(false);
                if (!string.Equals(get.Value, "Visvantha", StringComparison.Ordinal))
                {
                    Console.Error.WriteLine("GET failed");
                    return 1;
                }

                await client.DeleteAsync("product:1001").ConfigureAwait(false);
                AppResponse missing = await client.GetAsync("product:1001").ConfigureAwait(false);
                if (!string.Equals(missing.Status, "NOT_FOUND", StringComparison.Ordinal))
                {
                    Console.Error.WriteLine("DELETE failed");
                    return 1;
                }

                Console.WriteLine("Self-test PASS");
                return 0;
            }
        }

        private static async Task<int> HttpUiSelfTest(string prefix)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string[] pages = new[] { "/", "/resources.html", "/stats.html", "/nodes.html", "/health.html", "/version.html", "/css/app.css", "/js/app.js" };
                foreach (string page in pages)
                {
                    HttpResponseMessage ui = await client.GetAsync(prefix.TrimEnd('/') + page).ConfigureAwait(false);
                    if (!ui.IsSuccessStatusCode)
                    {
                        Console.Error.WriteLine("UI failed: " + page + " " + (int)ui.StatusCode);
                        return 1;
                    }

                    string html = await ui.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (page.EndsWith(".html") || string.Equals(page, "/", StringComparison.Ordinal))
                    {
                        if (html.IndexOf("Blazor", StringComparison.Ordinal) < 0 || html.IndexOf("sidebar", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            Console.Error.WriteLine("UI navigation markup missing: " + page);
                            return 1;
                        }
                    }
                }

                Console.WriteLine("HTTP UI self-test PASS");
                return 0;
            }
        }

        private static AppConfiguration LoadConfiguration()
        {
            string path = FindFile("config", "appsettings.json");
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText(path)) ?? AppConfiguration.CreateDefault();
            }

            return AppConfiguration.CreateDefault();
        }

        private static string FindFile(string folder, string name)
        {
            string[] roots = new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "src")
            };

            foreach (string root in roots)
            {
                string candidate = Path.Combine(root, folder, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                candidate = Path.Combine(root, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return Path.Combine(Directory.GetCurrentDirectory(), folder, name);
        }

        private static string FindDirectory(string name)
        {
            string[] roots = new[]
            {
                Path.Combine(AppContext.BaseDirectory, name),
                Path.Combine(Directory.GetCurrentDirectory(), "src", name),
                Path.Combine(Directory.GetCurrentDirectory(), name)
            };

            foreach (string root in roots)
            {
                if (Directory.Exists(root))
                {
                    return root;
                }
            }

            return Path.Combine(AppContext.BaseDirectory, name);
        }

        private static bool HasFlag(string[] args, string flag)
        {
            if (args == null)
            {
                return false;
            }

            foreach (string arg in args)
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
