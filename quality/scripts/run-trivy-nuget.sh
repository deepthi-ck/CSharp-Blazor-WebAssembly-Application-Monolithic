#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/quality/reports/trivy-nuget"
mkdir -p "$OUT"
if ! command -v trivy >/dev/null 2>&1; then
  echo "FAIL: trivy not found (NuGet/CVE scan skipped)" | tee "$OUT/error.txt"
  exit 1
fi
trivy fs --scanners vuln --pkg-types library --config "$ROOT/quality/config/trivy.yaml" --format json --output "$OUT/report.json" "$ROOT"
