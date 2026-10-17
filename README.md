# C Compiler

A small C compiler built with C# and .NET 11 RC1, following
[*Writing a C Compiler*](https://nostarch.com/writing-c-compiler) by Nora Sandler.
This is a learning and experimentation project, with more language features added
as work progresses through the book.

## Purpose

The goals of this project are to:

- Learn how compilers work by building one step by step.
- Showcase and explore .NET 11 features.
- Learn more about CI/CD through hands-on practice.
- Experiment with different development methodologies and agentic engineering.

## Current capabilities

The custom compiler currently completes chapter one: a single `int` function
with `void` parameters that returns an integer constant, like the included
[data/return_2.c](data/return_2.c):

```c
int main(void) {
    return 2;
}
```

Compiling this program creates an executable that returns `2` to your shell.

## Binary installation

The planned [v0.1.0 release](https://github.com/zachdillard/compiler/releases/tag/v0.1.0)
provides an **Apple Silicon-only** compiler with its .NET runtime included.
It generates **macOS x86-64** programs. Binary users need neither .NET nor a
source checkout. The intended minimum is **macOS 14.0 (Sonoma)**; runtime checks
on macOS 14 and 15 and the final browser-download walkthrough are still release
gates. These are candidate instructions: the download links become available
when task 08 publishes the release.

Compilation still requires Apple's Command Line Tools (`gcc` on `PATH`):

```sh
xcode-select --install # If the tools are not already installed
gcc --version
```

Rosetta 2 is needed on Apple Silicon only to execute the generated x86-64
programs, including the example below. It is not needed to run the ARM64
compiler. If necessary, run `softwareupdate --install-rosetta` and follow its
prompts; see [Apple's Rosetta instructions](https://support.apple.com/en-us/102527).

Download [compiler-0.1.0-osx-arm64.tar.gz](https://github.com/zachdillard/compiler/releases/download/v0.1.0/compiler-0.1.0-osx-arm64.tar.gz)
and [SHA256SUMS](https://github.com/zachdillard/compiler/releases/download/v0.1.0/SHA256SUMS)
from that same release. In their download directory, verify the runtime archive:

```sh
awk '$2 == "compiler-0.1.0-osx-arm64.tar.gz"' SHA256SUMS > runtime.sha256
test -s runtime.sha256 && shasum -a 256 -c runtime.sha256
```

Continue only after it reports `compiler-0.1.0-osx-arm64.tar.gz: OK`. Selecting
this entry avoids requiring the optional symbols archive listed in `SHA256SUMS`.
Extract and run the compiler directly:

```sh
tar -xzf compiler-0.1.0-osx-arm64.tar.gz
./compiler-0.1.0-osx-arm64/Compiler --version
# Compiler 0.1.0
```

This learning release is **unsigned for distribution and not notarized**; it
retains only the linker's ad hoc signature. Browser quarantine may cause macOS
to block the first launch. After the blocked attempt, dismiss the alert and,
if you trust the verified download, open **System Settings → Privacy & Security**,
scroll to **Security**, choose **Open Anyway** for `Compiler`, authenticate if
requested, and confirm **Open**. Repeat the version command. This follows
[Apple's per-item approval process](https://support.apple.com/en-us/102445).
Keep Gatekeeper enabled; if approval is unavailable or the alert reports damage
or malware, stop and report the exact message. The final downloaded executable's
approval walkthrough remains a task 08 gate; see the
[distribution policy](docs/distribution.md#download-approval).

Compile the chapter-one example from the download directory:

```sh
printf 'int main(void) { return 2; }\n' > return_2.c
./compiler-0.1.0-osx-arm64/Compiler return_2.c
./return_2
echo $? # Expected: 2; the program prints nothing
./compiler-0.1.0-osx-arm64/Compiler -S return_2.c
```

Normal compilation writes `return_2` beside its source and removes `.i` and `.s`
intermediates. `-S` writes `return_2.s` without linking and removes `.i`.

Optionally keep the executable, guide, and notices in a user-owned directory and
add a command to `PATH`:

```sh
mkdir -p "$HOME/.local/bin" "$HOME/.local/share/compiler"
cp -R compiler-0.1.0-osx-arm64 "$HOME/.local/share/compiler/"
ln -s "$HOME/.local/share/compiler/compiler-0.1.0-osx-arm64/Compiler" "$HOME/.local/bin/Compiler"
export PATH="$HOME/.local/bin:$PATH"
Compiler --version
```

Add the `export PATH` line to your shell's startup file for future terminals.
If `Compiler` already exists in that directory, inspect the existing installation
before replacing it. Keep `LICENSE` and `THIRD-PARTY-NOTICES.txt` with your copy.
Optional debug symbols are a separate download and are not needed at runtime.
See the [release notes](docs/releases/v0.1.0.md) for scope and remaining release
checks. There is no Intel or universal compiler download.

## Build from source

### Prerequisites

- .NET 11 RC1 SDK **11.0.100-rc.1.26425.128**, pinned in [global.json](global.json).
- GCC on `PATH` (Apple's Clang-backed `gcc` works).
- macOS with x86-64 assembly and linking support. On Apple Silicon, Rosetta 2
  is needed to run the custom compiler's executables.

See the [development guide](docs/development.md#setup) for installation steps
and SDK troubleshooting.

### Quick start

With Git and the prerequisites installed, run:

```sh
git clone https://github.com/zachdillard/compiler.git
cd compiler
dotnet build -m:1 --disable-build-servers
dotnet run --project src/Compiler.csproj -- data/return_2.c
./data/return_2
echo $? # Expected: 2
```

The compiler creates `data/return_2` beside the source file. The generated
program prints nothing and exits with status `2`, which `echo $?` displays.

## Source usage and compiler options

For source development, run commands from the repository root. The `--`
separates `dotnet run` options from the arguments passed to the compiler:
its options and one C source path.
The source path must end in `.c`; other extensions are rejected before GCC runs.
For example, to inspect the custom compiler's assembly:

```sh
dotnet run --project src/Compiler.csproj -- -S data/return_2.c
```

| Option | What it does |
| --- | --- |
| `--help`, `-h` | Show generated help for the compiler's arguments and options. |
| `--version` | Print the version without GCC; use alone, with no source path. |
| `--lex` | Check tokens without printing them or generating output files. |
| `--parse` | Parse the C program without generating output files. |
| `--codegen` | Generate the assembly AST without emitting output files. |
| `-S` | Write custom assembly without linking an executable. |

Assembly-only mode leaves `data/return_2.s`. Successful executable builds remove
intermediate `.i` and `.s` files. All stage-only modes remove `.i`.
The stage flags (`--lex`, `--parse`, `--codegen`, and `-S`) are mutually exclusive
and may appear before or after the source path. Each exits with `0` when its
stages succeed and a nonzero status on an error. `--codegen` stops before
assembly text emission; only `-S` writes a `.s` file.
Integer constants must fit in a C# `int`; out-of-range constants report an error
and exit with status `1` in every compilation mode.

Cleanup is also attempted after preprocessing fails. If a compiler file cannot
be read, written, or removed, the compiler reports a diagnostic on standard error
and exits with status `1`.

The binary accepts the same compiler options directly, for example
`Compiler --version` or `Compiler -S return_2.c`, without `dotnet run` or its `--`
separator.

## Testing

Run the project tests from the repository root:

```sh
dotnet test
```

Integration tests need GCC and, on Apple Silicon, Rosetta 2. The
[development guide](docs/development.md#testing) covers the optional book suite,
Debug/Release checks, and CI.

## Further reading

- [Compiler architecture](docs/architecture.md): grammar, pipeline, and compiler stages.
- [C AST](docs/c-ast.md) and [assembly AST](docs/asm-ast.md): current intermediate representations.
- [Development guide](docs/development.md): setup, debugging, testing, and versioning.
- [Concepts guide](concepts/README.md): standalone C# examples to explore.
- [Chapter 1 notes](notes/chapter-1.md) and [chapter 2 notes](notes/chapter-2.md): working notes for the book chapters.
- [Changelog](CHANGELOG.md): release history and current limitations.

## License

[MIT](LICENSE).
