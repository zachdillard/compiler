namespace C
{
    public record Program(Function Function);
    public record Function(string Identifier, Return Statement);
    public record Return(Constant Expression);
    public record Constant(int Value);
}

namespace Assembly
{
    public record Program(Function Function);
    public record Function(string Identifier, List<Instruction> Instructions);
    public abstract record Instruction;
    public record Mov(Operand Source, Operand Destination) : Instruction;
    public record Ret : Instruction;
    public abstract record Operand;
    public record Imm(int Value) : Operand;
    public record Register : Operand;
}
