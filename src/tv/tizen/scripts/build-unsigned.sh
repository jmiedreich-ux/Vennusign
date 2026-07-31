#!/usr/bin/env bash
set -euo pipefail

command -v tizen >/dev/null 2>&1 || {
  echo "Tizen Studio CLI is required for an unsigned local web build." >&2
  exit 1
}

cd "$(dirname "$0")/.."
tizen build-web --output .build .
echo "Unsigned Tizen web build created in .build; signing and WGT packaging are release operations."
