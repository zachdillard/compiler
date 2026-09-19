# C Compiler

This project is a C# and .NET 11 RC1 implementation of the C compiler described in [*Writing a C Compiler*](https://nostarch.com/writing-c-compiler) by Nora Sandler.

The implementation follows the book's progression from parsing C source through semantic analysis and code generation, with the goal of making each compiler stage clear, testable, and easy to extend.

## Requirements

- .NET 11 RC1 SDK
- GCC available on `PATH`

## Usage

Build the compiler from the repository root:

```sh
dotnet build
```

Compile a C source file with the custom compiler by passing its path as the only
argument:

```sh
dotnet run --project Compiler -- data/return_2.c
```

This runs the full pipeline: preprocessing, lexing, parsing, assembly
generation, and code emission, followed by assembling and linking. The example
above creates the executable `data/return_2`. Intermediate `.i` and `.s` files
are removed after each successful stage.

The compiler emits x86-64 assembly in AT&T syntax and uses macOS symbol
conventions, so `main` is emitted as `_main`. On Apple silicon, `gcc` is invoked
with `-arch x86_64` and the resulting executables run under Rosetta 2.

To stop after an intermediate stage, pass `--lex`, `--parse`, or `--codegen`:

```sh
dotnet run --project Compiler -- --lex data/return_2.c
dotnet run --project Compiler -- --parse data/return_2.c
dotnet run --project Compiler -- --codegen data/return_2.c
```

Lex-only mode prints the recognized tokens. None of these stages create
assembly or an executable. A valid file exits with status `0`; an invalid one
prints a diagnostic and exits with a nonzero status.

To test a specific file from the book's test suite, pass its path directly:

```sh
dotnet run --project Compiler -- --lex /path/to/writing-a-c-compiler-tests/tests/chapter_1/valid/return_2.c
```

To generate assembly without linking an executable, use `-S`:

```sh
dotnet run --project Compiler -- -S data/return_2.c
```

This creates `data/return_2.s`:

```asm
	.globl _main
_main:
	movl $2, %eax
	ret
```

To use the temporary GCC-backed compiler, pass `-gcc`:

```sh
dotnet run --project Compiler -- -gcc data/return_2.c
```

GCC preprocessing, assembly generation, and linking create an executable beside
the input file. To generate assembly with GCC, combine `-S` and `-gcc` in either
order:

```sh
dotnet run --project Compiler -- -gcc -S data/return_2.c
dotnet run --project Compiler -- -S -gcc data/return_2.c
```

You can run the generated executable with:

```sh
./data/return_2
echo $?
```

The example exits with status `2`.

To run the book's chapter tests manually, use the test runner in `tests/`:

```sh
./tests/run_book_tests.sh --chapter 1
```

This runs the full chapter-one suite. Pass `--stage lex`, `--stage parse`, or
`--stage codegen` to test a single stage. The script forwards options to the
book's `test_compiler` runner. Set `BOOK_TESTS_DIR` if the test checkout is in a
different location. See the
[`writing-a-c-compiler-tests`](https://github.com/nlsandler/writing-a-c-compiler-tests)
repository for additional test runner usage.

Run the integration tests from the repository root:

```sh
dotnet test
```

The tests require GCC to be available on `PATH`.

## Concepts

The `concepts/` folder contains self-contained, file-based C# examples. Each
example can be run individually from that folder with:

```sh
cd concepts
dotnet <concept-name>.cs
```

For example:

```sh
dotnet lexer.cs
```

## Status

> [!WARNING]
> This project is under development. The compiler implements chapter one of the
> book: it compiles a `main` function that returns a constant. Additional
> language features will be added as the implementation progresses through the
> book.
