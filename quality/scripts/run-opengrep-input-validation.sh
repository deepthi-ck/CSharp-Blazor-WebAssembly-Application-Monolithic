#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/quality/reports/opengrep-input-validation"
mkdir -p "$OUT"
SCANNER=""
if command -v opengrep >/dev/null 2>&1; then SCANNER=opengrep; elif command -v semgrep >/dev/null 2>&1; then SCANNER=semgrep; fi
if [[ -z "$SCANNER" ]]; then
  echo "FAIL: Opengrep/semgrep not found (C# input-validation scan skipped)" | tee "$OUT/error.txt"
  exit 1
fi
"$SCANNER" scan --config "$ROOT/quality/config/opengrep-input-validation.yml" "$ROOT/src" --json -o "$OUT/report.json" | tee "$OUT/stdout.txt"
