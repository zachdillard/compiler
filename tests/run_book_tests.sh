#!/bin/sh

set -eu

project_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
book_tests_dir=${BOOK_TESTS_DIR:-"$project_dir/../writing-a-c-compiler-tests"}
compiler="$project_dir/src/bin/Debug/net11.0/Compiler"
runner="$book_tests_dir/test_compiler"

for tool in dotnet gcc python3; do
  if ! command -v "$tool" >/dev/null 2>&1; then
    printf '%s\n' "Error: $tool is required to run the book test suite. See README.md for setup." >&2
    exit 1
  fi
done

if [ ! -x "$runner" ]; then
  printf '%s\n' "Error: book test runner not found: $runner" >&2
  printf '%s\n' "From the compiler repository root, run:" >&2
  printf '%s\n' "  git clone https://github.com/nlsandler/writing-a-c-compiler-tests.git ../writing-a-c-compiler-tests" >&2
  printf '%s\n' "Set BOOK_TESTS_DIR to the writing-a-c-compiler-tests checkout." >&2
  exit 1
fi

dotnet build "$project_dir/src/Compiler.csproj"

if [ ! -x "$compiler" ]; then
  printf '%s\n' "Error: compiler executable was not produced: $compiler" >&2
  exit 1
fi

printf '\nRunning tests from Writing a C Compiler...\n\n'

exec python3 "$runner" "$compiler" "$@"
