# Repository Instructions

- This is a learning, non-production project.
- The project is a C#/.NET 10 implementation of the C compiler described in *Writing a C Compiler* by Nora Sandler. Keep compiler stages clear, testable, and easy to extend.
- The .NET 10 SDK and GCC on `PATH` are required.
- Keep changes simple, clear, and easy to understand.
- Keep commit messages simple unless additional detail is required.
- From the repository root, use `dotnet build` to build and `dotnet test` to run integration tests. Tests require GCC on `PATH`.
- Run the book's chapter tests with `./tests/run_book_tests.sh --chapter <number>`. The script forwards options to the book's `test_compiler` runner; use `BOOK_TESTS_DIR` when the test checkout is elsewhere.
- The compiler accepts one C source path: `dotnet run -- <source-file>`. This selects the custom compiler implementation, which is currently a placeholder and returns failure after preprocessing.
- Use `dotnet run -- -gcc <source-file>` to select the temporary GCC-backed compiler. GCC preprocessing, assembly generation, and linking produce an executable beside the input; intermediate `.i` and `.s` files are removed after successful stages.
- Use `dotnet run -- -S <source-file>` for custom assembly-only mode, or combine `-S` and `-gcc` in either order (`dotnet run -- -gcc -S <source-file>`) for GCC assembly output without linking. The intermediate `.i` file is removed.
- When adding a new C source file under `data/`, add its generated output file to `.gitignore`.
