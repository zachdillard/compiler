# Release Roadmap

Each release marks the completion of one chapter of *Writing a C Compiler*.
The requirements for the final `v1.0.0` release are in
[release-requirements.md](release-requirements.md).

## Versioning

- `v0.N.0`: Chapter N is complete.
- `v0.N.x`: fixes or stretch goals added after the Chapter N release.
- `v1.0.0`: production release. See [release-requirements.md](release-requirements.md).

Every release is tagged in git (`git tag v0.N.0`).

## Milestones

| Milestone | Release | Book section |
|---|---|---|
| Part I: The Basics | `v0.10.0` | Chapters 1–10 |
| Part II: Types Beyond Int | `v0.18.0` | Chapters 11–18 |
| Part III: Optimizations | `v0.20.0` | Chapters 19–20 |
| Production | `v1.0.0` | Everything above, plus the release requirements |

## Definition of Done

A chapter release is ready when all of the following are true:

- [ ] `./tests/run_book_tests.sh --chapter N` passes. The runner also re-runs
      every earlier chapter by default, so this is the regression check too.
- [ ] `dotnet test` passes, with new tests for the chapter's new behavior.
- [ ] The driver accepts every flag the chapter's tests use.
- [ ] `docs/formal-grammar.md`, `docs/ast.md`, and `docs/asm-ast.md` are
      updated. From Chapter 2 onward, `docs/tacky.md` is updated as well.
- [ ] The README status is updated and a CHANGELOG entry is added.
- [ ] The release is tagged `v0.N.0`.

Stretch goals are the book's extra-credit features. They are optional and
never block a release. When one is done, test it with the runner's matching
flag (for example `--bitwise`), or test all of them with `--extra-credit`.

## Status

| Release | Chapter | Status |
|---|---|---|
| `v0.1.0` | 1. A Minimal Compiler | In progress |
| `v0.2.0` | 2. Unary Operators | Planned |
| `v0.3.0` | 3. Binary Operators | Planned |
| `v0.4.0` | 4. Logical and Relational Operators | Planned |
| `v0.5.0` | 5. Local Variables | Planned |
| `v0.6.0` | 6. If Statements and Conditional Expressions | Planned |
| `v0.7.0` | 7. Compound Statements | Planned |
| `v0.8.0` | 8. Loops | Planned |
| `v0.9.0` | 9. Functions | Planned |
| `v0.10.0` | 10. File Scope Variables and Storage-Class Specifiers | Planned |
| `v0.11.0` | 11. Long Integers | Planned |
| `v0.12.0` | 12. Unsigned Integers | Planned |
| `v0.13.0` | 13. Floating-Point Numbers | Planned |
| `v0.14.0` | 14. Pointers | Planned |
| `v0.15.0` | 15. Arrays and Pointer Arithmetic | Planned |
| `v0.16.0` | 16. Characters and Strings | Planned |
| `v0.17.0` | 17. Supporting Dynamic Memory Allocation | Planned |
| `v0.18.0` | 18. Structures | Planned |
| `v0.19.0` | 19. Optimizing TACKY Programs | Planned |
| `v0.20.0` | 20. Register Allocation | Planned |
| `v1.0.0` | Production | Planned |

---

## Part I: The Basics

### v0.1.0: A Minimal Compiler (Chapter 1)

- **Language:** `int main(void) { return <int>; }`
- **Stages:** lexer, parser, assembly generation, code emission.
- **Driver:** `--lex`, `--parse`, `--codegen`, `-S`.

Remaining work:

- [x] Lexer (`src/Compiler/Lexer.cs`) and `--lex`.
- [ ] Move the parser from `concepts/compiler.cs` into `src/Compiler/Parser.cs`
      and add `--parse`.
- [ ] Move assembly generation into `src/Compiler/Generator.cs` and add
      `--codegen`.
- [ ] Move code emission into `src/Compiler/Emitter.cs`, including the `_`
      symbol prefix on macOS.
- [ ] The custom compiler handles both `-S` and full compilation end to end.
- [ ] `./tests/run_book_tests.sh --chapter 1` passes.

### v0.2.0: Unary Operators (Chapter 2)

- **Language:** negation `-`, bitwise complement `~`, parenthesized expressions.
- **Stages:** TACKY intermediate representation, pseudo-register replacement,
  instruction fix-up, stack frame allocation.
- **Driver:** `--tacky`.
- **Docs:** add `docs/tacky.md`.

### v0.3.0: Binary Operators (Chapter 3)

- **Language:** `+`, `-`, `*`, `/`, `%`.
- **Stages:** precedence climbing in the parser. `idiv` and `cdq` in the code
  generator.
- **Stretch:** bitwise `&`, `|`, `^`, `<<`, `>>` (`--bitwise`).

### v0.4.0: Logical and Relational Operators (Chapter 4)

- **Language:** `!`, `&&`, `||`, `==`, `!=`, `<`, `>`, `<=`, `>=`.
- **Stages:** short-circuit evaluation with jumps and labels. `cmp` and
  `SetCC` in the code generator.

### v0.5.0: Local Variables (Chapter 5)

- **Language:** variable declarations, assignment expressions.
- **Stages:** semantic analysis (variable resolution).
- **Driver:** `--validate`.
- **Stretch:** compound assignment (`--compound`), `++` and `--`
  (`--increment`).

### v0.6.0: If Statements and Conditional Expressions (Chapter 6)

- **Language:** `if` and `else`, the conditional operator `?:`.
- **Stretch:** labeled statements and `goto` (`--goto`).

### v0.7.0: Compound Statements (Chapter 7)

- **Language:** blocks and nested scopes.
- **Stages:** scope-aware variable resolution.

### v0.8.0: Loops (Chapter 8)

- **Language:** `while`, `do`, `for`, `break`, `continue`.
- **Stages:** loop-labeling pass.
- **Stretch:** `switch` statements (`--switch`).

### v0.9.0: Functions (Chapter 9)

- **Language:** function declarations, definitions, and calls.
- **Stages:** type checker. System V AMD64 calling convention.
- **Driver:** `-c` (emit an object file). Link programs made of several files.

### v0.10.0: File Scope Variables and Storage-Class Specifiers (Chapter 10)

- **Language:** file-scope variables, `static`, `extern`.
- **Stages:** symbol table with linkage and storage duration. `.data` and
  `.bss` sections.
- **Milestone:** Part I is complete.

## Part II: Types Beyond Int

### v0.11.0: Long Integers (Chapter 11)

- **Language:** `long` and casts.
- **Stages:** typed AST, implicit conversions, 64-bit operands.

### v0.12.0: Unsigned Integers (Chapter 12)

- **Language:** `unsigned int` and `unsigned long`.
- **Stages:** unsigned arithmetic, comparisons, and conversions.

### v0.13.0: Floating-Point Numbers (Chapter 13)

- **Language:** `double`.
- **Stages:** SSE registers and instructions. Floating-point constants and
  conversions.
- **Driver:** `-l<lib>` passed to the linker (for example `-lm`).
- **Stretch:** NaN handling (`--nan`).

### v0.14.0: Pointers (Chapter 14)

- **Language:** address-of `&`, dereference `*`, pointer types.
- **Stages:** declarator parsing, pointer type checking.

### v0.15.0: Arrays and Pointer Arithmetic (Chapter 15)

- **Language:** arrays, subscripts, compound initializers, pointer arithmetic.
- **Stages:** aggregate layout, array-to-pointer decay.

### v0.16.0: Characters and Strings (Chapter 16)

- **Language:** `char`, `signed char`, `unsigned char`, character constants,
  string literals.
- **Stages:** read-only data, character conversions.

### v0.17.0: Supporting Dynamic Memory Allocation (Chapter 17)

- **Language:** `void`, `sizeof`, `void *`.
- **Stages:** incomplete types. Calls to `malloc` and related functions.

### v0.18.0: Structures (Chapter 18)

- **Language:** `struct` declarations, member access `.` and `->`.
- **Stages:** structure layout. System V classification for passing and
  returning structures.
- **Stretch:** unions (`--union`).
- **Milestone:** Part II is complete.

## Part III: Optimizations

### v0.19.0: Optimizing TACKY Programs (Chapter 19)

- **Stages:** control-flow graph and dataflow analysis. Constant folding,
  unreachable code elimination, copy propagation, dead store elimination.
- **Driver:** `--fold-constants`, `--eliminate-unreachable-code`,
  `--propagate-copies`, `--eliminate-dead-stores`, and `--optimize` to enable
  all four.

### v0.20.0: Register Allocation (Chapter 20)

- **Stages:** liveness analysis, interference graph, graph coloring, spilling,
  register coalescing.
- **Milestone:** Part III is complete, and so is the book.

## v1.0.0: Production

`v1.0.0` ships once every item in
[release-requirements.md](release-requirements.md) is met.
