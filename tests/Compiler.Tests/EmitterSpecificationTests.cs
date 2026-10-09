using Xunit;

namespace CompilerSpecificationTests;

public sealed class EmitterSpecificationTests
{
  [Theory]
  [InlineData("main", 42)]
  [InlineData("_answer42", 0)]
  [InlineData("maximum", int.MaxValue)]
  public void EmitsMacOsSymbolsAndX86ReturnInstructions(string name, int value)
  {
    Assembly.Program input = new(new Assembly.Function(name,
    [
      new Assembly.Mov(new Assembly.Imm(value), new Assembly.Register()),
      new Assembly.Ret()
    ]));

    string output = Emitter.Run(input);

    string expected = $"\t.globl _{name}\n_{name}:\n\tmovl ${value}, %eax\n\tret\n";
    Assert.Equal(expected.Replace("\n", Environment.NewLine), output);
  }

  [Fact]
  public void PreservesAssemblyInstructionOrder()
  {
    Assembly.Program input = new(new Assembly.Function("ordered",
    [
      new Assembly.Mov(new Assembly.Imm(1), new Assembly.Register()),
      new Assembly.Mov(new Assembly.Imm(2), new Assembly.Register()),
      new Assembly.Ret()
    ]));

    string output = Emitter.Run(input);

    const string expected = "\t.globl _ordered\n_ordered:\n\tmovl $1, %eax\n\tmovl $2, %eax\n\tret\n";
    Assert.Equal(expected.Replace("\n", Environment.NewLine), output);
  }
}
