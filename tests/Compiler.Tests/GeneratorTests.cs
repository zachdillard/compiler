using Xunit;

namespace Compiler.Tests;

public class GeneratorTests
{
  [Theory]
  [InlineData("main", 0)]
  [InlineData("_entry1", 42)]
  [InlineData("minimum", int.MinValue)]
  [InlineData("maximum", int.MaxValue)]
  public void Run_ReturnConstant_ProducesMoveThenReturn(string identifier, int value)
  {
    var program = new C.Program(new C.Function(identifier, new C.Return(new C.Constant(value))));

    var assembly = Generator.Run(program);

    Assert.Equal(identifier, assembly.Function.Identifer);
    Assert.Collection(assembly.Function.Instructions,
      instruction => Assert.True(instruction is Assembly.Mov
      {
        Source: Assembly.Imm immediate,
        Destination: Assembly.Register
      } && immediate.Value == value),
      instruction => Assert.True(instruction is Assembly.Ret));
  }
}
