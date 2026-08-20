#!/usr/bin/env bash
# Confirms the app is actually serving the commit that was just deployed, and
# restarts it once if it is not.
#
# `azure/webapps-deploy` returns on a successful upload. OneDeploy then requests
# a container recycle asynchronously, and under load - 28 apps on one B1 worker,
# sustained 100% CPU during a five-app deploy - the old worker kept serving. Both
# 2026-08-20 incidents were cleared by a manual `az webapp restart`, so that
# manual step is what this automates, with the difference that the job now fails
# instead of going green when the new build never takes (#740).
#
# The restart is the escalation, not the routine path: the app settings written
# by the deploy job already force a recycle, so the first verification normally
# succeeds and no restart happens.
#
# Requires an already-authenticated `az` (the deploy jobs run azure/login first).
set -euo pipefail

resource_group="${1:?usage: confirm-deployment.sh <resource-group> <app-name> <url> <expected-commit>}"
app_name="${2:?app name is required}"
url="${3:?version url is required}"
expected="${4:?expected commit is required}"

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

first_wait="${CONFIRM_DEPLOYMENT_FIRST_WAIT:-180}"
second_wait="${CONFIRM_DEPLOYMENT_SECOND_WAIT:-240}"

if bash "$script_dir/verify-deployed-build.sh" "$url" "$expected" "$first_wait"; then
  exit 0
fi

echo "::warning::$app_name did not pick up $expected within ${first_wait}s; restarting it once. This is the #740 failure mode - a green upload that never recycled."
az webapp restart --resource-group "$resource_group" --name "$app_name" --output none

if bash "$script_dir/verify-deployed-build.sh" "$url" "$expected" "$second_wait"; then
  echo "::warning::$app_name only started serving $expected after an explicit restart. The deploy would have reported success while serving the previous build."
  exit 0
fi

echo "::error::$app_name is not serving $expected even after a restart. Do not treat this deployment as applied."
exit 1
