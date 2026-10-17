# Development guide

Start with the [README](../README.md) for a quick introduction and compiler
options. Run commands from the repository root unless noted otherwise.

## Setup

- .NET 11 RC1 SDK **11.0.100-rc.1.26425.128**, as specified in [global.json](../global.json)
- GCC available on `PATH` (Apple's Clang-backed `gcc` is suitable)
- For the custom compiler: macOS with x86-64 assembly/linking support. Running
  its executables on Apple Silicon requires Rosetta 2.

Install the SDK from the [official .NET 11 download page](https://dotnet.microsoft.com/en-us/download/dotnet/11.0),
selecting the RC1 SDK version above: Arm64 for Apple Silicon or x64 for Intel
Macs. The repository uses preview C# features and limited SDK roll-forward;
installing only a newer SDK may not satisfy `global.json`.

Install Apple's Command Line Tools if you do not already have them:

```sh
xcode-select --install
```

On Apple Silicon, install Rosetta 2 if needed to execute the custom compiler's
x86-64 output:

```sh
softwareupdate --install-rosetta
```

With Git and the prerequisites installed, clone the repository and enter it:

```sh
git clone https://github.com/zachdillard/compiler.git
cd compiler
```

If you already cloned it, open a terminal in that checkout's root directory
(the folder containing `global.json`). Run the remaining setup commands there.

Check the prerequisites before building:

```sh
dotnet --version
gcc --version
xcode-select -p
```

Running `dotnet --version` inside the checkout checks SDK selection against
`global.json`. If `dotnet` is missing or reports that no compatible SDK is
installed, install the pinned RC1 SDK linked above, open a new terminal, and
return to the checkout before trying again. If the error persists, use
`dotnet --list-sdks` to check that the required SDK is installed.

The first project build or test run needs access to NuGet to restore test
dependencies.

Build from the repository root with:

```sh
dotnet build -m:1 --disable-build-servers
```

These flags avoid MSBuild worker socket failures in Codex’s sandbox.

## Native ARM64 release publication

Run the release command from the repository root:

```sh
./scripts/publish-release.sh
```

Publication requires an Apple Silicon Mac running natively (not a Rosetta shell),
the exact SDK version in `global.json`, and full Xcode with its license accepted
and macOS SDK installed. Install Xcode and finish its first-launch setup before
publishing. Command Line Tools alone remain sufficient for ordinary development;
the release script requires full Xcode so its version can be recorded. NuGet
access is needed on the first restore of the Native AOT toolchain/runtime packs.

The script checks the host, selected SDK, active developer directory, Xcode,
macOS SDK, and Clang. It respects `DEVELOPER_DIR` when set and never changes the
machine's global Xcode selection. To use a particular installed Xcode for one run:

```sh
DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer ./scripts/publish-release.sh
```

Each run creates a fresh `artifacts/release/osx-arm64.XXXXXX/` directory. Its
`publish/Compiler` is the Release, self-contained Native AOT executable;
`publish/Compiler.dSYM/` contains optional separate debug symbols. Build
intermediates go into `build/`, with `publish.log`, `publish.binlog`, and
`dependencies.txt` alongside it. The script prints the exact output paths.
Package only `Compiler`; retain the `.dSYM` for optional symbol packaging. The
executable needs no DLL, runtime configuration, .NET installation, or `.dSYM`.

Release settings apply only to this command. It publishes `src/Compiler.csproj`,
uses the repository's single-worker/build-server flags, and fails on compiler,
AOT, or trimming warnings without suppressing them. It checks the executable with
`file`, `lipo -archs`, and `otool -L`, requiring ARM64 alone and dynamic dependencies
only in public macOS system library/framework directories. GCC and Rosetta are
still needed to compile and run generated **x86-64** programs, respectively.
Native AOT publication and separate macOS symbols are described in the
[official .NET guide](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

The chosen minimum for the ARM64 binary is **macOS 14.0 (Sonoma)**. Publication
sets both Native AOT's `AppleMinOSVersion=14.0` and
`MACOSX_DEPLOYMENT_TARGET=14.0`; this matches the pinned toolchain's default
minimum. This is a support decision pending runtime verification, not a claim
that Mach-O deployment metadata proves compatibility. Task 03 must validate on
macOS 14 and its selected newer ARM64 CI OS (or track a manual macOS 14 machine
check). Task 08 must test the final downloaded artifact on macOS 14 and the newer
CI OS before release. Also record the build host OS if it differs. The compiler
continues to generate x86-64 output.

## ARM64 release CI

Chapter one's [distribution policy](distribution.md) is unsigned and
non-notarized, with only the linker's ad hoc signature. Publication verifies
that signature before smoke checks and records `signature.txt`. CI requires no
Developer ID or notarization credentials. The final browser-download approval
walkthrough remains a task 08 publication gate.

[Compiler Tests](../.github/workflows/compiler-tests.yml) preserves the Intel
Debug/Release jobs and calls the reusable
[ARM64 Release Validation](../.github/workflows/arm64-release-validation.yml)
workflow when source, tests, release scripts, publish settings, or workflows change.
It can also be run manually. All jobs have read-only repository permissions.

The ARM64 workflow builds and tests Release on `macos-15`, publishes with
`./scripts/publish-release.sh`, then runs release smoke checks and all four
chapter-one book-suite modes against that executable. The upstream suite is pinned
to `ae12014d2dec14488f3f80d14df4b6d8e4634d7d`, reviewed and exercised in task 02;
both Intel and ARM64 jobs log that revision. Updating it requires a deliberate
workflow change and a review of upstream changes.

GitHub currently documents both `macos-14` and `macos-15` as ARM64 in its
[runner reference](https://docs.github.com/en/actions/reference/runners/github-hosted-runners).
The jobs assert native ARM64 execution and the expected OS major version, report
Xcode/macOS SDK/GCC/Python versions, install Rosetta if absent, and probe Xcode's
`/usr/bin/python3` as x86-64. Missing Xcode or compatible Python fails with an
explicit prerequisite error. The publish script additionally requires the exact
.NET SDK pin and records its toolchain. A second job downloads the same validated
executable onto `macos-14` and repeats smoke checks without installing .NET.
This checks macOS 14 compatibility of the macOS 15 build, rather than rebuilding
on the older OS. Task 08 must still check the final downloaded release on separate
Apple Silicon macOS 14 and 15 machines, including the no-.NET installation check.

Diagnostics (including project test TRX, publish logs/binlog, source commit,
upstream commit, and checksums) are uploaded even after validation failure.
The successful executable and optional symbols are uploaded separately under
`compiler-arm64-<source-SHA>-<run-attempt>`; artifact download requires restoring
the executable bit. These are inspection artifacts, not packaged release assets.

For task 07, call the reusable workflow using
`uses: ./.github/workflows/arm64-release-validation.yml` and make packaging depend
on that call succeeding, including its macOS 14 job. Its `compiler-artifact`
output identifies the executable validated in the caller's run. Checkout is
explicitly bound to the caller's `github.sha` (including a PR merge commit or
tag commit), recorded and asserted in logs. A tagged release must invoke this
workflow on its own commit and consume that artifact; a green check from another
commit is insufficient. Signing and final archive validation remain task 07 work.

## Debugging in VS Code

Install the Microsoft C# extension and open the repository root in VS Code.
There is a debug configuration for the main compiler and each example in
`concepts/`. Choose a configuration in the Run and Debug view, set breakpoints,
and press F5. Each configuration runs its matching build task before launching.

The main compiler configuration uses `data/return_2.c` as its input. To debug
another source file or compiler mode, edit its `args` in
[`.vscode/launch.json`](../.vscode/launch.json).

## Formatting

The repository's `.editorconfig` defines C# formatting. To format the main
application and tests without changing the standalone concepts examples, run:

```sh
dotnet format whitespace Compiler.slnx
```

## Testing

Run the project tests from the repository root:

```sh
dotnet test
```

### Debug and Release checks

To check the project and chapter-one book tests in both build configurations:

```sh
dotnet build --configuration Debug -m:1 --disable-build-servers
dotnet test --configuration Debug --no-build
BUILD_CONFIGURATION=Debug ./tests/book_tests.sh --chapter 1
dotnet build --configuration Release -m:1 --disable-build-servers
dotnet test --configuration Release --no-build
BUILD_CONFIGURATION=Release ./tests/book_tests.sh --chapter 1
```

### What the tests cover

The xUnit v2 project includes unit tests for Lexer, Parser, Generator, and
Emitter, an in-memory pipeline test from C source to assembly text, and
integration tests for the compiler executable. The unit and in-memory pipeline
tests do not invoke GCC or require Rosetta.

Integration tests launch the compiler DLL copied into the test output, using
that build's test runtime configuration and dependency manifest. They do not
require a separate Debug build when testing Release. They require GCC on
`PATH`; executing custom compiler output on Apple Silicon also requires Rosetta 2.
They check CLI behavior, diagnostics, intermediate-file cleanup, and execution
of generated programs, including assembling and linking the output of `-S`.
Stage tests cover `--lex`, `--parse`, `--codegen`, and `-S`, including rejecting
combinations and ensuring that stage-only modes do not invoke the assembler.

Regression cases cover out-of-range integer constants in every compilation
mode, partial preprocessing output, missing GCC, and file-write and cleanup
errors. Some failure-path tests use a temporary GCC stub on `PATH` to reproduce
partial output and assembly cleanup failures deterministically.

### Continuous integration

[GitHub Actions](../.github/workflows/compiler-tests.yml) runs these builds and
tests on an Intel macOS runner for pushes to `main` and pull requests that change
`src/`, `tests/`, `global.json`, `Directory.Build.props`, `Compiler.slnx`, or the
workflow itself. It can also be started manually. It installs the SDK from
`global.json` and checks GCC availability. Each configuration also runs the
chapter-one book suite in full and at the lex, parse, and codegen stages against
a separate checkout of the upstream tests.

### Optional book tests

The book suite is a separate checkout and requires Git, Python 3.8 or newer,
and the compiler prerequisites above. From the compiler repository root:

```sh
git clone https://github.com/nlsandler/writing-a-c-compiler-tests.git \
  ../writing-a-c-compiler-tests
./tests/book_tests.sh --chapter 1
```

This runs the full chapter-one suite against the custom compiler, including
valid programs and invalid lexing/parsing cases. To test individual stages, use:

```sh
./tests/book_tests.sh --chapter 1 --stage lex
./tests/book_tests.sh --chapter 1 --stage parse
./tests/book_tests.sh --chapter 1 --stage codegen
```

The runner has no assembly-only stage; the project integration tests and release
smoke script validate `-S`, including linking its assembly and executing the result.

Run the script from the compiler repository root so `dotnet` uses the SDK
selection in `global.json` and the script resolves its paths correctly. It builds
the compiler into `artifacts/book-tests/`, restoring dependencies as needed,
then runs the book's `test_compiler` runner with your arguments. The generated
build output is ignored by Git.

For an existing checkout elsewhere:

```sh
BOOK_TESTS_DIR="/path/to/writing-a-c-compiler-tests" \
  ./tests/book_tests.sh --chapter 1
```

See the
[`writing-a-c-compiler-tests`](https://github.com/nlsandler/writing-a-c-compiler-tests)
repository for additional test runner usage. The script defaults to a Debug
build; set `BUILD_CONFIGURATION=Release` to test a Release build.

Set `BOOK_TESTS_PYTHON` to a Python executable name or path to select another
interpreter (one executable, without arguments). On macOS the wrapper launches
that interpreter with `arch -x86_64` and checks its version and architecture
independently before running the suite. An ARM-only Python does not become
x86-64 simply because its parent shell uses Rosetta. Python/Rosetta prerequisite
failures print `Prerequisite failure:` and exit with status 2. Choose a universal
or x86-64 Python 3.8+; for example, where available:

```sh
BOOK_TESTS_PYTHON=/usr/bin/python3 ./tests/book_tests.sh --chapter 1
```

### Published executable validation

After publication, pass the printed executable path to the smoke script:

```sh
./tests/release_smoke.sh artifacts/release/osx-arm64.XXXXXX/publish/Compiler
```

The script copies only that executable into a temporary directory and removes
the directory on exit. It requires ARM64-only compiler code, runs the compiler
directly with .NET excluded from `PATH`, checks help/version (against
`Directory.Build.props`), stages, diagnostics, paths with spaces, intermediate
cleanup, x86-64 preprocessing and output, and generated-program exit codes.
Set `EXPECTED_VERSION=0.1.0` to override the expected version for another release.
GCC and Rosetta are required. Excluding .NET from `PATH` is an isolation check;
task 08 separately requires a machine without .NET installed.

Use the same published artifact for all four book-suite modes:

```sh
export COMPILER_PATH="$PWD/artifacts/release/osx-arm64.XXXXXX/publish/Compiler"
./tests/book_tests.sh --chapter 1
./tests/book_tests.sh --chapter 1 --stage lex
./tests/book_tests.sh --chapter 1 --stage parse
./tests/book_tests.sh --chapter 1 --stage codegen
unset COMPILER_PATH
```

Replace `osx-arm64.XXXXXX` with the directory printed by publication. With
`COMPILER_PATH` supplied, the wrapper validates and resolves the executable path,
skips the build entirely, and invokes that executable through the upstream runner.
It reports and compares the compiler's SHA-256 before and after each run,
including failed runs, and preserves the runner's exit code. `BOOK_TESTS_DIR`,
`BOOK_TESTS_PYTHON`, and forwarded runner options still apply. Without
`COMPILER_PATH`, the existing source-build behavior and `BUILD_CONFIGURATION`
selection apply. The upstream checkout must be writable because tests create
outputs beside their C inputs.

To compile an individual book-test file directly:

```sh
dotnet run --project src/Compiler.csproj -- /path/to/writing-a-c-compiler-tests/tests/chapter_1/valid/return_2.c
```

## Concepts

The `concepts/` folder contains self-contained, file-based C# examples. Each
example can be run individually from that folder with:

```sh
cd concepts
dotnet <concept-name>.cs
```

See the [concepts guide](../concepts/README.md) for the available examples,
requirements, and run instructions.

## Versioning

Each `v0.N.0` release completes chapter N, including all earlier chapters.
Patch releases use `v0.N.P` for fixes within that chapter's scope. `v1.0.0`
is reserved for a separate production-readiness milestone.

The version is defined in [Directory.Build.props](../Directory.Build.props).
See the [changelog](../CHANGELOG.md) for release history and unreleased work.
