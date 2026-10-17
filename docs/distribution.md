# macOS distribution policy

The maintainer chose an **unsigned, non-notarized learning release** for chapter
one on 2026-10-17. Developer ID signing and Apple notarization are deferred;
neither credentials nor signing secrets are required in CI. This choice does
not block packaging.

Native AOT's linker gives the ARM64 executable an **ad hoc signature**. That
signature permits native execution and detects changes to its code; it does
not identify a developer and is not evidence of notarization. Describe release
assets as unsigned for distribution, with an ad hoc signature only.

## Artifact checks and ordering

`scripts/publish-release.sh` calls `scripts/check-release-signature.sh` before
artifact validation. The check requires a valid ad hoc signature, no signing
team, and no certificate authority, and writes inspection output to
`signature.txt` alongside the publish log. It never modifies the executable.
It can also be run directly:

```sh
./scripts/check-release-signature.sh artifacts/release/osx-arm64.XXXXXX/publish/Compiler
```

Keep the order: publish, inspect the signature, run smoke checks and book tests,
package, validate the extracted archive, then upload. Packaging must inspect
the supplied executable again and preserve its exact bytes. Any future signing
implementation must precede final tests, packaging, and checksums; it requires
an explicit policy change and its own credential and notarization work.

## Download approval

Browser downloads may carry macOS quarantine information. A downloaded archive
and its extracted `Compiler` can therefore trigger Gatekeeper even though the
locally built executable runs. An unidentified-developer or cannot-check-for-
malware alert is expected for this policy. Downloading with a command-line
tool or copying a local build does not test the browser experience.

After verifying the archive checksum and extracting it, try
`./compiler-0.1.0-osx-arm64/Compiler --version` from the download directory.
If macOS blocks it and you trust the download:

1. Dismiss the alert and open **System Settings → Privacy & Security**.
2. Scroll to **Security**, select **Open Anyway** for `Compiler`, and authenticate
   if requested.
3. Confirm **Open** in the next prompt, then repeat the version command.

This follows [Apple's per-item approval instructions](https://support.apple.com/en-us/102445).
It creates an exception for this executable. Do not disable Gatekeeper globally
or remove quarantine recursively. If approval is unavailable or the alert says
the file is damaged or contains malware, stop and report the exact message.
Changing managed-device security settings is outside this installation route.

The final browser-download walkthrough remains a **task 08 release gate**.
Before publication, record the browser, quarantine attributes, exact alert and
approval steps, successful version command, and generated-program exit status
on separate Apple Silicon macOS 14 and 15 environments without .NET. These
instructions are based on Apple's documented process; local signature/smoke
checks do not establish that this final download route has passed.
