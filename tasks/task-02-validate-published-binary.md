# Task 02: Validate the actual published executable

Status: Planned  
Depends on: [Task 01](task-01-native-arm64-publish.md)

## Outcome

Reusable validation accepts a published executable path and tests that artifact
without rebuilding it or substituting the compiler DLL.

## Work

- [ ] Add `tests/release_smoke.sh` or an equivalently small script accepting an
  executable path. Resolve it before changing directories and quote all paths.
- [ ] Copy only the executable into a temporary directory. Use temporary C inputs,
  including a path containing spaces; clean up the temporary directory on exit.
- [ ] Assert that the compiler contains only ARM64 code. Run it directly, without
  `dotnet` or an x86-64 wrapper, with .NET excluded from the test's `PATH`.
- [ ] Check `--help`, the expected `--version`, each stage flag, invalid input,
  conflicting flags, and the missing-GCC diagnostic.
- [ ] Check that `-S` emits x86-64 assembly, removes `.i`, and leaves no executable.
  Link that assembly and assert the output architecture and expected exit code.
- [ ] Check normal compilation, x86-64 preprocessing macros, output architecture,
  expected program exit code, and `.i`/`.s` cleanup. Handle expected nonzero program
  exit codes explicitly so shell `set -e` does not misreport them as test failures.
- [ ] Extend `tests/book_tests.sh` with an optional `COMPILER_PATH` override. When
  supplied, validate and use it without building. Preserve current default behavior,
  `BOOK_TESTS_DIR`, `BUILD_CONFIGURATION`, and forwarded runner options.
- [ ] Provide a way to select a compatible Python invocation for the upstream
  runner. Verify x86-64 Python under Rosetta independently; do not assume putting
  an ARM-only Python inside an x86-64 shell makes Python x86-64.
- [ ] Run chapter 1 in full and with lex, parse, and codegen stages against the same
  published artifact. Record the upstream test commit and results.

## Acceptance

- [ ] Smoke checks fail on the wrong compiler/output architecture, incorrect exit
  codes, missing output, unexpected files, or CLI failure.
- [ ] Book tests execute the provided release binary without modifying it or
  rebuilding it; its checksum before and after validation matches.
- [ ] All four book-suite modes pass. Rosetta/Python prerequisite failures are
  reported as prerequisite failures, not compiler regressions.
- [ ] Existing source-build book-test usage continues to work.
- [ ] A clean-machine check without installed .NET is included in task 08; changing
  `PATH` alone is not described as proof that .NET is absent from the machine.
