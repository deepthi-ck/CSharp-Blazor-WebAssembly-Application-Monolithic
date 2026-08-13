# Version matrix — Scenario 1 Monolithic only

Language = C#  
Priority = 1  
Project Type = Blazor WebAssembly Application  
Scenario = 1 - Monolithic  
Module = flat (single module)  

This matrix is **Scenario 1 only**. It is **not** Scenario 2 split FE/BE.

Proof Source for all tools: Shivam-C# sheet (Customer's Language Version Support column)

## Branches (exactly 8)

| Branch | Customer Version | MSBuild TargetFramework |
|---|---|---|
| C#_net6.0 | 6 | net6.0 |
| C#_net7.0 | 7 | net7.0 |
| C#_net8.0 | 8 | net8.0 |
| C#_net9.0 | 9 | net9.0 |
| C#_net10.0 | 10 | net10.0 |
| C#_net42.0 | 42.0 | net462 (named net42.0; net42 TFM does not exist) |
| C#_net472.0 | 4.7.2 | net472 |
| C#_net648.0 | 648.0 | net48 (named net648.0 = Framework 4.8) |

## Tools on every branch (all 12)

| Tool | Tool Verified Version | Derivation Note |
|---|---|---|
| unilyze | 8 | .NET 8–10, C# 8–14 |
| Dolos | N/A (language/runtime-version agnostic) | C# language-version independent (source similarity) |
| Opengrep | 12 | C# 12+ |
| Opengrep (unilyze wrong — code-health/complexity, not OWASP/secure-coding SAST) | 12 | C# 12+ |
| Opengrep (unilyze wrong — no input-validation or taint analysis) | 12 | C# 12+ |
| Opengrep (unilyze wrong — no secrets/dataflow/taint tracking) | 12 | C# 12+ |
| Opengrep (unilyze wrong — no auth/access-control analysis) | 12 | C# 12+ |
| Trivy (unilyze wrong — no NuGet/CVE/supply-chain scanning) | N/A (language/runtime-version agnostic) | .NET / NuGet (language-version agnostic) |
| Trivy | N/A (language/runtime-version agnostic) | .NET / NuGet (language-version agnostic) |
| MiniCover | 8 | .NET SDK 8/9/10 |
| Stryker.NET | 8 | .NET 8–10, C# 12–14 |
| diff-cover | UNRESOLVED | Not C# version dependent; consumes MiniCover coverage (OpenCover/Cobertura) |

Each of the 8 branches carries the same 12-tool list (Language, Priority, Project Type, Scenario, Module, Customer Version, Tool, Tool Verified Version, Proof Source, Derivation Note).
