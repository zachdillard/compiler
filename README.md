# C Compiler

This project is a C# and .NET 10 implementation of the C compiler described in [*Writing a C Compiler*](https://nostarch.com/writing-c-compiler) by Nora Sandler.

The implementation follows the book's progression from parsing C source through semantic analysis and code generation, with the goal of making each compiler stage clear, testable, and easy to extend.

## Requirements

- .NET 10 SDK
- GCC available on `PATH`

## Usage

Build the compiler from the repository root:

```sh
dotnet build
```

Compile a C source file with the custom compiler by passing its path as the only argument:

```sh
dotnet run -- data/return_2.c
```

The custom compiler currently implements lexing, but later stages are still
under development. To stop after lexing and print the recognized tokens, pass
`--lex`:

```sh
dotnet run -- --lex data/return_2.c
```

A lexically valid file exits with status `0`; an invalid token produces a
nonzero exit status. Lex-only mode does not create assembly or an executable.

To test a specific file from the book's test suite, pass its path directly:

```sh
dotnet run -- --lex /path/to/writing-a-c-compiler-tests/tests/chapter_1/valid/return_2.c
```

To use the temporary GCC-backed compiler, pass `-gcc`:

```sh
dotnet run -- -gcc data/return_2.c
```

GCC preprocessing, assembly generation, and linking create an executable
beside the input file. The example above creates `data/return_2`.
Intermediate `.i` and `.s` files are removed after each successful stage.

To generate assembly without linking an executable, use `-S`:

```sh
dotnet run -- -S data/return_2.c
```

This selects the custom compiler's assembly-only mode. To generate assembly
with GCC, combine `-S` and `-gcc` in either order:

```sh
dotnet run -- -gcc -S data/return_2.c
dotnet run -- -S -gcc data/return_2.c
```

These commands create `data/return_2.s` and remove the intermediate `.i` file.

You can run the generated executable with:

```sh
./data/return_2
echo $?
```

The GCC example exits with status `2`.

To run the book's chapter tests manually, use the test runner in `tests/`:

```sh
./tests/run_book_tests.sh --chapter 1 --stage lex
```

This runs the chapter-one lexer tests, including lexically valid and invalid
programs. The script forwards options to the book's `test_compiler` runner.
Set `BOOK_TESTS_DIR` if the test checkout is in a different location. See the
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
dotnet basic-regex-lexer.cs
```

## Status

> [!WARNING]
> This project is under development. Additional compiler stages and language
> features will be added as the implementation progresses through the book.
