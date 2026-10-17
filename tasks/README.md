# Chapter-one binary release

Deliver `v0.1.0` as one native Apple Silicon compiler executable. The compiler
continues to emit macOS x86-64 assembly and executables. Binary users do not need
.NET; compilation still needs Apple's Command Line Tools (`gcc` on `PATH`).
Rosetta 2 is required to execute generated programs and the upstream book-test
runner's x86-64 environment, not to run the ARM64 compiler itself.

These files describe planned work. Creating them does not implement the release
pipeline or publish a release. Complete the checkboxes with evidence as work lands.

| Task | Deliverable | Depends on |
| --- | --- | --- |
| [01 — Publish](task-01-native-arm64-publish.md) | Repeatable Native AOT publication | None |
| [02 — Artifact validation](task-02-validate-published-binary.md) | Smoke checks and book tests against the published executable | 01 |
| [03 — ARM64 CI](task-03-arm64-release-validation-ci.md) | Automated artifact validation on Apple Silicon | 01, 02 |
| [04 — Distribution signing](task-04-distribution-signing.md) | Explicit unsigned policy or signing/notarization implementation | 01 |
| [05 — Packaging](task-05-package-release-assets.md) | Versioned archive, notices, symbols, and checksum | 01, 02, 04 |
| [06 — Documentation](task-06-document-binary-installation.md) | Installation guide, support statement, and release notes | 01, 04, 05 |
| [07 — Release workflow](task-07-automate-draft-release.md) | Tag/manual workflow creating a validated draft release | 03, 05, 06 |
| [08 — First release](task-08-publish-chapter-one.md) | Rehearsal, download validation, and publication of `v0.1.0` | 01–07 |

Task 04 requires an explicit distribution choice; Developer ID signing and
notarization are optional for this learning project's first release. Other tasks
must support the chosen policy without silently claiming the binary is notarized.

## Existing evidence

- The exploratory Native AOT artifact was approximately 3.6 MB and identified as
  a thin ARM64 Mach-O executable.
- Copied alone, it passed help/version, stage, compilation, invalid-input, and
  cleanup smoke checks. Its generated x86-64 executable returned the expected code.
- Its Mach-O metadata declared macOS 14.0 as the deployment minimum. This is not
  evidence that the artifact works on that OS; the supported minimum needs testing.
- Existing CI runs on Intel. Driver integration tests invoke the compiler DLL
  through `dotnet`, and the book-test wrapper builds its own compiler executable.
- On 2026-10-17, `xcodebuild -version` succeeded with Xcode 27.1, build 27A9275;
  the selected developer directory was `/Applications/Xcode.app/Contents/Developer`.
- With that Xcode selection, rerunning the exploratory Native AOT publish command
  completed with exit code 0. The x86-64 Python probe also completed successfully
  and reported `x86_64 i386`. Both earlier toolchain errors are resolved locally;
  clean-checkout publication and the full published-artifact suite remain tasks.

## Scope

- Keep compiler stages and the existing x86-64 target unchanged.
- Publish only `osx-arm64`; do not add Intel or universal compiler downloads.
- Preserve normal source development and existing Intel test coverage.
- Keep all generated artifacts under ignored `artifacts/` directories.
- A single executable may be distributed in an archive with license/readme text;
  optional debug symbols are a separate asset, not a runtime dependency.
