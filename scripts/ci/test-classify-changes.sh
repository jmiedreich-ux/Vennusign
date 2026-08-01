#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
temp_dir="$(mktemp -d)"
trap 'rm -rf "$temp_dir"' EXIT

assert_output() {
  local scenario="$1"
  local key="$2"
  local expected="$3"
  local actual
  actual="$(sed -n "s/^${key}=//p" "$temp_dir/${scenario}.out")"
  if [[ "$actual" != "$expected" ]]; then
    echo "$scenario: expected $key=$expected, got $actual" >&2
    exit 1
  fi
}

run_scenario() {
  local scenario="$1"
  shift
  printf '%s\n' "$@" > "$temp_dir/${scenario}.paths"
  GITHUB_OUTPUT="$temp_dir/${scenario}.out" "$script_dir/classify-changes.sh" "$temp_dir/${scenario}.paths"
}

run_scenario docs docs/work-packages/WP-13.01-example.md PROJECT_STATUS.md
assert_output docs docs_only true
assert_output docs dotnet_api false
assert_output docs display false

run_scenario api src/Vennu.Api/Controllers/VenuesController.cs
assert_output api docs_only false
assert_output api dotnet_api true
assert_output api admin false
assert_output api android_tv false

run_scenario venue src/venue-admin/src/App.tsx
assert_output venue venue_admin true
assert_output venue admin false
assert_output venue display false

run_scenario display src/display/src/main.tsx
assert_output display display true
assert_output display android_tv false

run_scenario tv src/tv/tizen/scripts/validate.mjs
assert_output tv tizen true
assert_output tv android_tv false
assert_output tv dotnet_api false

run_scenario closure docs/work-packages/WP-13.10-phase-13-validation-closure.md
assert_output closure full true
assert_output closure dotnet_api true
assert_output closure admin true
assert_output closure android_tv true

run_scenario workflow .github/workflows/phase02-tests.yml
assert_output workflow full true
assert_output workflow webos true

echo "Change classification scenarios passed."
