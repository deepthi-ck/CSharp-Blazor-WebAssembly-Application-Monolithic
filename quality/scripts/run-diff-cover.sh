#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/quality/reports/diff-cover"
COV="$ROOT/quality/reports/minicover/cobertura.xml"
mkdir -p "$OUT"
if [[ ! -f "$COV" ]]; then
  echo "FAIL: MiniCover coverage XML missing; diff-cover cannot run" | tee "$OUT/error.txt"
  exit 1
fi
if ! command -v diff-cover >/dev/null 2>&1; then
  echo "FAIL: diff-cover not found (changed-code coverage skipped)" | tee "$OUT/error.txt"
  exit 1
fi
diff-cover "$COV" --html-report "$OUT/diff-cover.html" --json-report "$OUT/diff-cover.json" | tee "$OUT/stdout.txt"
