# Task 01: Make native ARM64 publication repeatable

Status: Planned  
Depends on: None

## Outcome

A repository command publishes one self-contained Native AOT executable named
`Compiler` for `osx-arm64`, using the SDK pinned in `global.json`.

## Work

- [ ] Add a small release-publish script, with a publish profile only if it avoids
  duplication. Publish `src/Compiler.csproj`, not the solution or test project.
- [ ] Use Release configuration, `osx-arm64`, `PublishAot=true`, and self-contained
  deployment. Keep ARM64 release settings from changing ordinary development builds.
- [ ] Start from the command below. Require a zero exit status and fail on actionable
  AOT/trimming warnings; investigate warnings rather than suppressing them.
- [ ] Publish into a fresh directory under `artifacts/` so stale files cannot enter
  the release. Preserve the existing executable name and `--version` behavior.
- [ ] Verify the selected Xcode, SDK, and ARM64 build host. Record prerequisites in
  the development guide; do not change the machine's global Xcode selection in a script.
- [ ] Choose an explicit supported minimum macOS version, align the deployment
  target with it, and record which OS versions must be tested in tasks 03 and 08.
- [ ] Inspect the artifact with `file`, `lipo -archs`, and `otool -L`; ensure it has
  only ARM64 code and does not depend on .NET or private build-machine libraries.
- [ ] Keep the generated `.dSYM` available for optional separate packaging.

```sh
dotnet publish src/Compiler.csproj \
  -c Release -r osx-arm64 --self-contained true \
  -p:PublishAot=true \
  -o artifacts/release/osx-arm64 \
  -m:1 --disable-build-servers
```

## Acceptance

- [ ] Publication succeeds from a clean checkout with the documented prerequisites.
- [ ] Copying just `Compiler` to a new directory is sufficient to run `--help` and
  `--version`; no DLL, runtime configuration, or `.dSYM` is needed.
- [ ] `--version` matches `Directory.Build.props`; generated code remains x86-64.
- [ ] The supported macOS minimum is documented as a decision, with verification
  tracked rather than inferred solely from Mach-O metadata.
