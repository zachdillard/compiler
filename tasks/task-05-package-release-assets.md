# Task 05: Package the validated executable

Status: Planned  
Depends on: [Task 01](task-01-native-arm64-publish.md),
[Task 02](task-02-validate-published-binary.md),
[Task 04](task-04-distribution-signing.md)

## Outcome

A repeatable packaging command creates named, verifiable release assets from the
already published and validated executable without rebuilding it.

## Work

- [ ] Add a small packaging script. Read version metadata from the project and
  validate any supplied release version against it; avoid duplicate hardcoded versions.
- [ ] Stage only `Compiler`, `LICENSE`, required third-party notices for redistributed
  dependencies/runtime components, and a concise installation/readme file.
- [ ] Audit applicable .NET and System.CommandLine redistribution notices and include
  the required license texts. Do not assume this repository's MIT license covers all
  redistributed components by itself.
- [ ] Create `compiler-0.1.0-osx-arm64.tar.gz` for version 0.1.0, with one predictable
  top-level directory and executable permissions preserved.
- [ ] Keep `.dSYM` outside the runtime archive; optionally create a separately named
  symbols archive. Exclude intermediate files, test binaries, and source-build outputs.
- [ ] Generate `SHA256SUMS` for final release archives after signing and packaging.
- [ ] Extract the archive into a fresh directory, verify its checksum and expected
  contents, and rerun the smoke checks against the extracted compiler.
- [ ] Record the source commit, SDK, build OS/Xcode, and upstream book-test commit in
  a manifest or release notes. Do not claim bit-for-bit reproducibility without testing it.

## Acceptance

- [ ] The archive contains exactly one runtime executable plus documentation/notices;
  it needs no adjacent DLLs or runtime configuration files.
- [ ] Extracted permissions permit direct execution and all smoke checks pass.
- [ ] Checksums validate, and the extracted binary matches the validated binary.
- [ ] Packaging uses the supplied artifact and never calls `dotnet publish` again.
- [ ] Assets are versioned, ARM64-only, and stored under ignored `artifacts/` paths.
