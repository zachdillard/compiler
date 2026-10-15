# Diagnostic examples

These examples were run on macOS with Apple's Clang-backed `gcc`. Each exits
with status `1` and writes its diagnostics to standard error. GCC/Clang and
operating-system error wording can differ between machines.

From the repository root, compile an example with:

```sh
dotnet run --project src/Compiler.csproj -- path/to/example.c
```

The examples below use source arguments containing only the filenames, so
their diagnostics show those filenames rather than longer paths.

## Command-line arguments

Pass `--lex=true lex.c`; the `--lex` flag takes no value.

```text
Error: option '--lex=true' does not accept a value.
Usage: Compiler [-S] [--lex] <source-file>
```

## Preprocessing

A missing include is diagnosed by GCC and forwarded unchanged.

```c
#include "missing.h"
```

```text
preprocess.c:1:10: fatal error: 'missing.h' file not found
    1 | #include "missing.h"
      |          ^~~~~~~~~~~
1 error generated.
```

## Lexing

The entire malformed integer token is highlighted.

```c
int main(void) { return 123abc; }
```

```text
lex.c:1:25: error: invalid integer constant '123abc'; only decimal digits are supported.
1 | int main(void) { return 123abc; }
                            ^~~~~~
note: columns and source excerpts refer to preprocessed text.
```

## Parsing

A missing semicolon identifies the token encountered instead.

```c
int main(void) {
    return 7
}
```

```text
parse.c:3:1: error: expected ';' after the return statement; found '}'.
3 | }
    ^
note: columns and source excerpts refer to preprocessed text.
```

## Parsing at end of input

A missing closing brace points back to its opening brace.

```c
int main(void) {
    return 7;
```

```text
eof.c:3:1: error: expected '}' to close the function body; found end of input.
3 |
    ^
eof.c:1:16: note: to match this '{'.
1 | int main(void) {
                   ^
note: columns and source excerpts refer to preprocessed text.
```

## Writing assembly

Use `-S write.c` after creating a directory named `write.s`, which prevents writing the assembly file.

```c
int main(void) { return 7; }
```

```text
Error: could not access a compiler file: Access to the path '/private/tmp/compiler-diagnostics-examples/write.s' is denied.
```

## Assembling

The current emitter produces only supported instructions, so no accepted C program naturally reproduces an assembler syntax error. For this example, a temporary GCC wrapper replaced the generated `assemble.s` with `invalid_opcode` before invoking the real assembler. This is deliberate fault injection, and the output is what the compiler forwarded.

```text
assemble.s:1:1: error: invalid instruction mnemonic 'invalid_opcode'
invalid_opcode
^~~~~~~~~~~~~~
Error: could not assemble and link the program.
```

## Linking

This function compiles and assembles successfully, but the executable has no `main` entry point.

```c
int other(void) { return 7; }
```

```text
Undefined symbols for architecture x86_64:
  "_main", referenced from:
      <initial-undefines>
ld: symbol(s) not found for architecture x86_64
clang: error: linker command failed with exit code 1 (use -v to see invocation)
Error: could not assemble and link the program.
```

## Code generation and emission

The generator lowers a parsed return constant to `movl` and `ret`; the emitter
formats that assembly. Every program accepted by the current parser has a
supported representation in these stages. They have no independent
source-triggered diagnostic to demonstrate. An unknown instruction or operand
in an internally constructed assembly AST would be a compiler implementation
error rather than a C source error.

## Source locations and current limits

Custom lexer/parser diagnostics use original filenames and logical line numbers
from GCC's line markers. This also works inside includes and with `#line`.
Columns and excerpts describe preprocessed text, including expanded macros;
they are not exact original macro-token locations. A reminder is printed with
custom diagnostics. Token ranges use `^` followed by `~`; end-of-input uses a
single caret. Related delimiter notes have their own location and excerpt.

Unsupported operators explain that they are not implemented yet, for example
`unary '-' is not supported yet.` for `return -1;`. Out-of-range integer
messages include the literal and supported range, for example
`integer constant '2147483648' is outside the supported int range (0 to 2147483647).`

The compiler stops at its first lexer/parser error. Semantic warnings for
variables, types, and control flow will become relevant as those language
features are implemented.
