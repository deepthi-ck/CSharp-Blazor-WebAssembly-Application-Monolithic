#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/quality/reports/unilyze"
mkdir -p "$OUT"
if ! command -v unilyze >/dev/null 2>&1; then
  echo "FAIL: unilyze not found (C# analysis skipped)" | tee "$OUT/error.txt"
  exit 1
fi
unilyze "$ROOT/src" --config "$ROOT/quality/config/unilyze.json" --output "$OUT" | tee "$OUT/stdout.txt"
