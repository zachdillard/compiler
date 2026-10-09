using Xunit;

namespace CompilerSpecificationTests;

public sealed class GeneratorSpecificationTests
{
  [Theory]
  [InlineData("main", 42)]
  [InlineData("_answer42", 0)]
  [InlineData("maximum", int.MaxValue)]
  public void LowersAReturnConstantToAMoveIntoTheReturnRegisterThenReturn(string name, int value)
  {
    C.Program input = new(new C.Function(name, new C.Return(new C.Constant(value))));

    Assembly.Program output = Generator.Run(input);

    Assert.Equal(name, output.Function.Identifer);
    Assert.Collection(output.Function.Instructions,
      instruction => Assert.True(instruction is Assembly.Mov
      {
        Source: Assembly.Imm { Value: var immediate },
        Destination: Assembly.Register
      } && immediate == value),
      instruction => Assert.True(instruction is Assembly.Ret));
  }
}
