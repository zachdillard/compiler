# Task 03: Add Apple Silicon artifact validation to CI

Status: Implemented; hosted macOS 14/15 run pending
Depends on: [Task 01](task-01-native-arm64-publish.md),
[Task 02](task-02-validate-published-binary.md)

## Outcome

Changes affecting releases automatically prove that an ARM64 compiler can produce
working x86-64 programs, while existing Intel Debug/Release coverage remains intact.

## Work

- [x] Extend `.github/workflows/compiler-tests.yml` or add a focused validation
  workflow. Use a currently supported, explicitly versioned ARM64 macOS runner
  such as `macos-15`; verify the label's architecture during implementation.
- [x] Assert ARM64 execution, install the pinned SDK through `global.json`, and
  report Xcode, SDK, GCC, Python, and OS versions in the job log.
- [x] Verify Rosetta and a working x86-64 Python for upstream book tests. Install
  missing runner prerequisites explicitly and fail clearly if unavailable.
- [x] Run the existing project tests, publish through task 01's command, then run
  task 02's smoke checks and all four chapter-one book-suite modes.
- [x] Pin the upstream book-test checkout to a reviewed commit and record it in
  logs so future upstream changes cannot silently alter a tagged release's checks.
- [x] Include release scripts, validation scripts, publish settings, and workflow
  files in trigger paths. Avoid maintaining separate copies of release commands.
- [x] Upload diagnostic logs and the validated artifact for inspection. Give this
  validation workflow read-only repository permissions.
- [ ] Validate the minimum supported macOS version from task 01 on an appropriate
  runner, or record a concrete manual-machine validation requirement for task 08.
  The chosen minimum is macOS 14.0 (Sonoma): test macOS 14 and the selected newer
  ARM64 CI OS. If macOS 14 is unavailable in CI, require an Apple Silicon Mac
  running macOS 14 for task 08's final downloaded-artifact checks.

## Acceptance

- [x] A pull request changing publication or artifact checks triggers ARM64 CI.
- [ ] Logs show ARM64 compiler execution and x86-64 generated-program execution.
- [ ] Existing tests, release smoke checks, and book tests pass; no relevant stage
  is skipped because of an unhandled architecture mismatch.
- [x] The CI result can be reused by the tagged release workflow without assuming
  that a green check on another commit validates the release commit.

## Implementation and local evidence

- Added a read-only reusable workflow called by Compiler Tests, with a manual
  trigger. It checks out and asserts the caller's exact `github.sha`, runs Release
  project tests on ARM64 `macos-15`, and calls the existing publish/smoke/book scripts.
  The publish script exposes its unique output paths via GitHub step outputs.
- GitHub's [runner reference](https://docs.github.com/en/actions/reference/runners/github-hosted-runners)
  and [runner images](https://github.com/actions/runner-images) list `macos-14`
  and `macos-15` as standard ARM64 runners (verified 2026-10-17). The workflow
  asserts both architecture and OS version. It installs missing Rosetta explicitly
  and requires Xcode's working x86-64 Python, with clear prerequisite failures.
- The macOS 14 job downloads the exact macOS 15 executable and runs smoke checks,
  without publishing again. Diagnostics survive failed steps; the caller receives
  the validated executable artifact name only after the reusable workflow succeeds.
  Task 08 still checks the final downloaded assets on separate Apple Silicon
  macOS 14 and 15 machines without .NET installed.
- Intel Debug/Release jobs remain. Both architectures pin book tests to
  `ae12014d2dec14488f3f80d14df4b6d8e4634d7d`, reviewed and exercised in task 02,
  and record that SHA in logs. Trigger paths now include `scripts/**` and all
  workflow files, alongside existing source/publish settings and tests.
- Local checks on 2026-10-17: `actionlint` 1.7.12 passed for both workflows;
  YAML parsing, `bash -n`, and `git diff --check` passed. The runner prerequisite
  script passed with expected major version 27 on this ARM64 macOS 27.0.1 host.
- `dotnet build -m:1 --disable-build-servers`: zero warnings/errors.
  `dotnet test --no-build -m:1 --disable-build-servers`:
  **Passed: 185, Failed: 0, Skipped: 0**, outside the sandbox for the VSTest socket.
- `./scripts/publish-release.sh` succeeded using the pinned SDK, Xcode 27.1,
  and macOS SDK 27.0. It retained the known SDK duplicate debug-map warnings
  documented in task 01; no compiler/AOT/trimming warning occurred.
  Smoke checks passed and explicitly logged ARM64 compiler execution plus x86-64
  generated programs returning 37 and 42. All four book modes passed **24 tests
  each** against that binary, with unchanged SHA-256
  `cd05c6f3ddd903c3da2aabcd24ed7ca3efc59f657cea87568b6b988c3e05c2a9`.
- Hosted workflow execution and artifact upload/download have not been run from
  this checkout. The remaining acceptance boxes require a real GitHub Actions run
  on macOS 14/15; local macOS 27 results do not establish those OS results.
