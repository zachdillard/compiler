# Task 02: Validate the actual published executable

Status: Complete

Depends on: [Task 01](task-01-native-arm64-publish.md)

## Outcome

Reusable validation accepts a published executable path and tests that artifact
without rebuilding it or substituting the compiler DLL.

## Work

- [x] Add `tests/release_smoke.sh` or an equivalently small script accepting an
  executable path. Resolve it before changing directories and quote all paths.
- [x] Copy only the executable into a temporary directory. Use temporary C inputs,
  including a path containing spaces; clean up the temporary directory on exit.
- [x] Assert that the compiler contains only ARM64 code. Run it directly, without
  `dotnet` or an x86-64 wrapper, with .NET excluded from the test's `PATH`.
- [x] Check `--help`, the expected `--version`, each stage flag, invalid input,
  conflicting flags, and the missing-GCC diagnostic.
- [x] Check that `-S` emits x86-64 assembly, removes `.i`, and leaves no executable.
  Link that assembly and assert the output architecture and expected exit code.
- [x] Check normal compilation, x86-64 preprocessing macros, output architecture,
  expected program exit code, and `.i`/`.s` cleanup. Handle expected nonzero program
  exit codes explicitly so shell `set -e` does not misreport them as test failures.
- [x] Extend `tests/book_tests.sh` with an optional `COMPILER_PATH` override. When
  supplied, validate and use it without building. Preserve current default behavior,
  `BOOK_TESTS_DIR`, `BUILD_CONFIGURATION`, and forwarded runner options.
- [x] Provide a way to select a compatible Python invocation for the upstream
  runner. Verify x86-64 Python under Rosetta independently; do not assume putting
  an ARM-only Python inside an x86-64 shell makes Python x86-64.
- [x] Run chapter 1 in full and with lex, parse, and codegen stages against the same
  published artifact. Record the upstream test commit and results.

## Acceptance

- [x] Smoke checks fail on the wrong compiler/output architecture, incorrect exit
  codes, missing output, unexpected files, or CLI failure.
- [x] Book tests execute the provided release binary without modifying it or
  rebuilding it; its checksum before and after validation matches.
- [x] All four book-suite modes pass. Rosetta/Python prerequisite failures are
  reported as prerequisite failures, not compiler regressions.
- [x] Existing source-build book-test usage continues to work.
- [x] A clean-machine check without installed .NET is included in task 08; changing
  `PATH` alone is not described as proof that .NET is absent from the machine.

## Completion evidence

Validated on 2026-10-17 on ARM64 macOS with the existing published Native AOT
`Compiler` at `/Users/zach/Projects/compiler/artifacts/release/osx-arm64/Compiler`.
No compiler production code changed and this validation did not republish it.

- `./tests/release_smoke.sh <published-executable>` passed all six named groups:
  help/version; stages and paths with spaces; invalid input and conflicting flags;
  missing GCC; assembly-only output/architecture/exit code; normal compilation,
  preprocessing macros, architecture, and cleanup. Generated programs contained
  only x86-64 code and returned `37` and `42`, respectively. The compiler contained
  only ARM64 code and reported `Compiler 0.1.0`.
- The smoke script resolves the executable before changing directories, copies
  only that file, runs it directly with `PATH=/usr/bin:/bin:/usr/sbin:/sbin`, and
  checks exact directory contents. No DLL or `dotnet` invocation is substituted.
- Upstream tests: `nlsandler/writing-a-c-compiler-tests` commit
  `ae12014d2dec14488f3f80d14df4b6d8e4634d7d`. Used a clean `git archive` export
  under ignored `artifacts/upstream book tests/` because the sandbox cannot write
  beside inputs in the original external checkout.
- `COMPILER_PATH=<published-executable> BOOK_TESTS_DIR=<export>` with
  `./tests/book_tests.sh --chapter 1`: **24 tests, OK**. The same command with
  `--stage lex`, `--stage parse`, and `--stage codegen`: **24 tests, OK each**.
- The original artifact's SHA-256 before and after smoke and every book mode:
  `e5ea0d0825b5b1565292376d01015a1511c41422bc0c14ecb319f28df7314259`.
  The wrappers compare and report this, including when the runner fails.
- Python was independently launched with `/usr/bin/arch -x86_64 /usr/bin/python3`:
  Python `3.9.6`, machine `x86_64`, processor `i386`. The wrapper repeats a
  version/architecture probe before running the upstream suite and reports
  Python/Rosetta prerequisite failures separately with exit status 2.
- Additional full chapter-one run: passed 24 tests with .NET excluded from PATH,
  `BUILD_CONFIGURATION=DoNotBuild`, and a relative `COMPILER_PATH` containing
  spaces pointing to that same artifact. This verifies the build is bypassed.
- Default source-build behavior: `BOOK_TESTS_DIR=<export>
  ./tests/book_tests.sh --chapter 1 --stage lex` built Debug and passed 24 tests.
  `BUILD_CONFIGURATION` selection and forwarded runner arguments remain intact.
- Negative checks rejected an incorrect expected version and a non-ARM64-only
  compiler; rejected an unusable Python and missing compiler override as
  prerequisite failures; forwarded an invalid upstream option and preserved
  its failing exit status while still checking the artifact checksum.
- `bash -n tests/release_smoke.sh tests/book_tests.sh` and `git diff --check`
  passed. `dotnet build -m:1 --disable-build-servers` passed with zero warnings
  and errors. `dotnet test --no-build -m:1 --disable-build-servers`:
  **Passed: 185, Failed: 0, Skipped: 0**, using elevated execution for VSTest's
  local communication socket.
- These checks exclude .NET from PATH; they do not establish that it is absent
  from the machine. Task 08 retains the separate clean-machine requirement.
