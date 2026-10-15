# C Compiler

A C# and .NET 11 implementation of the C compiler from Nora Sandler's [*Writing a C Compiler*](https://nostarch.com/writing-c-compiler), built chapter by chapter as a learning project.

> [!NOTE]
> This project is a work in progress. It currently completes **Chapter 1** (compiling a single `int` function with `void` parameters that returns an integer constant), with new compiler stages and language features added as we work through the book.

## Quick start

Clone the repository, build the compiler, and compile the sample program:

```sh
git clone https://github.com/zachdillard/compiler.git
cd compiler
dotnet build
dotnet run --project src/Compiler.csproj -- data/return_2.c
./data/return_2
echo $?   # prints 2 (the program's exit status)
```

## Requirements

- **[.NET 11 RC1 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/11.0)** (`11.0.100-rc.1.26425.128`, pinned in `global.json` — choose Arm64 for Apple Silicon or x64 for Intel)
- **GCC** on `PATH` (Apple's Clang-backed `gcc` from Command Line Tools works out of the box)
- **macOS** with x86-64 support (on Apple Silicon, Rosetta 2 is required to run compiled binaries)

If you need to install Apple's Command Line Tools or Rosetta 2:

```sh
xcode-select --install
softwareupdate --install-rosetta   # Apple Silicon only
```

You can verify your setup inside the repository root with `dotnet --version`, `gcc --version`, and `xcode-select -p`.

## Usage

Run the compiler from the repository root with:

```sh
dotnet run --project src/Compiler.csproj -- [options] <source-file>
```

| Option | Behavior |
| --- | --- |
| `<source-file>` | Compiles C source to an executable beside the input file and cleans up intermediate `.i` and `.s` files. |
| `--lex <source-file>` | Runs the lexer only without producing output files. Exits `0` if valid or nonzero on invalid tokens (integer constants must fit in a C# `int`). |
| `-S <source-file>` | Emits an x86-64 assembly file (`.s`) beside the input file without assembling or linking, and removes the `.i` file. |
| `-gcc <source-file>` | Uses the temporary GCC-backed pipeline to preprocess, compile, and link an executable. Combine with `-S` (`-gcc -S` or `-S -gcc`) to emit GCC assembly only. |
| `--version` | Prints the compiler version without invoking GCC (must be used on its own). |

For example, to generate assembly only or run lex-only mode:

```sh
dotnet run --project src/Compiler.csproj -- -S data/return_2.c
dotnet run --project src/Compiler.csproj -- --lex data/return_2.c
```

## Architecture & concepts

- **[Compiler architecture](docs/architecture.md)**: Supported grammar, pipeline diagram, stage responsibilities, and GCC-backed mode.
- **[Concepts guide](concepts/README.md)**: Self-contained, single-file C# examples in `concepts/` that illustrate individual compiler stages (`cd concepts && dotnet <concept-name>.cs`).

## Testing & development

Run the xUnit unit and integration test suite from the repository root:

```sh
dotnet test
```

Format whitespace across the solution (without modifying standalone `concepts/` scripts):

```sh
dotnet format whitespace Compiler.slnx
```

### Book test suite

To test against the official [`writing-a-c-compiler-tests`](https://github.com/nlsandler/writing-a-c-compiler-tests) repository (requires Git and Python 3.8+):

```sh
git clone https://github.com/nlsandler/writing-a-c-compiler-tests.git ../writing-a-c-compiler-tests
./tests/book_tests.sh --chapter 1
```

You can also test a single stage (`./tests/book_tests.sh --chapter 1 --stage lex`), point to a checkout in another location with `BOOK_TESTS_DIR="/path/to/writing-a-c-compiler-tests"`, or test a Release build with `BUILD_CONFIGURATION=Release`.

### Debugging in VS Code

Open the repository root in VS Code with the Microsoft C# extension installed. Pick a configuration in **Run and Debug** (for the main compiler or any `concepts/` example), set breakpoints, and press **F5**. To debug a different source file or flag with the main compiler, edit `args` in [`.vscode/launch.json`](.vscode/launch.json).

## Versioning & license

Each `v0.N.0` release completes Chapter *N* of the book, with `v0.N.P` for patches and `v1.0.0` reserved for a production-readiness milestone. Chapter 1 is implemented (`v0.1.0` in preparation). The version is defined in [`Directory.Build.props`](Directory.Build.props); see the [changelog](CHANGELOG.md) for release history.

This project is [MIT licensed](LICENSE).
