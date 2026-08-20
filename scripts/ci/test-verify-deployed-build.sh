#!/usr/bin/env bash
# Exercises verify-deployed-build.sh against a real HTTP server rather than a
# stubbed curl, because the behaviour under test is "what does the thing on the
# other end of the wire actually say".
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
temp_dir="$(mktemp -d)"
server_pid=""
cleanup() {
  [[ -n "$server_pid" ]] && kill "$server_pid" 2>/dev/null || true
  rm -rf "$temp_dir"
}
trap cleanup EXIT

port=""
start_server() {
  python3 -u -m http.server 0 --bind 127.0.0.1 --directory "$temp_dir" >"$temp_dir/server.log" 2>&1 &
  server_pid=$!
  for _ in $(seq 1 50); do
    port="$(sed -n 's/.*port \([0-9][0-9]*\).*/\1/p' "$temp_dir/server.log" | head -1)"
    [[ -n "$port" ]] && return 0
    sleep 0.1
  done
  echo "test-verify-deployed-build: server did not start" >&2
  exit 1
}

fail() { echo "test-verify-deployed-build: $1" >&2; exit 1; }

start_server

# A build reporting the expected commit passes immediately.
printf '{"sourceCommit":"abc123","buildId":"77","builtAtUtc":"2026-08-20T00:00:00Z"}' > "$temp_dir/version.json"
if ! bash "$script_dir/verify-deployed-build.sh" "http://127.0.0.1:$port/version.json" abc123 5 1 >"$temp_dir/match.log" 2>&1; then
  cat "$temp_dir/match.log" >&2
  fail "expected a matching commit to pass"
fi

# The stale-deploy case: the previous build is still answering. This is exactly
# what a green azure/webapps-deploy looked like on 2026-08-20 (#740).
if bash "$script_dir/verify-deployed-build.sh" "http://127.0.0.1:$port/version.json" newsha 3 1 >"$temp_dir/stale.log" 2>&1; then
  fail "expected a stale build to fail the job"
fi
grep -q "still reporting 'abc123'" "$temp_dir/stale.log" || {
  cat "$temp_dir/stale.log" >&2
  fail "expected the failure to name the commit actually being served"
}

# An unstamped build - what every dev API reports today - must not pass as new.
printf '{"sourceCommit":"local","buildId":"local"}' > "$temp_dir/version.json"
if bash "$script_dir/verify-deployed-build.sh" "http://127.0.0.1:$port/version.json" abc123 3 1 >"$temp_dir/unstamped.log" 2>&1; then
  fail "expected an unstamped build to fail"
fi

# The API's real /health/version shape, with sourceCommit not first.
printf '{"productVersion":"0.0.0","componentVersion":"0.0.0","apiContractMajor":1,"sourceCommit":"deadbee","buildId":"9"}' > "$temp_dir/version.json"
if ! bash "$script_dir/verify-deployed-build.sh" "http://127.0.0.1:$port/version.json" deadbee 5 1 >"$temp_dir/api.log" 2>&1; then
  cat "$temp_dir/api.log" >&2
  fail "expected the API health/version shape to be read"
fi

# SPA hosts answer a missing file with index.html, so HTML must never pass.
printf '<!doctype html><html><body>app</body></html>' > "$temp_dir/version.json"
if bash "$script_dir/verify-deployed-build.sh" "http://127.0.0.1:$port/version.json" abc123 3 1 >"$temp_dir/html.log" 2>&1; then
  fail "expected an SPA index.html fallback to fail rather than pass"
fi
grep -q "no sourceCommit in response" "$temp_dir/html.log" || {
  cat "$temp_dir/html.log" >&2
  fail "expected the HTML fallback to be reported as having no sourceCommit"
}

# An app that never answers - a cold start that never completes - must fail,
# not hang and not pass.
if bash "$script_dir/verify-deployed-build.sh" "http://127.0.0.1:$port/not-deployed.json" abc123 3 1 >"$temp_dir/down.log" 2>&1; then
  fail "expected an unreachable version endpoint to fail"
fi

# stamp-build-version.sh writes something this script can actually read.
mkdir -p "$temp_dir/dist"
VENNU_SOURCE_COMMIT=stamped VENNU_BUILD_ID=42 bash "$script_dir/stamp-build-version.sh" "$temp_dir/dist" >/dev/null
cp "$temp_dir/dist/version.json" "$temp_dir/version.json"
if ! bash "$script_dir/verify-deployed-build.sh" "http://127.0.0.1:$port/version.json" stamped 5 1 >"$temp_dir/roundtrip.log" 2>&1; then
  cat "$temp_dir/roundtrip.log" >&2
  fail "expected a stamped build to verify"
fi

# Stamping a directory that does not exist fails loudly rather than silently
# shipping an unidentifiable build.
if bash "$script_dir/stamp-build-version.sh" "$temp_dir/no-such-dist" >/dev/null 2>&1; then
  fail "expected stamping a missing dist directory to fail"
fi


# The scripts working is not the point; the deploy actually using them is. The
# failure this guards is the ordinary one - a sixth application is added later
# and nobody remembers the check, so that app goes back to deploying blind.
workflow="$script_dir/../../.github/workflows/deploy-dev.yml"
deployed_apps="$(sed -n 's/^ *app-name: *//p' "$workflow")"
[[ -n "$deployed_apps" ]] || fail "found no deployed apps in deploy-dev.yml"
while read -r app; do
  if ! grep -A2 "confirm-deployment.sh" "$workflow" | grep -q "[\"' ]$app "; then
    fail "$app is deployed by deploy-dev.yml but nothing confirms it is serving the new build (#740)"
  fi
done <<< "$deployed_apps"

# Every static build must be stamped, or the check above has nothing to read.
static_builds="$(grep -c 'stamp-build-version.sh' "$workflow" || true)"
spa_count="$(grep -c 'npm run build' "$workflow" || true)"
if [[ "$static_builds" != "$spa_count" ]]; then
  fail "deploy-dev.yml builds $spa_count static apps but stamps $static_builds of them"
fi

echo "test-verify-deployed-build: all scenarios passed, and deploy-dev.yml is wired to them"
