# Release Requirements (v1.0.0)

This is a learning project. Here, "production" means the compiler is
complete, correct, reproducible, and well-documented *within the scope of
Writing a C Compiler*. It does not mean a general-purpose C compiler.

`v1.0.0` ships once every item below is checked. The chapter releases leading
up to it are listed in [roadmap.md](roadmap.md).

## 1. Language Completeness

- [ ] `v0.20.0` is released: every chapter in the book is complete.
- [ ] `./tests/run_book_tests.sh --chapter 20` passes for chapters 1–20.
- [ ] The Chapter 19 tests pass with `--eliminate-dead-stores`, which enables
      all four optimizations.
- [ ] The Chapter 20 tests pass both with and without `--no-coalescing`.
- [ ] Extra-credit features are optional. Any that are implemented pass their
      tests.

## 2. Custom Compiler Only

- [ ] The GCC-backed `-gcc` option and the GCC call in `src/Compiler.cs` are
      removed.
- [ ] GCC is used only where the book uses it: preprocessing, assembling, and
      linking.
- [ ] Nothing in `src/` depends on prototypes in `concepts/`. The `concepts/`
      folder stays as learning material.

## 3. Driver and Command Line

- [ ] Every flag the book's test runner uses is supported: `--lex`,
      `--parse`, `--validate`, `--tacky`, `--codegen`, `-S`, `-c`,
      `-l<lib>`, `--fold-constants`, `--eliminate-unreachable-code`,
      `--propagate-copies`, and `--eliminate-dead-stores`.
- [ ] `--optimize` enables all four optimizations at once.
- [ ] `--help` prints usage, and `--version` prints the release version.
- [ ] Exit codes are documented and consistent: `0` for success, and
      separate codes for invalid programs, usage errors, and toolchain
      failures.
- [ ] Intermediate `.i` and `.s` files are cleaned up on both success and
      failure.

## 4. Diagnostics

- [ ] No input program causes an unhandled exception or stack trace.
- [ ] Every error reports `file:line:column`, the failing stage, and a
      readable message.
- [ ] Invalid programs are rejected with a nonzero exit code. The book suite
      passes with `--expected-error-codes`, so rejections are distinguished
      from crashes.

## 5. Testing

- [ ] Each stage has unit tests: lexer, parser, semantic analysis, TACKY
      generation, optimizer, assembly generation, register allocation, and
      emitter.
- [ ] Integration tests in `tests/Compiler.Tests` cover the driver's flags,
      output files, and error paths.
- [ ] The full book suite runs in CI.

## 6. Platforms and Toolchain

- [ ] The project builds on the .NET 11 GA SDK, not RC1, and `global.json` is
      updated to match.
- [ ] Supported targets are documented and tested:
  - x86-64 Linux.
  - x86-64 macOS: native on Intel, and through Rosetta (`arch -x86_64`) on
    Apple Silicon.
- [ ] Platform differences such as macOS symbol prefixes and Linux
      `.note.GNU-stack` sections are handled in the emitter.

## 7. Continuous Integration and Delivery

- [ ] A GitHub Actions workflow runs on every push and pull request. It
      builds the project, runs `dotnet test`, and runs the book suite on an
      x86-64 Linux runner.
- [ ] The workflow fetches `writing-a-c-compiler-tests` itself and doesn't
      depend on a local path.
- [ ] A release workflow runs on `v*` tags and publishes the binaries.

## 8. Reproducible Test Tooling

- [ ] `tests/run_book_tests.sh` no longer hard-codes a personal checkout path
      or `bin/Debug`. Its default works on a fresh clone, for example through
      a pinned submodule or a clone under the repository.
- [ ] The book test suite is pinned to a known revision.

## 9. Packaging and Versioning

- [ ] `dotnet publish` produces runnable binaries for the supported platforms.
- [ ] The version is embedded in the binary from the git tag.
- [ ] `CHANGELOG.md` has an entry for every release.

## 10. Code Health

- [ ] Each stage lives in its own file in `src/Compiler/`, with a clear
      input and output type: Lexer, Parser, SemanticAnalysis,
      TackyGenerator, Optimizer, AssemblyGenerator, RegisterAllocator, and
      Emitter.
- [ ] No stub or dead files remain in `src/`.
- [ ] The build has no warnings, and `TreatWarningsAsErrors` is enabled.
- [ ] `dotnet format --verify-no-changes` passes.

## 11. Documentation

- [ ] The README describes the finished compiler, its usage, and its
      supported platforms.
- [ ] `docs/formal-grammar.md`, `docs/ast.md`, `docs/tacky.md`, and
      `docs/asm-ast.md` cover the complete language.
- [ ] An architecture overview explains the compiler pipeline from source
      to executable.
- [ ] `AGENTS.md` matches the current behavior of the project.
- [ ] The repository has a `LICENSE`.

## 12. Performance

- [ ] The full book suite finishes in CI without timeouts.
- [ ] Optimized output passes the book's Chapter 19 and 20 checks on
      instruction counts and spills.

## Non-Goals

These are out of scope for `v1.0.0`:

- Full C17 or C23 conformance beyond the book's subset.
- A custom preprocessor, assembler, or linker.
- Backends for architectures other than x86-64, such as native arm64.
- Debug information (DWARF).
