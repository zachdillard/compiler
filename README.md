# C Compiler

> [!WARNING]
> This project is under development. Additional compiler stages and language
> features will be added as the implementation progresses through the book.

This project is a C# and .NET 11 RC1 implementation of the C compiler described in [*Writing a C Compiler*](https://nostarch.com/writing-c-compiler) by Nora Sandler.

## Versioning and license

Each `v0.N.0` release completes chapter N, including all earlier chapters.
Patch releases use `v0.N.P` for fixes within that chapter's scope. `v1.0.0`
is reserved for a separate production-readiness milestone.
Chapter one is implemented; `v0.1.0` is being prepared and has not been published.
The version is defined in `Directory.Build.props`; see the [changelog](CHANGELOG.md)
for release history.

The project is [MIT licensed](LICENSE).

## Requirements

- .NET 11 RC1 SDK **11.0.100-rc.1.26425.128**, as specified in `global.json`
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

Check the prerequisites before building:

```sh
dotnet --version
gcc --version
xcode-select -p
```

Run `dotnet --version` inside the checkout so it uses `global.json`.

## Quick start

With Git and the prerequisites installed:

```sh
git clone https://github.com/zachdillard/compiler.git
cd compiler
dotnet build
dotnet run --project src/Compiler.csproj -- data/return_2.c
./data/return_2
echo $?
```

The generated program prints nothing and exits with status `2`, which
`echo $?` displays. This is the expected result, not a compilation failure.

## Architecture

See [Compiler architecture](docs/architecture.md) for the supported grammar,
pipeline diagram, stage responsibilities, and GCC-backed mode.

## Usage

Print the version without invoking GCC:

```sh
dotnet run --project src/Compiler.csproj -- --version
```

`--version` must be used on its own.


Build the compiler from the repository root:

```sh
dotnet build
```

Compile a C source file with the custom compiler by passing its path as the only argument:

```sh
dotnet run --project src/Compiler.csproj -- data/return_2.c
```

This creates `data/return_2` beside the input file. Intermediate `.i` and `.s`
files are removed.

To stop after lexing, pass `--lex`:

```sh
dotnet run --project src/Compiler.csproj -- --lex data/return_2.c
```

A lexically valid file exits with status `0`; an invalid token produces a
nonzero exit status. Tokens are collected without being printed. Lex-only mode
does not parse the program or create assembly or an executable. Integer
constants must fit in a C# `int`.

To test a specific file from the book's test suite, pass its path directly:

```sh
dotnet run --project src/Compiler.csproj -- /path/to/writing-a-c-compiler-tests/tests/chapter_1/valid/return_2.c
```

To use the temporary GCC-backed compiler, pass `-gcc`:

```sh
dotnet run --project src/Compiler.csproj -- -gcc data/return_2.c
```

The example above creates `data/return_2` beside the input file.
Intermediate `.i` and `.s` files are removed after each successful stage.

To generate assembly without linking an executable, use `-S`:

```sh
dotnet run --project src/Compiler.csproj -- -S data/return_2.c
```

This selects the custom compiler's assembly-only mode. To generate assembly
with GCC, combine `-S` and `-gcc` in either order:

```sh
dotnet run --project src/Compiler.csproj -- -gcc -S data/return_2.c
dotnet run --project src/Compiler.csproj -- -S -gcc data/return_2.c
```

These commands create `data/return_2.s` and remove the intermediate `.i` file.

You can run the generated executable with:

```sh
./data/return_2
echo $?
```

The example exits with status `2` with either compiler. It prints nothing;
`echo $?` displays the exit status.

## Unit tests

From the repository root, build and run the xUnit v3 tests:

```sh
dotnet build -m:1 --disable-build-servers
dotnet test --solution Compiler.slnx --no-build
```

The tests exercise Lexer, Parser, Generator, and Emitter directly, plus one
in-memory pipeline check. They require the pinned .NET SDK but do not invoke
GCC, create compiler output files, or require Rosetta. The book tests below
provide acceptance coverage for the complete compiler executable.

## Optional book tests

The book suite is a separate checkout and requires Git, Python 3.8 or newer,
and the compiler prerequisites above. From the compiler repository root:

```sh
git clone https://github.com/nlsandler/writing-a-c-compiler-tests.git \
  ../writing-a-c-compiler-tests
./tests/book_tests.sh --chapter 1
```

The equivalent file-based C# script accepts the same options and environment
variables:

```sh
dotnet tests/book_tests.cs -- --chapter 1
```

This runs the full chapter-one suite against the custom compiler, including
valid programs and invalid lexing/parsing cases. To test only lexing, use:

```sh
./tests/book_tests.sh --chapter 1 --stage lex
```

Run the script from the compiler repository root so `dotnet` uses the SDK
selection in `global.json`. It builds the compiler into `artifacts/book-tests/`,
restoring dependencies as needed, then runs the book's `test_compiler` runner
with your arguments. The generated build output is ignored by Git.
For an existing checkout elsewhere:

```sh
BOOK_TESTS_DIR="/path/to/writing-a-c-compiler-tests" \
  ./tests/book_tests.sh --chapter 1
```

See the
[`writing-a-c-compiler-tests`](https://github.com/nlsandler/writing-a-c-compiler-tests)
repository for additional test runner usage. The script defaults to a Debug
build; set `BUILD_CONFIGURATION=Release` to test a Release build.

## Development

The repository's `.editorconfig` defines C# formatting. To format the compiler
and unit tests without changing the standalone scripts and concepts examples, run:

```sh
dotnet format whitespace Compiler.slnx
```

To build and run the unit and book tests in both configurations:

```sh
dotnet build --configuration Debug -m:1 --disable-build-servers
dotnet test --solution Compiler.slnx --configuration Debug --no-build
BUILD_CONFIGURATION=Debug ./tests/book_tests.sh --chapter 1
dotnet build --configuration Release -m:1 --disable-build-servers
dotnet test --solution Compiler.slnx --configuration Release --no-build
BUILD_CONFIGURATION=Release ./tests/book_tests.sh --chapter 1
```

GitHub Actions runs these builds and tests on an Intel macOS runner for pushes,
pull requests, and manual runs. It installs the SDK from `global.json` and checks
GCC availability. Each configuration runs the unit tests and the chapter-one
book suite against a separate checkout of the upstream tests.

The current implementation passes all 24 chapter-one book tests on macOS with
Apple Silicon and Rosetta 2.

## Concepts

The `concepts/` folder contains self-contained, file-based C# examples. Each
example can be run individually from that folder with:

```sh
cd concepts
dotnet <concept-name>.cs
```

