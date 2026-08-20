#!/usr/bin/env bash
# Writes <dist>/version.json so a deployed static build can be identified from
# outside the deploy job.
#
# Without this, nothing served by an SPA host says which commit it came from,
# so a deploy that uploads successfully but never actually starts serving the
# new files is indistinguishable from a healthy one (#726, #740). The SPA hosts
# run `pm2 serve <wwwroot> --spa`, which serves a real file when one exists and
# only falls back to index.html when it does not — so a real version.json at the
# root is served as JSON with no host configuration change.
#
# Off CI the values fall back to "local", matching what the API's
# ReleaseVersionMetadata reports for an unstamped build.
set -euo pipefail

dist_dir="${1:?usage: stamp-build-version.sh <dist-dir>}"

if [[ ! -d "$dist_dir" ]]; then
  echo "stamp-build-version: '$dist_dir' is not a directory - build before stamping" >&2
  exit 1
fi

source_commit="${VENNU_SOURCE_COMMIT:-${GITHUB_SHA:-local}}"
build_id="${VENNU_BUILD_ID:-${GITHUB_RUN_ID:-local}}"
built_at="${VENNU_BUILT_AT_UTC:-$(date -u +%Y-%m-%dT%H:%M:%SZ)}"

cat > "$dist_dir/version.json" <<JSON
{"sourceCommit":"$source_commit","buildId":"$build_id","builtAtUtc":"$built_at"}
JSON

echo "stamp-build-version: $dist_dir/version.json -> sourceCommit=$source_commit buildId=$build_id"
