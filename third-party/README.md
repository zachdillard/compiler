# Binary redistribution notices

The runtime archive contains the repository's `LICENSE` and a combined
`THIRD-PARTY-NOTICES.txt` assembled from the three verbatim files here.
The repository MIT license does not replace dependency notices.

Audit performed on 2026-10-17 for the versions in `versions.json`:

- **.NET Native AOT runtime 11.0.0-rc.1.26425.128**: `dotnet-LICENSE.txt`
  and `dotnet-THIRD-PARTY-NOTICES.txt` are copied verbatim from the restored
  `Microsoft.NETCore.App.Runtime.NativeAOT.osx-arm64` NuGet package. Its nuspec
  identifies MIT and the source commit
  `3551975be08744f0418857c5bed8ab1545c5dd47` in `dotnet/dotnet`. Include the full
  third-party notice collection conservatively, even though trimming can remove
  components. The ordinary macOS runtime package contains the same notice file.
  Upstream: [.NET license](https://github.com/dotnet/runtime/blob/v11.0.0-rc.1.26425.128/LICENSE.TXT)
  and [runtime notices](https://github.com/dotnet/runtime/blob/v11.0.0-rc.1.26425.128/THIRD-PARTY-NOTICES.TXT).
- **System.CommandLine 2.0.13**: its NuGet package declares MIT and source
  commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699` in `dotnet/dotnet`.
  `system-commandline-LICENSE.md` is the verbatim
  [license at that commit](https://github.com/dotnet/dotnet/blob/4099f83977f4d29604a9aeb971fb0a7bc1d5f699/src/command-line-api/LICENSE.md).
  The selected `net8.0` asset has no transitive package dependencies. The
  `.NETStandard2.0` dependency on System.Memory is not selected for `net11.0`.
- GCC, Xcode, Rosetta, and macOS system libraries are user/platform prerequisites
  and are not redistributed in the archive. Test SDKs and xUnit are not shipped.

Publication records the resolved runtime and System.CommandLine versions from
`project.assets.json`. Packaging rejects versions outside this audit. When
upgrading either dependency, review its selected dependencies and package
license metadata, replace the applicable texts from the exact version, and
update this audit and `versions.json`. Packaging requires no network access and
never restores packages or republishes the executable.
