# Task 07: Automate creation of a validated draft release

Status: Planned  
Depends on: [Task 03](task-03-arm64-release-validation-ci.md),
[Task 05](task-05-package-release-assets.md),
[Task 06](task-06-document-binary-installation.md)

## Outcome

A tag-triggered GitHub Actions workflow validates, packages, and uploads a draft
release. Manual runs rehearse the same process without publishing a release.

## Work

- [ ] Add `.github/workflows/release.yml` with version-tag push and manual triggers.
  Validate tags as `vMAJOR.MINOR.PATCH` and require an exact match with project
  metadata and the published executable's reported version.
- [ ] Check out and record the exact source commit. For tag releases, verify it is
  reachable from the intended release branch and test that commit, not an unrelated
  latest branch revision. Check out sufficient history for these validations.
- [ ] Run existing source tests and task 03's ARM64 validation using shared scripts
  or reusable jobs. Do not rely only on path-filtered push/PR checks having run.
- [ ] Publish once, apply the selected signing policy, validate, package, and validate
  the extracted archive. Upload those exact files; do not rebuild in the upload job.
- [ ] On manual rehearsal, retain downloadable workflow artifacts and logs without
  creating tags or GitHub releases. Make this behavior clear in workflow inputs.
- [ ] On successful tag runs, create a draft release and attach the archive,
  `SHA256SUMS`, optional symbols, and prepared release notes/build metadata.
- [ ] Default workflow permissions to `contents: read`; grant `contents: write`
  only to the release upload job. Keep signing secrets out of untrusted jobs.
- [ ] Serialize runs for the same tag. Define rerun behavior: reuse a matching draft
  carefully or fail clearly, and never silently overwrite a published release's assets.
- [ ] Set bounded job timeouts and preserve useful failure logs. Any failed required
  check must prevent creation of a release that appears ready to publish.

## Acceptance

- [ ] A manual rehearsal completes and produces inspectable archives/checksums,
  without creating a public release or tag.
- [ ] Version mismatch, failed tests, failed publish, or failed signing/checksum
  validation prevents draft-release upload.
- [ ] A valid tag run produces a draft with the exact validated artifact and notes.
- [ ] The documented final publication step promotes this draft without rebuilding.
- [ ] Workflow syntax and permissions are reviewed, and its real behavior is
  verified by rehearsal rather than only inspecting YAML.
