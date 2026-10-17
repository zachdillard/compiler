#!/bin/bash
set -euo pipefail

fail() { echo "FAIL: $*" >&2; exit 1; }
prerequisite() { echo "Prerequisite failure: $*" >&2; exit 2; }
[[ $# == 1 ]] || fail "Usage: $0 <published-executable> (optional EXPECTED_VERSION override)"
[[ -f "$1" && -x "$1" ]] || fail "Expected an executable file: $1"
compiler=$(cd "$(dirname "$1")" && pwd)/$(basename "$1")
repo_root=$(cd "$(dirname "$0")/.." && pwd)
expected_version=${EXPECTED_VERSION:-$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$repo_root/Directory.Build.props")}
[[ -n "$expected_version" ]] || fail "Expected version is empty."
[[ $(uname -s) == Darwin ]] || prerequisite "Release validation requires macOS."

# Use only macOS tools. This excludes dotnet from PATH, but does not prove that
# .NET is absent from the machine; task 08 covers that separate installation check.
export PATH=/usr/bin:/bin:/usr/sbin:/sbin
if command -v dotnet >/dev/null; then fail "Test PATH must exclude dotnet."; fi
for tool in gcc lipo shasum; do
  command -v "$tool" >/dev/null || prerequisite "Required tool is missing: $tool"
done
/usr/bin/arch -x86_64 /usr/bin/true || prerequisite "Rosetta 2 is required on Apple Silicon."
[[ $(lipo -archs "$compiler") == arm64 ]] || fail "Compiler must contain only ARM64 code."
before=$(shasum -a 256 "$compiler")
work=$(mktemp -d "${TMPDIR:-/tmp}/compiler-release.XXXXXX")
trap 'rm -rf "$work"' EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
cp "$compiler" "$work/Compiler"
cd "$work"

expect_status() {
  local expected=$1 actual=0
  shift
  "$@" >"$work/stdout" 2>"$work/stderr" || actual=$?
  if [[ $actual != "$expected" ]]; then
    cat "$work/stdout" "$work/stderr" >&2
    fail "Expected exit $expected, got $actual: $*"
  fi
}
contains() { grep -Fq -- "$2" "$1" || fail "Missing '$2' in $1"; }
only_files() {
  local actual expected
  actual=$(find "$1" -mindepth 1 -maxdepth 1 -exec basename {} \; | sort)
  shift
  expected=$(printf '%s\n' "$@" | sort)
  [[ "$actual" == "$expected" ]] || fail "Unexpected files: expected [$expected], got [$actual]"
}
check_program() {
  [[ -x "$1" ]] || fail "Missing executable: $1"
  [[ $(lipo -archs "$1") == x86_64 ]] || fail "Generated program must contain only x86-64 code."
  expect_status "$2" "$1"
}

expect_status 0 ./Compiler --help
for text in Usage --version --lex --parse --codegen -S; do contains stdout "$text"; done
expect_status 0 ./Compiler --version
[[ $(cat stdout) == "Compiler $expected_version" ]] || fail "Incorrect version."
echo "PASS: help and version"

mkdir "input with spaces"
source_file="$work/input with spaces/return value.c"
printf 'int main(void) { return 37; }\n' >"$source_file"
for stage in --lex --parse --codegen; do
  expect_status 0 ./Compiler "$stage" "$source_file"
  only_files "input with spaces" 'return value.c'
done
echo "PASS: stages and paths with spaces"

for first in --lex --parse --codegen -S; do
  for second in --lex --parse --codegen -S; do
    [[ "$first" != "$second" ]] || continue
    expect_status 1 ./Compiler "$first" "$second" "$source_file"
    contains stderr 'Usage:'
    only_files "input with spaces" 'return value.c'
  done
done
expect_status 1 ./Compiler
contains stderr 'Usage:'
expect_status 1 ./Compiler --unknown "$source_file"
contains stderr 'Usage:'
expect_status 1 ./Compiler "input with spaces/missing.c"
contains stderr 'error:'
expect_status 1 ./Compiler "input with spaces/source.txt"
contains stderr 'Error: source file must have a .c extension.'
printf 'int main(void) { return @; }\n' >"input with spaces/invalid.c"
for stage in --lex --parse --codegen -S ''; do
  if [[ -n "$stage" ]]; then
    expect_status 1 ./Compiler "$stage" "input with spaces/invalid.c"
  else
    expect_status 1 ./Compiler "input with spaces/invalid.c"
  fi
  contains stderr 'Error:'
  only_files "input with spaces" 'return value.c' invalid.c
done
rm "input with spaces/invalid.c"
printf 'int main(void) { return ; }\n' >"input with spaces/invalid.c"
for stage in --parse --codegen -S ''; do
  if [[ -n "$stage" ]]; then
    expect_status 1 ./Compiler "$stage" "input with spaces/invalid.c"
  else
    expect_status 1 ./Compiler "input with spaces/invalid.c"
  fi
  contains stderr 'Error:'
  only_files "input with spaces" 'return value.c' invalid.c
done
rm "input with spaces/invalid.c"
echo "PASS: invalid input and conflicting flags"

mkdir empty-path
expect_status 1 /usr/bin/env PATH="$work/empty-path" ./Compiler "$source_file"
contains stderr 'Error: could not start gcc. Ensure gcc is installed and available on PATH.'
only_files "input with spaces" 'return value.c'
echo "PASS: missing GCC diagnostic"

expect_status 0 ./Compiler -S "$source_file"
assembly_file="${source_file%.c}.s"
only_files "input with spaces" 'return value.c' 'return value.s'
contains "$assembly_file" '%eax'
contains "$assembly_file" 'ret'
gcc -arch x86_64 "$assembly_file" -o "${source_file%.c}"
check_program "${source_file%.c}" 37
rm "$assembly_file" "${source_file%.c}"
echo "PASS: assembly-only output, architecture and exit code"

# ARM64 preprocessing would hit #error; verify the compiler requests x86-64 GCC.
cat >"$source_file" <<'EOF'
#if !defined(__x86_64__) || defined(__arm64__) || defined(__aarch64__)
#error Expected x86-64 preprocessing
#endif
int main(void) { return 42; }
EOF
expect_status 0 ./Compiler "$source_file"
only_files "input with spaces" 'return value.c' 'return value'
check_program "${source_file%.c}" 42
only_files "$work" Compiler stdout stderr 'input with spaces' empty-path
only_files empty-path
echo "PASS: normal compilation, preprocessing macros, architecture and cleanup"

after=$(shasum -a 256 "$compiler")
[[ "$before" == "$after" ]] || fail "Original compiler checksum changed."
[[ $(shasum -a 256 ./Compiler | cut -d ' ' -f 1) == "${before%% *}" ]] || fail "Copied compiler checksum changed."
echo "Compiler SHA-256: ${after%% *} (unchanged)"
echo "Release smoke checks passed."
