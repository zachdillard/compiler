# Task 06: Document installation and chapter-one support

Status: Planned  
Depends on: [Task 01](task-01-native-arm64-publish.md),
[Task 04](task-04-distribution-signing.md),
[Task 05](task-05-package-release-assets.md)

## Outcome

A user can install the binary and compile a chapter-one program without cloning
the repository or installing .NET.

## Work

- [ ] Add a binary-installation section to `README.md`, keeping source-build setup
  distinct. Link the GitHub release asset and describe the supported macOS minimum.
- [ ] Document checksum verification, archive extraction, optional installation to
  a user-owned directory on `PATH`, and direct `Compiler --version` invocation.
- [ ] State Apple Silicon-only compiler support and the macOS x86-64 output target.
  Explain that GCC/Apple Command Line Tools remain necessary for compilation and
  Rosetta is necessary only when executing generated x86-64 programs.
- [ ] Document the chosen signing/notarization status and tested download approval
  procedure. Keep instructions specific to this download rather than system-wide changes.
- [ ] Include a minimal `int main(void) { return 2; }` example, compile/run commands,
  the expected exit code, and a direct `Compiler -S` example.
- [ ] Update `docs/development.md` with publication, artifact validation, packaging,
  manual rehearsal, tagged-release operations, and required build prerequisites.
- [ ] Update `CHANGELOG.md` with binary distribution and its limitations. Prepare
  release notes covering chapter-one scope, prerequisites, and known limitations;
  set the release date when publication actually occurs.
- [ ] Explain that SDK requirements apply to contributors, whereas the release
  executable includes its runtime. Keep chapter-based versioning unchanged.

## Acceptance

- [ ] Commands match actual asset names, archive layout, executable name, and flags.
- [ ] A user can follow the binary path without invoking `dotnet` or cloning sources.
- [ ] No claim implies dependency-free compilation, native ARM64 generated programs,
  Intel compiler support, untested macOS compatibility, or nonexistent notarization.
- [ ] Task 08 records a successful walkthrough of these instructions using a
  downloaded artifact, including expected program exit status.
