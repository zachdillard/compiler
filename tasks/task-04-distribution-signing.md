# Task 04: Choose and implement the distribution signing policy

Status: Planned  
Depends on: [Task 01](task-01-native-arm64-publish.md)

## Outcome

The first release has an explicit, accurately documented macOS distribution policy.
Developer ID signing and notarization are optional for this learning project.

## Work

- [ ] Record the maintainer's choice in the release documentation: publish an
  unsigned/not-notarized learning release, or implement Developer ID signing and
  notarization. Do not silently select a policy requiring unavailable credentials.
- [ ] Inspect the produced signature. A linker-generated ad hoc signature is not
  a Developer ID signature and must not be described as notarized distribution.
- [ ] For the unsigned path, document the actual browser-download behavior and
  Apple's supported per-item approval process. Do not advise disabling Gatekeeper
  globally. Task 08 must verify that the documented installation route works.
- [ ] For the signed path, document required Developer ID credentials and notarization
  authentication. Keep secrets in GitHub secrets and an ephemeral CI keychain;
  expose them only to trusted release jobs, never pull-request jobs.
- [ ] For the signed path, sign with the appropriate hardened-runtime/timestamp
  options, verify the signature, and submit a supported package using `notarytool`.
  Wait for success and retain the notarization log before treating it as releasable.
- [ ] Use a temporary ZIP if needed for notarization while keeping the planned
  release archive. Do not attempt to staple a ticket to a bare executable or ZIP;
  consult current Apple documentation if choosing a staplable distribution container.
- [ ] Ensure signing precedes final smoke checks, packaging, and checksums. Preserve
  the exact signed executable through validation and upload.

## Acceptance

- [ ] One path is recorded with its implications for installation and CI.
- [ ] If unsigned is chosen, signing/notarization work is explicitly deferred and
  does not block packaging; release notes state the actual signing status.
- [ ] If signing is chosen, signature verification and notarization succeed and
  failures prevent publication. Credentials are absent from logs and artifacts.
- [ ] Download verification in task 08 checks the final distribution policy on a
  separate Apple Silicon environment, not just the build machine.
