# Changelog

## 0.1.0 — Unreleased

### Added

- Native AOT Apple Silicon (`osx-arm64`) distribution with its .NET runtime
  included, standalone artifact smoke checks, and ARM64 release validation CI.
- Versioned runtime archive, optional separate debug symbols, SHA-256 checksums,
  build provenance, and bundled .NET/System.CommandLine licenses and notices.
- Binary installation instructions, an explicit unsigned/non-notarized
  distribution policy, and draft chapter-one release notes. Publication and
  final download validation remain tasks 07–08; no release date is set yet.
- `--parse` and `--codegen` stop after parsing and assembly AST generation without
  emitting output files. Stage flags (`--lex`, `--parse`, `--codegen`, and `-S`)
  are mutually exclusive.
- Intermediate-stage book tests in CI and assembly-only output validation through
  GCC assembly, linking, and execution.

- Chapter one custom compiler: a single `int` function with `void` parameters
  returning an integer constant, emitted as macOS x86-64 assembly or an executable.
- Shared version metadata in `Directory.Build.props` and `--version`, which
  reports the compiler version without requiring GCC.
- MIT license and chapter-based versioning policy.

### Fixed

- Inputs without a `.c` extension are rejected before preprocessing, preventing
  intermediate-file cleanup from deleting an input `.i` file.
- Preprocessing selects the x86-64 target on macOS so architecture macros match
  the generated executable on Apple Silicon.
- CI also runs when root build configuration or the test workflow changes.
- Out-of-range integer constants report a diagnostic and exit with status `1`
  instead of crashing, including in lex-only mode.
- Partial preprocessed output is cleaned up after preprocessing failures.
- File access and intermediate-file cleanup exceptions, including assembly
  cleanup exceptions, report diagnostics instead of crashing.

### Limitations

- Later chapter features and extra credit are not implemented.
- GCC is required for preprocessing, assembly, and linking.
- The custom compiler generates macOS x86-64 programs; Apple Silicon needs Rosetta.
- Source builds require the pinned .NET 11 RC1 SDK.
- Binary distribution is Apple Silicon-only; no Intel or universal compiler
  archive is provided. Its intended minimum is macOS 14.0, pending runtime and
  final downloaded-artifact checks on macOS 14 and 15 before publication.
- Distribution has only the linker's ad hoc signature; Developer ID signing and
  notarization are deferred. Browser downloads may require per-item approval in
  macOS Privacy & Security settings. The final approval walkthrough is pending.
