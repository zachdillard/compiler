using Xunit;

namespace CompilerIntegrationTests;

public sealed class GeneratorTests
{
  [Theory]
  [InlineData("main", 2)]
  [InlineData("answer", 0)]
  [InlineData("maximum", int.MaxValue)]
  public void GeneratesMoveAndReturn(string identifier, int value)
  {
    C.Program program = new(new C.Function(identifier, new C.Return(new C.Constant(value))));

    Assembly.Program assembly = Generator.Run(program);

    Assert.Equal(identifier, assembly.Function.Identifer);
    Assert.Equal(2, assembly.Function.Instructions.Count);
    Assert.True(assembly.Function.Instructions[0] is Assembly.Mov
    {
      Source: Assembly.Imm { Value: var immediate },
      Destination: Assembly.Register
    } && immediate == value);
    Assert.True(assembly.Function.Instructions[1] is Assembly.Ret);
  }
}
