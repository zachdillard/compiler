#!/bin/bash
set -euo pipefail

book_tests_dir=${BOOK_TESTS_DIR:-../writing-a-c-compiler-tests}
python=${BOOK_TESTS_PYTHON:-python3}

prerequisite() {
  echo "Prerequisite failure: $*" >&2
  exit 2
}

[[ -f "$book_tests_dir/test_compiler" ]] || prerequisite "Set BOOK_TESTS_DIR to the upstream test checkout."
python=$(command -v "$python") || prerequisite "Set BOOK_TESTS_PYTHON to a Python 3.8+ executable."
# Launch Python itself as x86-64 on macOS, rather than just its parent shell.
python_command=("$python")
if [[ $(uname -s) == Darwin ]]; then
  /usr/bin/arch -x86_64 /usr/bin/true || prerequisite "Rosetta 2 is required on Apple Silicon."
  python_command=(/usr/bin/arch -x86_64 "$python")
fi
"${python_command[@]}" -c '
import platform, sys
machine, processor = platform.machine().lower(), platform.processor().lower()
if sys.version_info < (3, 8):
    sys.exit("Python 3.8+ is required")
# Some macOS Python versions report the host machine under Rosetta.
translated = sys.platform == "darwin" and machine == "arm64" and processor == "i386"
if machine not in ("x86_64", "amd64") and not translated:
    sys.exit("Python must run as x86-64")
print("Book-test Python: " + sys.executable + " (" + machine + ", " + processor + ")")
' || prerequisite "Select an x86-64-capable Python with BOOK_TESTS_PYTHON; an ARM-only Python is insufficient."

if [[ -n ${COMPILER_PATH:-} ]]; then
  [[ -f "$COMPILER_PATH" && -x "$COMPILER_PATH" ]] || prerequisite "COMPILER_PATH must name an executable file."
  compiler=$(cd "$(dirname "$COMPILER_PATH")" && pwd)/$(basename "$COMPILER_PATH")
else
  dotnet build src/Compiler.csproj \
    --configuration "${BUILD_CONFIGURATION:-Debug}" \
    --output artifacts/book-tests -m:1 -p:UseSharedCompilation=false
  compiler="$PWD/artifacts/book-tests/Compiler"
fi

before=$(shasum -a 256 "$compiler")
status=0
"${python_command[@]}" "$book_tests_dir/test_compiler" "$compiler" "$@" || status=$?
after=$(shasum -a 256 "$compiler")
[[ "$before" == "$after" ]] || { echo "Error: compiler checksum changed during book tests." >&2; exit 1; }
echo "Compiler SHA-256: ${after%% *} (unchanged)"
exit "$status"
