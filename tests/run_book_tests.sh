#!/bin/sh
set -eu

book_tests_dir=${BOOK_TESTS_DIR:-../writing-a-c-compiler-tests}

dotnet build src/Compiler.csproj --output artifacts/book-tests

exec python3 "$book_tests_dir/test_compiler" \
  "$PWD/artifacts/book-tests/Compiler" "$@"
