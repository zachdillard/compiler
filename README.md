# C Compiler

This project is a C# and .NET 11 implementation of the C compiler described in [*Writing a C Compiler*](https://nostarch.com/writing-c-compiler) by Nora Sandler.

The implementation follows the book's progression from parsing C source through semantic analysis and code generation, with the goal of making each compiler stage clear, testable, and easy to extend.

## Requirements

- .NET 11 SDK
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

The custom compiler is still under development, so this mode currently exits
with failure after preprocessing. To use the temporary GCC-backed compiler,
pass `-gcc`:

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
./tests/run_book_tests.sh --chapter 1
```

The script forwards options to the book's `test_compiler` runner. Set
`BOOK_TESTS_DIR` if the test checkout is in a different location.

Run the integration tests from the repository root:

```sh
dotnet test
```

The tests require GCC to be available on `PATH`.

## Status

> [!WARNING]
> This project is under development. Additional compiler stages and language
> features will be added as the implementation progresses through the book.
