#!/usr/bin/env bash
set -euo pipefail

paths_file="${1:?usage: classify-changes.sh <paths-file>}"
output_file="${GITHUB_OUTPUT:-/dev/stdout}"

docs_only=true
full="${CI_FULL_VALIDATION:-false}"
dotnet_api=false
dotnet_data_access=false
admin=false
venue_admin=false
display=false
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
    src/Vennu.Api/*|tests/Vennu.Api.Tests/*)
      dotnet_api=true
      ;;
    src/Vennu.Core.Models/*)
      dotnet_api=true
      dotnet_data_access=true
      ;;
    src/Vennu.Data/*)
      dotnet_api=true
      ;;
    src/Vennu.DataAccess/*|src/DataAcess.sql/*|tests/Vennu.DataAccess.Tests/*)
      dotnet_api=true
      dotnet_data_access=true
      ;;
    src/admin/*)
      admin=true
      ;;
    src/venue-admin/*)
      venue_admin=true
      ;;
    src/display/*)
      display=true
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
  esac

  case "$path" in
    docs/work-packages/WP-??.10-*|docs/work-packages/RWP-??.10-*)
      full=true
      ;;
  esac

  case "$path" in
    docs/*|ai/handoffs/*|PROJECT_STATUS.md|tracker/assignments.json|AGENTS.md|AI_DEVELOPMENT_GUIDE.md|.github/pull_request_template.md|.github/copilot-instructions.md|.github/ISSUE_TEMPLATE/*|*.md|.github/workflows/*|scripts/ci/*|Vennusign.sln|Directory.*|src/Vennu.Api/*|tests/Vennu.Api.Tests/*|src/Vennu.Core.Models/*|src/Vennu.Data/*|src/Vennu.DataAccess/*|src/DataAcess.sql/*|tests/Vennu.DataAccess.Tests/*|src/admin/*|src/venue-admin/*|src/display/*|src/tv/android/*|src/tv/tizen/*|src/tv/webos/*|tools/Vennu.DevControl/*|tools/Vennu.DevControl.Tests/*)
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
  admin=true
  venue_admin=true
  display=true
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
  echo "admin=$admin"
  echo "venue_admin=$venue_admin"
  echo "display=$display"
  echo "android_tv=$android_tv"
  echo "tizen=$tizen"
  echo "webos=$webos"
  echo "dev_control=$dev_control"
} >> "$output_file"
