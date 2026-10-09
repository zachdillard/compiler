namespace ClaudeTests;

public class GeneratorTests
{
    [Theory]
    [InlineData("main", 2)]
    [InlineData("foo", 0)]
    [InlineData("bar", int.MaxValue)]
    public void ReturnBecomesMovIntoRegisterThenRet(string identifier, int value)
    {
        var program = new C.Program(new C.Function(identifier, new C.Return(new C.Constant(value))));

        Assembly.Program assembly = Generator.Run(program);

        Assert.Equal(identifier, assembly.Function.Identifer);
        Assert.Collection(assembly.Function.Instructions,
            instruction =>
            {
                Assert.True(instruction is Assembly.Mov mov
                    && mov.Source is Assembly.Imm imm
                    && imm.Value == value
                    && mov.Destination is Assembly.Register);
            },
            instruction => Assert.True(instruction is Assembly.Ret));
    }
}
