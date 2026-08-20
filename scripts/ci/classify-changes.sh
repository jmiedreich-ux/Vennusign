#!/usr/bin/env bash
set -euo pipefail

paths_file="${1:?usage: classify-changes.sh <paths-file>}"
output_file="${GITHUB_OUTPUT:-/dev/stdout}"

docs_only=true
full="${CI_FULL_VALIDATION:-false}"
dotnet_api=false
dotnet_data_access=false
platform_operations=false
back_office=false
display=false
www=false
android_tv=false
tizen=false
webos=false
dev_control=false

while IFS= read -r path || [[ -n "$path" ]]; do
  [[ -z "$path" ]] && continue

  case "$path" in
    docs/*|ai/handoffs/*|PROJECT_STATUS.md|tracker/assignments.json|AGENTS.md|AI_DEVELOPMENT_GUIDE.md|.github/pull_request_template.md|.github/copilot-instructions.md|.github/ISSUE_TEMPLATE/*|*.md)
      ;;
    *)
      docs_only=false
      ;;
  esac

  case "$path" in
    .github/workflows/*|scripts/ci/*|Vennusign.sln|Directory.*)
      full=true
      ;;
    src/Vennu.Api/*)
      dotnet_api=true
      ;;
    src/Vennu.Core.Models/*)
      dotnet_api=true
      dotnet_data_access=true
      ;;
    src/Vennu.Data/*)
      dotnet_api=true
      ;;
    src/Vennu.DataAccess/*|src/DataAcess.sql/*)
      dotnet_api=true
      dotnet_data_access=true
      ;;
    src/platform-operations/*)
      platform_operations=true
      ;;
    src/back-office/*)
      back_office=true
      ;;
    src/display/*)
      display=true
      ;;
    src/www/*)
      www=true
      ;;
    src/tv/android/*)
      android_tv=true
      ;;
    src/tv/tizen/*)
      tizen=true
      ;;
    src/tv/webos/*)
      webos=true
      ;;
    tools/Vennu.DevControl/*|tools/Vennu.DevControl.Tests/*)
      dev_control=true
      ;;
    scripts/set-platform-operations-key.ps1)
      dev_control=true
      ;;
    # Nothing under tests/ is deployed, and the remaining scripts/ entries are
    # local development and QA launchers. A change to either ships nothing, so it
    # must not trigger a deploy of anything. scripts/ci/* still forces a full run
    # above, because it decides this classification.
    tests/*|scripts/*)
      ;;
  esac

  case "$path" in
    docs/work-packages/WP-??.10-*|docs/work-packages/RWP-??.10-*)
      full=true
      ;;
  esac

  case "$path" in
    docs/*|ai/handoffs/*|PROJECT_STATUS.md|tracker/assignments.json|AGENTS.md|AI_DEVELOPMENT_GUIDE.md|.github/pull_request_template.md|.github/copilot-instructions.md|.github/ISSUE_TEMPLATE/*|*.md|.gitignore|.github/workflows/*|scripts/ci/*|scripts/set-platform-operations-key.ps1|Vennusign.sln|Directory.*|src/Vennu.Api/*|tests/Vennu.Api.Tests/*|src/Vennu.Core.Models/*|src/Vennu.Data/*|src/Vennu.DataAccess/*|src/DataAcess.sql/*|tests/Vennu.DataAccess.Tests/*|tests/*|scripts/*|src/platform-operations/*|src/back-office/*|src/display/*|src/www/*|src/tv/android/*|src/tv/tizen/*|src/tv/webos/*|tools/Vennu.DevControl/*|tools/Vennu.DevControl.Tests/*)
      ;;
    *)
      # New or cross-cutting paths must fail safe until their affected-area mapping is explicit.
      full=true
      ;;
  esac
done < "$paths_file"

if [[ "$full" == true ]]; then
  docs_only=false
  dotnet_api=true
  dotnet_data_access=true
  platform_operations=true
  back_office=true
  display=true
  www=true
  android_tv=true
  tizen=true
  webos=true
  dev_control=true
fi

{
  echo "docs_only=$docs_only"
  echo "full=$full"
  echo "dotnet_api=$dotnet_api"
  echo "dotnet_data_access=$dotnet_data_access"
  echo "platform_operations=$platform_operations"
  echo "back_office=$back_office"
  echo "display=$display"
  echo "www=$www"
  echo "android_tv=$android_tv"
  echo "tizen=$tizen"
  echo "webos=$webos"
  echo "dev_control=$dev_control"
} >> "$output_file"
