# Task 04: Choose and implement the distribution signing policy

Status: Implemented; final download walkthrough remains a task 08 gate
Depends on: [Task 01](task-01-native-arm64-publish.md)

## Outcome

The first release has an explicit, accurately documented macOS distribution policy.
Developer ID signing and notarization are optional for this learning project.

## Work

- [x] Record the maintainer's choice in the release documentation: publish an
  unsigned/not-notarized learning release, or implement Developer ID signing and
  notarization. Do not silently select a policy requiring unavailable credentials.
- [x] Inspect the produced signature. A linker-generated ad hoc signature is not
  a Developer ID signature and must not be described as notarized distribution.
- [x] For the unsigned path, document the actual browser-download behavior and
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
- [x] Ensure signing precedes final smoke checks, packaging, and checksums. Preserve
  the exact signed executable through validation and upload.

The signed-path items above are not applicable to the chosen unsigned release.

## Acceptance

- [x] One path is recorded with its implications for installation and CI.
- [x] If unsigned is chosen, signing/notarization work is explicitly deferred and
  does not block packaging; release notes state the actual signing status.
- [ ] If signing is chosen, signature verification and notarization succeed and
  failures prevent publication. Credentials are absent from logs and artifacts.
- [ ] Download verification in task 08 checks the final distribution policy on a
  separate Apple Silicon environment, not just the build machine.

## Completion evidence

- On 2026-10-17 the maintainer explicitly selected the unsigned, non-notarized
  learning release. [Distribution policy](../docs/distribution.md) records the
  choice, Apple approval route, CI implications, and deferred signing work.
- Publication calls `scripts/check-release-signature.sh` before final tests and
  retains `signature.txt`; ARM64 CI retains this diagnostic on failures too.
- Local Native AOT publication on Apple Silicon macOS 27.0.1, Xcode 27.1
  (27A9275), and SDK `11.0.100-rc.1.26425.128` passed signature inspection:
  `Signature=adhoc`, linker-signed, `TeamIdentifier=not set`, no `Authority`.
- `tests/release_smoke.sh` passed against that exact executable, SHA-256
  `bb15cd258e2fc3e95f9364c4ac7f50ae9e147c6f3d509a9b1bedd8ae2423023c`.
  Changing a code byte in a separate copy caused signature verification to fail.
- Apple documentation was checked on 2026-10-17. Browser quarantine/approval on
  the final downloaded artifact, separate no-.NET hosts, and macOS 14/15
  remain explicitly assigned to task 08; local smoke checks do not prove them.
