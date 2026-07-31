#!/usr/bin/env bash
set -euo pipefail

command -v ares-package >/dev/null 2>&1 || {
  echo "LG webOS TV CLI is required for an unsigned local package." >&2
  exit 1
}

cd "$(dirname "$0")/.."
mkdir -p .build
ares-package . --outdir .build
echo "Unsigned webOS IPK created in .build; signing and store submission are release operations."
