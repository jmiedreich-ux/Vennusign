#!/usr/bin/env bash
# Fails unless the build actually answering at <url> reports <expected-commit>.
#
# A green `azure/webapps-deploy` means the package uploaded, not that the new
# build is running: OneDeploy requests a container recycle asynchronously, and
# under load the old worker keeps serving. That happened twice on 2026-08-20 and
# both times a shipped fix read as "the fix does not work" (#740). This turns a
# silent stale deploy into a failed job.
#
# The endpoint must be one that reports the running build's own commit:
#   /health/version  for the API  (sourceCommit from VENNU_SOURCE_COMMIT)
#   /version.json    for an SPA   (written by stamp-build-version.sh)
#
# Cold starts on the shared B1 plan have been measured at ~49s, so absent or
# failing responses are treated as "not up yet" until the deadline, and only the
# deadline fails the job.
set -euo pipefail

url="${1:?usage: verify-deployed-build.sh <url> <expected-commit> [timeout-seconds] [interval-seconds]}"
expected="${2:?expected commit is required}"
timeout_seconds="${3:-180}"
interval_seconds="${4:-10}"

# The version documents are flat objects, so the field is read directly rather
# than depending on jq being present on the runner or on a developer's machine.
json_string_field() {
  local body="$1" field="$2"
  printf '%s' "$body" \
    | tr -d ' \n\r\t' \
    | sed -n "s/.*\"$field\":\"\([^\"]*\)\".*/\1/p"
}

deadline=$(( SECONDS + timeout_seconds ))
attempt=0
last_seen=""

while :; do
  attempt=$(( attempt + 1 ))
  body="$(curl -sS --max-time 30 "$url" 2>/dev/null || true)"

  if [[ -n "$body" ]]; then
    reported="$(json_string_field "$body" sourceCommit)"
    if [[ -n "$reported" ]]; then
      last_seen="$reported"
      if [[ "$reported" == "$expected" ]]; then
        echo "verify-deployed-build: $url is serving $expected (attempt $attempt)"
        exit 0
      fi
    else
      last_seen="<no sourceCommit in response>"
    fi
  else
    last_seen="${last_seen:-<no response>}"
  fi

  if (( SECONDS >= deadline )); then
    break
  fi

  echo "verify-deployed-build: $url reports '${last_seen}', want '$expected' - retrying in ${interval_seconds}s"
  sleep "$interval_seconds"
done

echo "::error::$url is still reporting '${last_seen}' after ${timeout_seconds}s; expected '${expected}'. The deployed package is not the build that is running."
exit 1
