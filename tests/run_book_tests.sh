#!/bin/sh

set -eu

project_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
book_tests_dir=${BOOK_TESTS_DIR:-/Users/zach/Projects/nlsandler/writing-a-c-compiler-tests}
compiler="$project_dir/bin/Debug/net11.0/Compiler"
runner="$book_tests_dir/test_compiler"

if ! command -v python3 >/dev/null 2>&1; then
  printf '%s\n' "Error: python3 is required to run the book test suite." >&2
  exit 1
fi

if [ ! -x "$runner" ]; then
  printf '%s\n' "Error: book test runner not found: $runner" >&2
  printf '%s\n' "Set BOOK_TESTS_DIR to the writing-a-c-compiler-tests checkout." >&2
  exit 1
fi

dotnet build "$project_dir/Compiler.csproj" --no-restore

if [ ! -x "$compiler" ]; then
  printf '%s\n' "Error: compiler executable was not produced: $compiler" >&2
  exit 1
fi

printf '\nRunning tests from Writing a C Compiler...\n\n'

exec python3 "$runner" "$compiler" "$@"
