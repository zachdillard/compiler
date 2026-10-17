# Task 06: Document installation and chapter-one support

Status: Implemented; final browser-download verification remains a task 08 gate
Depends on: [Task 01](task-01-native-arm64-publish.md),
[Task 04](task-04-distribution-signing.md),
[Task 05](task-05-package-release-assets.md)

## Outcome

A user can install the binary and compile a chapter-one program without cloning
the repository or installing .NET.

## Work

- [x] Add a binary-installation section to `README.md`, keeping source-build setup
  distinct. Link the GitHub release asset and describe the supported macOS minimum.
- [x] Document checksum verification, archive extraction, optional installation to
  a user-owned directory on `PATH`, and direct `Compiler --version` invocation.
- [x] State Apple Silicon-only compiler support and the macOS x86-64 output target.
  Explain that GCC/Apple Command Line Tools remain necessary for compilation and
  Rosetta is necessary only when executing generated x86-64 programs.
- [ ] Document the chosen signing/notarization status and tested download approval
  procedure. Keep instructions specific to this download rather than system-wide changes.
  The status and Apple per-item instructions are documented; the final browser
  approval test remains task 08 work, so this item is not yet marked complete.
- [x] Include a minimal `int main(void) { return 2; }` example, compile/run commands,
  the expected exit code, and a direct `Compiler -S` example.
- [x] Update `docs/development.md` with publication, artifact validation, packaging,
  manual rehearsal, tagged-release operations, and required build prerequisites.
- [x] Update `CHANGELOG.md` with binary distribution and its limitations. Prepare
  release notes covering chapter-one scope, prerequisites, and known limitations;
  set the release date when publication actually occurs.
- [x] Explain that SDK requirements apply to contributors, whereas the release
  executable includes its runtime. Keep chapter-based versioning unchanged.

## Acceptance

- [x] Commands match actual asset names, archive layout, executable name, and flags.
- [x] A user can follow the binary path without invoking `dotnet` or cloning sources.
- [x] No claim implies dependency-free compilation, native ARM64 generated programs,
  Intel compiler support, untested macOS compatibility, or nonexistent notarization.
- [ ] Task 08 records a successful walkthrough of these instructions using a
  downloaded artifact, including expected program exit status.

## Completion evidence

- `README.md` separates the binary path from source development and links the
  planned archive/checksum assets. It documents the exact top-level directory,
  executable, checksum selection when symbols are optional, per-item approval,
  user-owned installation, a program returning `2`, and direct `-S` usage.
- `docs/development.md` documents prerequisites, one-time publication, supplied-
  artifact validation/packaging, local and future workflow rehearsals, and future
  tag/draft promotion operations. It explicitly identifies task 07's release
  workflow as unimplemented, rather than claiming unavailable automation works.
- `CHANGELOG.md` remains `0.1.0 — Unreleased`. Draft notes in
  `docs/releases/v0.1.0.md` describe chapter-one scope, runtime/user prerequisites,
  notices, unsigned status, assets, and pending release evidence with no invented
  publication date. Chapter-based versioning is unchanged.
- On 2026-10-17 the README's checksum, extraction, version, compile/run, and
  assembly-only command blocks were executed against the task 05 local archive
  with .NET excluded from `PATH`. Checksum verification printed `OK`, the binary
  reported `Compiler 0.1.0`, the x86-64 program exited with `2`, and `-S` produced
  assembly with no `.i` left behind. The optional PATH installation also passed
  in an isolated user-install directory, retaining the license and notices.
- Relative links in the changed guides and release notes were checked. The
  repository has no `v0.1.0` tag or release; asset links are explicitly planned.
- This local archive walkthrough does not prove browser quarantine approval,
  absence of .NET on the host, or macOS 14/15 compatibility. The unchecked items
  above remain task 08's final downloaded-artifact gates and are called out in
  the user guide, distribution policy, and draft notes.
