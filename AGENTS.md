# Repository Instructions

- This is a learning, non-production project.
- The project is a C#/.NET 11 implementation of the C compiler described in *Writing a C Compiler* by Nora Sandler. Keep compiler stages clear, testable, and easy to extend.
- The .NET 11 SDK and GCC on `PATH` are required.
- Keep changes simple, clear, and easy to understand.
- Keep commit messages simple unless additional detail is required.
- From the repository root, use `dotnet build` to build and `dotnet test` to run integration tests. Tests require GCC on `PATH`.
- Run the book's chapter tests with `./tests/run_book_tests.sh --chapter <number>`. The script forwards options to the book's `test_compiler` runner; use `BOOK_TESTS_DIR` when the test checkout is elsewhere.
- The compiler accepts one C source path: `dotnet run -- data/return_2.c`. Normal compilation preprocesses with GCC, generates assembly, links an executable beside the input, and removes intermediate `.i` and `.s` files after successful stages.
- Use `dotnet run -- -S <source-file>` to retain generated assembly (`.s`) without linking; the intermediate `.i` file is removed.
- When adding a new C source file under `data/`, add its generated output file to `.gitignore`.
