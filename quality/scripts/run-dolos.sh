#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/quality/reports/dolos"
mkdir -p "$OUT"
if command -v dolos >/dev/null 2>&1; then
  dolos run "$ROOT/src" --language csharp --output "$OUT" | tee "$OUT/stdout.txt"
elif command -v npx >/dev/null 2>&1; then
  npx --yes @dodona/dolos run "$ROOT/src" --language csharp --output "$OUT" | tee "$OUT/stdout.txt"
else
  echo "FAIL: Dolos not found (C# similarity analysis skipped)" | tee "$OUT/error.txt"
  exit 1
fi
