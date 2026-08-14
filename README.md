# C# Blazor WebAssembly Application — Scenario 1 Monolithic

Minimal C# Blazor WebAssembly monolith: one repo, one customer .NET version per branch, flat single module. UI and API use the **same** version. This is **not** Scenario 2 (no `CSharp_FE*_BE*` branches).

Inspired conceptually by [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) (Blazor UI + hosted API). The ASP.NET Core tree is **not** cloned.

## Layout

- `src/` — flat module: Blazor WASM UI (`App.razor`, `AppDashboard.razor`, `wwwroot/`), API, store, replication, TTL, eviction
- `tests/` — unit + UI/API integration (same tests feed quality tools)
- `quality/` — 12 alt tools, shared configs/scripts/reports
- `config/`, `data/`, `fixtures/` — one copy each

## Branches (exactly 8)

| Branch | Customer Version | MSBuild TFM |
|---|---|---|
| `C#_net6.0` | 6 | net6.0 |
| `C#_net7.0` | 7 | net7.0 |
| `C#_net8.0` | 8 | net8.0 |
| `C#_net9.0` | 9 | net9.0 |
| `C#_net10.0` | 10 | net10.0 |
| `C#_net42.0` | 42.0 (named) | net462 |
| `C#_net472.0` | 4.7.2 | net472 |
| `C#_net648.0` | 648.0 (named, Framework 4.8) | net48 |

`net42` is not a shipping TFM; `C#_net42.0` compiles as `net462` so `System.Text.Json` + `HttpClient` remain real BCL/package APIs. `C#_net648.0` compiles as `net48`. `/version` still reports the **named** customer version from `Directory.Build.props`.

## Build

```bash
python build.py
```

or `./build.sh`

This detects the branch, validates the single TFM, checks C# built-in usage, builds, tests, runs all 12 alt tools, starts the monolith, and executes PUT/GET/DELETE E2E (`product:1001` = Visvantha).

```bash
dotnet run --project src/CSharpBlazorWasmMonolith.csproj
```

- Operator UI: http://127.0.0.1:5080/
- `GET /health` `GET /version` `GET /app/stats` `GET /app/resources` `GET /app/nodes`
- `GET|PUT|DELETE /app/{key}`

## Operator UI

Full page-to-page navigation (not a single-page-only console):

| Page | Route |
|---|---|
| Dashboard | `/` |
| Resources | `/resources.html` |
| Statistics | `/stats.html` |
| Partition nodes | `/nodes.html` |
| Health | `/health.html` |
| Version | `/version.html` |

Catalog placeholders use `product:1001` / `Visvantha`. The hosted UI talks to the same in-process `AppApi` as `AppClient`.

## Quality tools (all 12, every branch)

unilyze, Dolos, Opengrep, Opengrep code-health, Opengrep input-validation, Opengrep secrets, Opengrep auth, Trivy NuGet/CVE, Trivy, MiniCover, Stryker.NET, diff-cover (MiniCover → diff-cover).

See `quality/README.md` and `docs/version-matrix.md`.
