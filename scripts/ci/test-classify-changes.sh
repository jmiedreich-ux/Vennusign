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
  GITHUB_OUTPUT="$temp_dir/${scenario}.out" bash "$script_dir/classify-changes.sh" "$temp_dir/${scenario}.paths"
}

run_scenario docs docs/work-packages/WP-13.01-example.md PROJECT_STATUS.md
assert_output docs docs_only true
assert_output docs dotnet_api false
assert_output docs display false
assert_output docs dev_control false

run_scenario api src/Vennu.Api/Controllers/VenuesController.cs
assert_output api docs_only false
assert_output api dotnet_api true
assert_output api platform_operations false
assert_output api android_tv false

run_scenario venue src/back-office/src/App.tsx
assert_output venue back_office true
assert_output venue platform_operations false
assert_output venue display false

run_scenario display src/display/src/main.tsx
assert_output display display true
assert_output display android_tv false

run_scenario www src/www/src/Home.tsx
assert_output www www true
assert_output www display false
assert_output www back_office false
assert_output www full false

run_scenario tv src/tv/tizen/scripts/validate.mjs
assert_output tv tizen true
assert_output tv android_tv false
assert_output tv dotnet_api false

run_scenario closure docs/work-packages/WP-13.10-phase-13-validation-closure.md
assert_output closure full true
assert_output closure dotnet_api true
assert_output closure platform_operations true
assert_output closure android_tv true

run_scenario workflow .github/workflows/phase02-tests.yml
assert_output workflow full true
assert_output workflow webos true
assert_output workflow dev_control true

run_scenario dev-control tools/Vennu.DevControl/MainWindow.xaml tools/Vennu.DevControl.Tests/BootstrapConfigurationTests.cs
assert_output dev-control full false
assert_output dev-control dev_control true
assert_output dev-control dotnet_api false

run_scenario key-helper scripts/set-platform-operations-key.ps1
assert_output key-helper full false
assert_output key-helper dev_control true
assert_output key-helper dotnet_api false

run_scenario gitignore .gitignore
assert_output gitignore full false
assert_output gitignore docs_only false
assert_output gitignore dotnet_api false
assert_output gitignore back_office false

run_scenario uitests tests/ui/specs/example.spec.ts
assert_output uitests full false
assert_output uitests back_office false
assert_output uitests dotnet_api false

run_scenario unknown config/new-runtime-policy.json
assert_output unknown full true
assert_output unknown dotnet_api true
assert_output unknown display true

echo "Change classification scenarios passed."
