using System.Text;
using Assembly = Compiler.Syntax.Assembly;

namespace Compiler;

class Emitter
{
    public static string Run(Assembly.Program asm)
    {
        StringBuilder output = new();

        output.AppendLine($"\t.globl _{asm.Function.Identifier}");
        output.Append(EmitFunction(asm.Function));

        return output.ToString();
    }

    private static string EmitFunction(Assembly.Function function)
    {
        StringBuilder output = new();

        output.AppendLine($"_{function.Identifier}:");
        foreach (Assembly.Instruction instruction in function.Instructions)
            output.AppendLine($"\t{EmitInstruction(instruction)}");

        return output.ToString();
    }

    private static string EmitInstruction(Assembly.Instruction instruction) => instruction switch
    {
        Assembly.Mov mov => $"movl {EmitOperand(mov.Source)}, {EmitOperand(mov.Destination)}",
        Assembly.Ret _ => "ret",
        _ => throw new CompilerException("Unknown assembly instruction.")
    };

    private static string EmitOperand(Assembly.Operand operand) => operand switch
    {
        Assembly.Imm imm => $"${imm.Value}",
        Assembly.Register => "%eax",
        _ => throw new CompilerException("Unknown assembly operand.")
    };
}
