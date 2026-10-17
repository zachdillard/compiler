# Task 08: Rehearse and publish the first chapter release

Status: Planned  
Depends on: Tasks 01–07

## Outcome

GitHub release `v0.1.0` contains a verified Apple Silicon compiler binary, installation
instructions, release notes, and checksums.

## Work

- [ ] Confirm tasks 01–07 are complete, including the signing policy and supported
  macOS decision. Record the release candidate commit and expected version `0.1.0`.
- [ ] Run the manual workflow rehearsal. Inspect logs for SDK/Xcode versions,
  architecture assertions, project tests, smoke checks, and all book-suite modes.
- [ ] Download and extract the rehearsal archive in a separate Apple Silicon
  environment without .NET installed. Install only documented user prerequisites.
- [ ] Validate the supported minimum macOS version, not just the build host's OS.
  Test the final downloaded artifact on Apple Silicon running macOS 14 (the
  chosen minimum) and the newer ARM64 OS selected in task 03. Record the build
  host OS as well if it differs.
  If this fails, fix deployment settings or narrow the support statement and rebuild
  before creating the final tag; do not leave compatibility implied but untested.
- [ ] Follow the installation guide exactly. Verify checksum, executable permissions,
  help/version, `-S`, compilation, and execution of the generated x86-64 program.
- [ ] Verify browser-download/quarantine behavior for the chosen signing policy.
  Distinguish any Gatekeeper issue from Rosetta or missing Command Line Tools.
- [ ] Finalize the changelog/release notes, merge the release preparation, and create
  and push `v0.1.0` on the intended tested commit when release execution is authorized.
  This task file itself does not create a tag or authorize an immediate publication.
- [ ] Let the tag workflow revalidate that commit and create the draft. Download its
  exact final assets, verify checksums, and repeat the installation smoke check.
- [ ] Review the draft's commit, version, notices, prerequisites, signing status, and
  asset list, then publish the existing draft when authorized. Do not rebuild it.
- [ ] Verify the public release links and downloads. Record the release URL, workflow
  run, tested OS versions, and checksums in the completion evidence below.
- [ ] If a published artifact needs correction, prepare a patch release following
  the versioning policy; do not move the published tag or silently replace assets.

## Acceptance

- [ ] The release is public and downloadable with one ARM64 compiler executable.
- [ ] The final downloaded artifact runs without .NET and produces verified x86-64
  output using documented prerequisites.
- [ ] Documentation matches the tested minimum macOS and download experience.
- [ ] Version, source commit, workflow artifact, checksums, and release notes agree.

## Completion evidence

- Release URL:
- Source commit / tag:
- Workflow run:
- Archive SHA-256:
- Tested Apple Silicon hardware / macOS versions:
- Signing/notarization status:
- Clean-machine installation and generated-program result:
