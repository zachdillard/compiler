# Changelog

## 0.1.0 — Unreleased

### Added

- Source locations, caret ranges, expected/actual syntax errors, related
  delimiter notes, unsupported-operator messages, and detailed numeric and
  command-line errors. Preprocessor line markers preserve original file/line
  locations, including included files.
- Chapter one custom compiler: a single `int` function with `void` parameters
  returning an integer constant, emitted as macOS x86-64 assembly or an executable.
- Shared version metadata in `Directory.Build.props` and `--version`, which
  reports the compiler version without requiring GCC.
- MIT license and chapter-based versioning policy.

### Fixed

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
