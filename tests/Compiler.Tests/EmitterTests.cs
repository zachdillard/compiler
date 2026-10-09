using Xunit;

namespace CompilerIntegrationTests;

public sealed class EmitterTests
{
  [Theory]
  [InlineData("main", 2)]
  [InlineData("answer", 0)]
  [InlineData("maximum", int.MaxValue)]
  public void EmitsFunctionWithMatchingSymbolAndInstructions(string identifier, int value)
  {
    Assembly.Program program = new(new Assembly.Function(identifier,
    [
      new Assembly.Mov(new Assembly.Imm(value), new Assembly.Register()),
      new Assembly.Ret()
    ]));

    string output = Emitter.Run(program);

    Assert.Equal($"\t.globl _{identifier}\n_{identifier}:\n\tmovl ${value}, %eax\n\tret\n"
      .Replace("\n", Environment.NewLine), output);
  }
}
