# Task 05: Package the validated executable

Status: Implemented
Depends on: [Task 01](task-01-native-arm64-publish.md),
[Task 02](task-02-validate-published-binary.md),
[Task 04](task-04-distribution-signing.md)

## Outcome

A repeatable packaging command creates named, verifiable release assets from the
already published and validated executable without rebuilding it.

## Work

- [x] Add a small packaging script. Read version metadata from the project and
  validate any supplied release version against it; avoid duplicate hardcoded versions.
- [x] Stage only `Compiler`, `LICENSE`, required third-party notices for redistributed
  dependencies/runtime components, and a concise installation/readme file.
- [x] Audit applicable .NET and System.CommandLine redistribution notices and include
  the required license texts. Do not assume this repository's MIT license covers all
  redistributed components by itself.
- [x] Create `compiler-0.1.0-osx-arm64.tar.gz` for version 0.1.0, with one predictable
  top-level directory and executable permissions preserved.
- [x] Keep `.dSYM` outside the runtime archive; optionally create a separately named
  symbols archive. Exclude intermediate files, test binaries, and source-build outputs.
- [x] Generate `SHA256SUMS` for final release archives after signing and packaging.
- [x] Extract the archive into a fresh directory, verify its checksum and expected
  contents, and rerun the smoke checks against the extracted compiler.
- [x] Record the source commit, SDK, build OS/Xcode, and upstream book-test commit in
  a manifest or release notes. Do not claim bit-for-bit reproducibility without testing it.

## Acceptance

- [x] The archive contains exactly one runtime executable plus documentation/notices;
  it needs no adjacent DLLs or runtime configuration files.
- [x] Extracted permissions permit direct execution and all smoke checks pass.
- [x] Checksums validate, and the extracted binary matches the validated binary.
- [x] Packaging uses the supplied artifact and never calls `dotnet publish` again.
- [x] Assets are versioned, ARM64-only, and stored under ignored `artifacts/` paths.

## Completion evidence

- `scripts/package-release.py` packages the supplied executable without calling
  a build, restore, or publish command. The version comes from
  `Directory.Build.props`; the optional version and binary report must agree.
- `scripts/publish-release.sh` records build-time provenance and the binary
  hash in adjacent `build-metadata.json`. ARM64 CI retains that file with the
  executable for later packaging. Dependency versions must match the audit in
  [third-party/README.md](../third-party/README.md).
- Clean source commit `d336699b29981374ef7795f64850577f73336702` was published
  on Apple Silicon macOS 27.0.1 (26A434), Xcode 27.1 (27A9275), with SDK
  `11.0.100-rc.1.26425.128`. The binary SHA-256 was
  `3de31b291d2d0da7158a5639e83bef80dceea7de7f1fd651f1a59d6207adc7a6`.
- `dotnet build -m:1 --disable-build-servers` passed with zero warnings/errors;
  `dotnet test` reported Passed: 185, Failed: 0, Skipped: 0.
- Published-executable smoke checks passed. Upstream book tests at
  `ae12014d2dec14488f3f80d14df4b6d8e4634d7d` passed all 24 chapter-one cases
  in each mode: full, lex, parse, and codegen, preserving the executable hash.
- Packaging with `v0.1.0 --symbols` produced
  `artifacts/release/package.x53gpw4_/`. The runtime archive contains exactly
  `Compiler` (mode 0755), `LICENSE`, `README.md`, and
  `THIRD-PARTY-NOTICES.txt` beneath `compiler-0.1.0-osx-arm64/`.
  Both archive checksums passed and the extracted executable passed all smoke
  checks with the original binary hash. Runtime archive SHA-256:
  `3dc063fa5b9c08f7fed58083322b6edf7f453172addc22cb45e3f73028e42fe7`.
- Packaging without symbols also passed with a supplied path containing spaces
  and .NET excluded from `PATH`; success emitted the expected CI output paths.
  Incorrect release version, missing provenance, modified code, missing requested
  symbols, mismatched binary hash, and unaudited dependency version all failed.
- These are local rehearsal assets, not a published release. Final macOS 14/15,
  separate no-.NET-machine, and browser-download checks remain task 08 gates.
