# Quality tools (12 alt tools)

All 12 alternative tools run on **every** Scenario 1 branch against this C# Blazor WASM monolith.

| # | Tool | Config | Script | Report |
|---|---|---|---|---|
| 1 | unilyze | `config/unilyze.json` | `scripts/run-unilyze.sh` | `reports/unilyze/` |
| 2 | Dolos | `config/dolos.json` | `scripts/run-dolos.sh` | `reports/dolos/` |
| 3 | Opengrep | `config/opengrep.yml` | `scripts/run-opengrep.sh` | `reports/opengrep/` |
| 4 | Opengrep code-health | `config/opengrep-code-health.yml` | `scripts/run-opengrep-code-health.sh` | `reports/opengrep-code-health/` |
| 5 | Opengrep input-validation | `config/opengrep-input-validation.yml` | `scripts/run-opengrep-input-validation.sh` | `reports/opengrep-input-validation/` |
| 6 | Opengrep secrets | `config/opengrep-secrets.yml` | `scripts/run-opengrep-secrets.sh` | `reports/opengrep-secrets/` |
| 7 | Opengrep auth | `config/opengrep-auth.yml` | `scripts/run-opengrep-auth.sh` | `reports/opengrep-auth/` |
| 8 | Trivy NuGet/CVE | `config/trivy.yaml` | `scripts/run-trivy-nuget.sh` | `reports/trivy-nuget/` |
| 9 | Trivy | `config/trivy.yaml` | `scripts/run-trivy.sh` | `reports/trivy/` |
| 10 | MiniCover | `config/minicover.json` | `scripts/run-minicover.sh` | `reports/minicover/` |
| 11 | Stryker.NET | `config/stryker-config.json` | `scripts/run-stryker.sh` | `reports/stryker/` |
| 12 | diff-cover | consumes MiniCover coverage | `scripts/run-diff-cover.sh` | `reports/diff-cover/` |

Interconnect: MiniCover coverage XML → diff-cover.

`python build.py` orchestrates solution build, tests, these 12 tools, app start, and E2E.

Missing tool binary or skipped C# analysis is **FAIL** (not PASS).
