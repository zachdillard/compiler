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

Compile a C source file by passing its path as the only argument:

```sh
dotnet run -- data/return_2.c
```

The compiler preprocesses the source with GCC, generates assembly, and links
the result into an executable beside the input file. The example above creates
`data/return_2`. Intermediate `.i` and `.s` files are removed after each
successful stage.

To generate assembly without linking an executable, use `-S`:

```sh
dotnet run -- -S data/return_2.c
```

This creates `data/return_2.s` and removes the intermediate `.i` file.

You can run the generated executable with:

```sh
./data/return_2
echo $?
```

The example exits with status `2`.

Run the integration tests from the repository root after building the compiler:

```sh
dotnet build
dotnet test tests/Compiler.Tests/Compiler.Tests.csproj --no-restore
```

The tests require GCC to be available on `PATH`.

## Status

> [!WARNING]
> This project is under development. Additional compiler stages and language
> features will be added as the implementation progresses through the book.
