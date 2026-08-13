#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/quality/reports/stryker"
mkdir -p "$OUT"
cd "$ROOT"
dotnet tool restore
if ! dotnet tool run dotnet-stryker --version >/dev/null 2>&1; then
  echo "FAIL: Stryker.NET not available (mutation skipped)" | tee "$OUT/error.txt"
  exit 1
fi
dotnet tool run dotnet-stryker --config-file "$ROOT/quality/config/stryker-config.json" --output "$OUT" | tee "$OUT/stdout.txt"
