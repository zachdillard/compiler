using System.Runtime.InteropServices;
using System.Text;

public class Emitter
{
    public string Emit(Assembly.Program program)
    {
        bool arm64 = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
        bool x64 = (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            && RuntimeInformation.ProcessArchitecture == Architecture.X64;
        if (!arm64 && !x64)
            throw new PlatformNotSupportedException("Assembly emission supports macOS ARM64 and macOS/Linux x64.");

        string symbol = (OperatingSystem.IsMacOS() ? "_" : "") + program.Function.Identifier;
        StringBuilder output = new();
        output.AppendLine($".globl {symbol}");
        output.AppendLine($"{symbol}:");

        foreach (Assembly.Instruction instruction in program.Function.Instructions)
        {
            switch (instruction)
            {
                case Assembly.Mov { Source: Assembly.Imm imm, Destination: Assembly.Register }:
                    if (arm64)
                    {
                        uint bits = unchecked((uint)imm.Value);
                        output.AppendLine($"\tmovz w0, #{bits & 0xffff}");
                        output.AppendLine($"\tmovk w0, #{bits >> 16}, lsl #16");
                    }
                    else
                        output.AppendLine($"\tmovl ${imm.Value}, %eax");
                    break;
                case Assembly.Ret:
                    output.AppendLine("\tret");
                    break;
                default:
                    throw new InvalidOperationException("Unknown assembly instruction.");
            }
        }

        return output.ToString();
    }
}
