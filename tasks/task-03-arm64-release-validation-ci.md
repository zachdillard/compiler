# Task 03: Add Apple Silicon artifact validation to CI

Status: Planned  
Depends on: [Task 01](task-01-native-arm64-publish.md),
[Task 02](task-02-validate-published-binary.md)

## Outcome

Changes affecting releases automatically prove that an ARM64 compiler can produce
working x86-64 programs, while existing Intel Debug/Release coverage remains intact.

## Work

- [ ] Extend `.github/workflows/compiler-tests.yml` or add a focused validation
  workflow. Use a currently supported, explicitly versioned ARM64 macOS runner
  such as `macos-15`; verify the label's architecture during implementation.
- [ ] Assert ARM64 execution, install the pinned SDK through `global.json`, and
  report Xcode, SDK, GCC, Python, and OS versions in the job log.
- [ ] Verify Rosetta and a working x86-64 Python for upstream book tests. Install
  missing runner prerequisites explicitly and fail clearly if unavailable.
- [ ] Run the existing project tests, publish through task 01's command, then run
  task 02's smoke checks and all four chapter-one book-suite modes.
- [ ] Pin the upstream book-test checkout to a reviewed commit and record it in
  logs so future upstream changes cannot silently alter a tagged release's checks.
- [ ] Include release scripts, validation scripts, publish settings, and workflow
  files in trigger paths. Avoid maintaining separate copies of release commands.
- [ ] Upload diagnostic logs and the validated artifact for inspection. Give this
  validation workflow read-only repository permissions.
- [ ] Validate the minimum supported macOS version from task 01 on an appropriate
  runner, or record a concrete manual-machine validation requirement for task 08.
  The chosen minimum is macOS 14.0 (Sonoma): test macOS 14 and the selected newer
  ARM64 CI OS. If macOS 14 is unavailable in CI, require an Apple Silicon Mac
  running macOS 14 for task 08's final downloaded-artifact checks.

## Acceptance

- [ ] A pull request changing publication or artifact checks triggers ARM64 CI.
- [ ] Logs show ARM64 compiler execution and x86-64 generated-program execution.
- [ ] Existing tests, release smoke checks, and book tests pass; no relevant stage
  is skipped because of an unhandled architecture mismatch.
- [ ] The CI result can be reused by the tagged release workflow without assuming
  that a green check on another commit validates the release commit.
