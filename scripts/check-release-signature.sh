#!/bin/bash
set -euo pipefail

# The learning release has no Developer ID signature or notarization. Native
# ARM64 code still needs the linker's ad hoc signature; inspect it, never re-sign.
fail() { echo "Signature policy failure: $*" >&2; exit 1; }
[[ $# == 1 ]] || fail "Usage: $0 <published-executable>"
[[ -f "$1" && -x "$1" ]] || fail "Expected an executable file: $1"
[[ $(uname -s) == Darwin ]] || fail "Signature inspection requires macOS."

codesign --verify --strict --verbose=2 "$1" || fail "Invalid code signature."
signature=$(codesign --display --verbose=4 "$1" 2>&1) || fail "Cannot inspect signature."
echo "$signature"
grep -Fxq 'Signature=adhoc' <<<"$signature" || fail "Expected the linker's ad hoc signature."
grep -Fxq 'TeamIdentifier=not set' <<<"$signature" || fail "Unexpected signing team."
if grep -q '^Authority=' <<<"$signature"; then
  fail "A certificate signature requires a new distribution policy."
fi
echo "Distribution policy: ad hoc only; no Developer ID signing; not notarized."
