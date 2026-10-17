# C Compiler @VERSION@ for Apple Silicon

This chapter-one learning compiler is a native ARM64 executable with its .NET
runtime included. It generates macOS **x86-64** programs. It supports one `int`
function with `void` parameters returning an integer constant. Later chapters
and extra credit are not implemented.

The intended minimum is **macOS 14.0**. Runtime checks on macOS 14 and 15 and
the final download walkthrough must pass before publication; deployment metadata
alone does not establish compatibility. No Intel compiler download is provided.

Compilation requires Apple's Command Line Tools (`xcode-select --install`,
then check `gcc --version`). Rosetta 2 is required to execute generated x86-64
programs on Apple Silicon, but not to run this ARM64 compiler. Install Rosetta
when macOS prompts while opening an Intel program; Apple's instructions are at
https://support.apple.com/en-us/102527. Binary users do not need .NET or Git.

## Verify and extract

Download `compiler-@VERSION@-osx-arm64.tar.gz` and `SHA256SUMS` from the same
release at https://github.com/zachdillard/compiler/releases/tag/v@VERSION@.
In their download directory, check only the runtime archive (the checksum file
may also list an optional symbols archive):

```sh
awk '$2 == "compiler-@VERSION@-osx-arm64.tar.gz"' SHA256SUMS > runtime.sha256
test -s runtime.sha256 && shasum -a 256 -c runtime.sha256
```

Continue only after it prints `compiler-@VERSION@-osx-arm64.tar.gz: OK`:

```sh
tar -xzf compiler-@VERSION@-osx-arm64.tar.gz
./compiler-@VERSION@-osx-arm64/Compiler --version
# Compiler @VERSION@
```

## Approve this download

This release is **unsigned for distribution and not notarized**. Its linker
provides only an ad hoc code signature, not a Developer ID identity. Browser
quarantine may cause macOS to block the version command. If you trust this
verified download, dismiss the alert, open **System Settings → Privacy &
Security**, scroll to **Security**, and choose **Open Anyway** for `Compiler`.
Authenticate if requested, confirm **Open**, then repeat the version command.
See https://support.apple.com/en-us/102445 for Apple's per-item process. Do not
disable Gatekeeper globally or remove quarantine recursively. If no approval is
offered or macOS reports damage or malware, stop and report the exact alert.
The final browser walkthrough is a publication gate, tracked in task 08.

## Optional user installation

Keep the license and notices with the executable:

```sh
mkdir -p "$HOME/.local/bin" "$HOME/.local/share/compiler"
cp -R compiler-@VERSION@-osx-arm64 "$HOME/.local/share/compiler/"
ln -s "$HOME/.local/share/compiler/compiler-@VERSION@-osx-arm64/Compiler" "$HOME/.local/bin/Compiler"
export PATH="$HOME/.local/bin:$PATH"
Compiler --version
```

Add the `export PATH` line to your shell's startup file to keep it for future
terminals. The `ln` command fails if `Compiler` already exists; inspect that
installation before replacing it. You can also use the extracted executable
directly without installing it.

## Compile a program

With the optional installation above (otherwise use the full extracted path):

```sh
printf 'int main(void) { return 2; }\n' > return_2.c
Compiler return_2.c
./return_2
echo $? # Expected: 2; the program prints nothing
Compiler -S return_2.c
```

`-S` writes `return_2.s` without linking. `--lex`, `--parse`, and `--codegen`
stop after intermediate stages without emitting files. These four stage flags
are mutually exclusive. Sources must end in `.c`; integer constants must fit
in a signed 32-bit integer. Normal compilation removes `.i` and `.s`;
assembly-only compilation removes `.i` and leaves `.s`.

`LICENSE` covers this compiler. `THIRD-PARTY-NOTICES.txt` contains the bundled
.NET and System.CommandLine license texts/notices. Optional symbols are a
separate download and are not required to run the compiler. Build provenance
and archive checksums are recorded in the release's `BUILD-MANIFEST.json`.
