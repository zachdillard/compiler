namespace ClaudeTests;

public class EmitterTests
{
    private static Assembly.Program ReturnProgram(string identifier, int value) =>
        new(new Assembly.Function(identifier,
        [
            new Assembly.Mov(new Assembly.Imm(value), new Assembly.Register()),
            new Assembly.Ret()
        ]));

    [Fact]
    public void EmitsReturnTwoProgramExactly()
    {
        string output = Emitter.Run(ReturnProgram("main", 2));

        Assert.Equal("\t.globl _main\n_main:\n\tmovl $2, %eax\n\tret\n", output);
    }

    [Fact]
    public void UsesFunctionNameInGloblAndLabel()
    {
        string output = Emitter.Run(ReturnProgram("foo", 0));

        Assert.StartsWith("\t.globl _foo\n_foo:\n", output);
    }

    [Theory]
    [InlineData(0, "movl $0, %eax")]
    [InlineData(2147483647, "movl $2147483647, %eax")]
    public void EmitsImmediateAndRegisterOperands(int value, string instruction)
    {
        string output = Emitter.Run(ReturnProgram("main", value));

        Assert.Contains($"\t{instruction}\n", output);
    }

    [Fact]
    public void PreservesInstructionOrder()
    {
        var program = new Assembly.Program(new Assembly.Function("main",
        [
            new Assembly.Mov(new Assembly.Imm(1), new Assembly.Register()),
            new Assembly.Mov(new Assembly.Imm(2), new Assembly.Register()),
            new Assembly.Ret(),
            new Assembly.Ret()
        ]));

        string output = Emitter.Run(program);

        Assert.Equal("\t.globl _main\n_main:\n\tmovl $1, %eax\n\tmovl $2, %eax\n\tret\n\tret\n", output);
    }

    [Fact]
    public void EmptyFunctionEmitsOnlyHeader()
    {
        string output = Emitter.Run(new Assembly.Program(new Assembly.Function("main", [])));

        Assert.Equal("\t.globl _main\n_main:\n", output);
    }
}
