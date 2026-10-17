#!/bin/bash
set -euo pipefail

# Runner preparation is kept separate from publication so the downloaded binary
# can be checked on the minimum OS without installing or invoking .NET there.
fail() { echo "Prerequisite failure: $*" >&2; exit 2; }
[[ $# == 1 ]] || fail "Usage: $0 <expected-macOS-major-version>"
[[ $(uname -s) == Darwin && $(uname -m) == arm64 ]] || fail "An ARM64 macOS runner is required."
[[ $(sw_vers -productVersion) == "$1".* ]] || fail "Expected macOS $1."
sw_vers
uname -m
for tool in xcodebuild xcrun gcc /usr/bin/python3; do
  command -v "$tool" >/dev/null || fail "Missing $tool; install Xcode with its macOS SDK and Python."
done
xcodebuild -version
xcrun --sdk macosx --show-sdk-version
gcc --version

if ! /usr/bin/arch -x86_64 /usr/bin/true; then
  echo "Installing Rosetta 2 for generated x86-64 programs."
  sudo /usr/sbin/softwareupdate --install-rosetta --agree-to-license || fail "Could not install Rosetta 2."
fi
/usr/bin/arch -x86_64 /usr/bin/true || fail "Rosetta 2 is unavailable."
# Xcode's system Python is universal; Homebrew's ARM-only Python is unsuitable.
/usr/bin/arch -x86_64 /usr/bin/python3 -c '
import platform, sys
print("Book-test Python:", sys.version, sys.executable, platform.machine(), platform.processor())
if sys.version_info < (3, 8) or platform.machine().lower() != "x86_64":
    sys.exit("Python 3.8+ executing as x86-64 is required")
' || fail "Xcode must provide an x86-64-capable /usr/bin/python3."
