namespace Compiler.Syntax.Assembly;

record Program(Function Function);
record Function(string Identifier, List<Instruction> Instructions);
readonly union Instruction(Mov, Ret);
record Mov(Operand Source, Operand Destination);
record Ret();
readonly union Operand(Imm, Register);
record Imm(int Value);
record Register();
