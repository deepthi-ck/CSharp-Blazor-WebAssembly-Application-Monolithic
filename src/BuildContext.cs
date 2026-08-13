using System;
using System.Linq;
using System.Reflection;

namespace BlazorWasmMonolith
{
    public sealed class BuildContext
    {
        public BuildContext(string branch, string customerVersion, string targetFramework, string scenario, string module)
        {
            Branch = branch;
            CustomerVersion = customerVersion;
            TargetFramework = targetFramework;
            Scenario = scenario;
            Module = module;
        }

        public string Branch { get; private set; }
        public string CustomerVersion { get; private set; }
        public string TargetFramework { get; private set; }
        public string Scenario { get; private set; }
        public string Module { get; private set; }

        public static BuildContext FromAssembly()
        {
            Assembly assembly = typeof(BuildContext).Assembly;
            return new BuildContext(
                Read(assembly, "BranchName"),
                Read(assembly, "CustomerVersion"),
                Read(assembly, "AppTargetFramework"),
                Read(assembly, "Scenario"),
                Read(assembly, "ModuleLayout"));
        }

        public void ValidateMonolithSameVersion()
        {
            if (string.IsNullOrWhiteSpace(CustomerVersion))
            {
                throw new ArgumentException("Customer version is missing.");
            }

            if (string.IsNullOrWhiteSpace(TargetFramework))
            {
                throw new ArgumentException("Target framework is missing.");
            }

            string compiled = DetectCompiledTfm();
            if (!string.Equals(compiled, TargetFramework, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Compiled TFM '" + compiled + "' does not match configured TFM '" + TargetFramework + "'.");
            }
        }

        public VersionInfo ToVersionInfo()
        {
            return new VersionInfo
            {
                Application = "C# Blazor WebAssembly Application",
                Scenario = Scenario,
                Module = Module,
                CustomerVersion = CustomerVersion,
                Branch = Branch,
                App = "ready",
                TargetFramework = TargetFramework
            };
        }

        public static string DetectCompiledTfm()
        {
#if NET10_0
            return "net10.0";
#elif NET9_0
            return "net9.0";
#elif NET8_0
            return "net8.0";
#elif NET7_0
            return "net7.0";
#elif NET6_0
            return "net6.0";
#elif NET48
            return "net48";
#elif NET472
            return "net472";
#elif NET462
            return "net462";
#else
            return "unknown";
#endif
        }

        private static string Read(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute match = assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.Ordinal));
            return match == null ? string.Empty : match.Value;
        }
    }
}
