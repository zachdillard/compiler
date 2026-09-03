# Abstract Syntax Tree

The parser turns the lexer's token list into an abstract syntax tree (AST).
The nodes below cover the subset of C supported so far: a single `main`
function whose body is one `return` statement with a constant operand.

## Definition

Using the book's ASDL notation:

```
program = Program(function_definition)
function_definition = Function(identifier name, statement body)
statement = Return(exp)
exp = Constant(int)
```

## Nodes

Each production has a matching record in `src/Compiler/Ast.cs`:

| Node                      | Description                                             |
| ------------------------- | ------------------------------------------------------- |
| `Ast`                     | Base record for every node.                             |
| `ProgramNode`             | Whole translation unit; holds one function.             |
| `FunctionNode`            | Function name and its body.                             |
| `StatementNode`           | Base record for statements.                             |
| `ReturnStatementNode`     | `return <exp>;` with the returned expression.           |
| `ExpressionNode`          | Base record for expressions.                            |
| `ConstantExpressionNode`  | An integer constant.                                    |

The nodes are C# records, so they compare by value and print themselves as a
readable tree. That is what `--parse` writes to standard output:

```sh
dotnet run -- --parse data/return_2.c
```

```
ProgramNode { Function = FunctionNode { Identifier = main, Statement = ReturnStatementNode { Expression = ConstantExpressionNode { Constant = 2 } } } }
```

## Grammar

The parser is a recursive descent parser in `src/Compiler/Parser.cs`, with one
method per production:

```
<program>   ::= <function>
<function>  ::= "int" <identifier> "(" "void" ")" "{" <statement> "}"
<statement> ::= "return" <exp> ";"
<exp>       ::= <int>
```

Each method consumes tokens from the front of the list and throws an
`InvalidOperationException` when the next token is not the one the grammar
requires. `Parser.Run` also rejects leftover tokens after the function, so
trailing junk is a syntax error rather than a silently ignored suffix.
