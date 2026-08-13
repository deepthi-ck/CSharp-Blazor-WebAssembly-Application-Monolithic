#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/quality/reports/minicover"
mkdir -p "$OUT"
cd "$ROOT"
dotnet tool restore
if ! dotnet minicover --help >/dev/null 2>&1 && ! dotnet tool run minicover --help >/dev/null 2>&1; then
  echo "FAIL: MiniCover not available (coverage skipped)" | tee "$OUT/error.txt"
  exit 1
fi
dotnet tool run minicover instrument --workdir "$ROOT" --assemblies "**/CSharpBlazorWasmMonolith.dll" --sources "src/**/*.cs" | tee "$OUT/stdout.txt"
dotnet run --project tests/CSharpBlazorWasmMonolith.Tests.csproj -c Release --no-build || dotnet run --project tests/CSharpBlazorWasmMonolith.Tests.csproj -c Release
dotnet tool run minicover uninstrument --workdir "$ROOT" | tee -a "$OUT/stdout.txt"
dotnet tool run minicover report --workdir "$ROOT" --threshold 1 | tee -a "$OUT/stdout.txt"
if [[ -f "$ROOT/coverage.xml" ]]; then cp "$ROOT/coverage.xml" "$OUT/cobertura.xml"; fi
if [[ -f "$ROOT/cobertura.xml" ]]; then cp "$ROOT/cobertura.xml" "$OUT/cobertura.xml"; fi
ls "$OUT" >/dev/null
