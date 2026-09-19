namespace Compiler.Syntax.C;

record Program(Function Function);
record Function(string Identifier, Return Statement);
record Return(Constant Expression);
record Constant(int Value);
