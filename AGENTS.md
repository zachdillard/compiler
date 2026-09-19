# Repository Instructions

- This is a learning, non-production project.
- The project is a C#/.NET 11 RC1 implementation of the C compiler described in *Writing a C Compiler* by Nora Sandler. Keep compiler stages clear, testable, and easy to extend.
- The .NET 11 RC1 SDK and GCC on `PATH` are required.
- Keep changes simple, clear, and easy to understand.
- Keep commit messages simple unless additional detail is required.
- From the repository root, use `dotnet build` to build and `dotnet test` to run integration tests. Tests require GCC on `PATH`.
- Run the book's chapter tests with `./tests/run_book_tests.sh --chapter <number>`. The script forwards options to the book's `test_compiler` runner; use `BOOK_TESTS_DIR` when the test checkout is elsewhere.
- The compiler accepts one C source path: `dotnet run -- <source-file>`. This selects the custom compiler implementation, which lexes, parses, generates assembly, emits it, and then assembles and links an executable beside the input.
- The custom compiler emits x86-64 assembly in AT&T syntax with macOS symbol conventions. GCC is invoked with `-arch x86_64`, so executables run under Rosetta 2 on Apple silicon.
- Use `dotnet run -- --lex <source-file>`, `--parse`, or `--codegen` to stop after an intermediate stage. These stages must not leave an assembly file or an executable behind; the book's test runner checks for them.
- Use `dotnet run -- -gcc <source-file>` to select the temporary GCC-backed compiler. GCC preprocessing, assembly generation, and linking produce an executable beside the input; intermediate `.i` and `.s` files are removed after successful stages.
- Use `dotnet run -- -S <source-file>` for custom assembly-only mode, or combine `-S` and `-gcc` in either order (`dotnet run -- -gcc -S <source-file>`) for GCC assembly output without linking. The intermediate `.i` file is removed.
- Compiler stages live in `src/Compiler/` and follow the style of `concepts/compiler.cs`: four-space indentation, explicit types, and `union`/`record` declarations.
- The `concepts/` folder contains self-contained, file-based C# examples. From that folder, run an example individually with `dotnet <concept-name>.cs`.
- When adding a new C source file under `data/`, add its generated output file to `.gitignore`.
