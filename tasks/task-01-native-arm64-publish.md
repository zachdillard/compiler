# Task 01: Make native ARM64 publication repeatable

Status: Complete

Depends on: None

## Outcome

A repository command publishes one self-contained Native AOT executable named
`Compiler` for `osx-arm64`, using the SDK pinned in `global.json`.

## Work

- [x] Add a small release-publish script, with a publish profile only if it avoids
  duplication. Publish `src/Compiler.csproj`, not the solution or test project.
- [x] Use Release configuration, `osx-arm64`, `PublishAot=true`, and self-contained
  deployment. Keep ARM64 release settings from changing ordinary development builds.
- [x] Start from the command below. Require a zero exit status and fail on actionable
  AOT/trimming warnings; investigate warnings rather than suppressing them.
- [x] Publish into a fresh directory under `artifacts/` so stale files cannot enter
  the release. Preserve the existing executable name and `--version` behavior.
- [x] Verify the selected Xcode, SDK, and ARM64 build host. Record prerequisites in
  the development guide; do not change the machine's global Xcode selection in a script.
- [x] Choose an explicit supported minimum macOS version, align the deployment
  target with it, and record which OS versions must be tested in tasks 03 and 08.
- [x] Inspect the artifact with `file`, `lipo -archs`, and `otool -L`; ensure it has
  only ARM64 code and does not depend on .NET or private build-machine libraries.
- [x] Keep the generated `.dSYM` available for optional separate packaging.

```sh
dotnet publish src/Compiler.csproj \
  -c Release -r osx-arm64 --self-contained true \
  -p:PublishAot=true \
  -o artifacts/release/osx-arm64 \
  -m:1 --disable-build-servers
```

## Acceptance

- [x] Publication succeeds from a clean checkout with the documented prerequisites.
- [x] Copying just `Compiler` to a new directory is sufficient to run `--help` and
  `--version`; no DLL, runtime configuration, or `.dSYM` is needed.
- [x] `--version` matches `Directory.Build.props`; generated code remains x86-64.
- [x] The supported macOS minimum is documented as a decision, with verification
  tracked rather than inferred solely from Mach-O metadata.

## Completion evidence

- Command: `./scripts/publish-release.sh`; no project-wide release settings or
  publish profile were needed. See the [development guide](../docs/development.md#native-arm64-release-publication).
- Validated on 2026-10-17: ARM64 macOS 27.0.1, Xcode 27.1 (27A9275), macOS SDK
  27.0, and .NET SDK `11.0.100-rc.1.26425.128`.
- Publication exited zero with fresh intermediates/output, both in the working
  checkout and in a clean `git archive HEAD` export plus the new script, under
  a path containing spaces. Local evidence: `artifacts/release/clean-publish.log`
  and `artifacts/release/osx-arm64.6Wwilc/` (ignored build artifacts).
- `file` and `lipo -archs` reported ARM64 only. `otool -L` showed only public
  `/usr/lib/` and `/System/Library/Frameworks/` dependencies. `vtool -show-build`
  reported `minos 14.0`. The output contains `Compiler` and `Compiler.dSYM` alone.
- Copied only `Compiler` to a fresh directory, excluded .NET from `PATH`, and
  ran `--help` and `--version` successfully. Version was `Compiler 0.1.0`, matching
  `Directory.Build.props`. `-S` emitted x86-64 assembly; normal compilation
  produced a Mach-O x86-64 program returning `2` and removed intermediates.
- No AOT/trimming warnings occurred. Apple's symbol tool emitted two duplicate
  debug-map warnings: `ar -t` confirmed duplicate `contract-descriptor.c.o` and
  `contractpointerdata.cpp.o` members in the pinned SDK's
  `libRuntime.WorkstationGC.a`. These SDK symbol warnings were not suppressed;
  the `.dSYM` was produced and retained.
- Ordinary development build passed with zero warnings/errors. Existing tests:
  Passed: 185, Failed: 0, Skipped: 0. The test command used `--no-build`, `-m:1`,
  and `--disable-build-servers` and required execution outside the sandbox for
  VSTest's local communication socket.
- macOS 14.0 is the chosen minimum. Runtime verification on macOS 14 and the newer
  ARM64 CI OS is explicitly required by tasks 03 and 08; it was not performed on
  this macOS 27 host. A machine without .NET remains task 08's installation check.
