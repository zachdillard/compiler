namespace ClaudeTests;

public class PipelineTests
{
    private static string CompileToAssembly(string source) =>
        Emitter.Run(Generator.Run(Parser.Run(Lexer.Run(source))));

    [Theory]
    [InlineData("int main(void) { return 2; }", "main", 2)]
    [InlineData("int main(void)\n{\n    return 0;\n}\n", "main", 0)]
    [InlineData("int foo(void){return 100;}", "foo", 100)]
    public void CompilesSourceToAssembly(string source, string identifier, int value)
    {
        Assert.Equal($"\t.globl _{identifier}\n_{identifier}:\n\tmovl ${value}, %eax\n\tret\n", CompileToAssembly(source));
    }

    [Theory]
    [InlineData("int main(void) { return 0 }")]
    [InlineData("int main(void) { retur 0; }")]
    [InlineData("int main(void) { return 0; } foo")]
    [InlineData("int main(void) { return; }")]
    [InlineData("int main(void) { return 0;")]
    [InlineData("int 3 (void) { return 0; }")]
    public void ParserRejectsInvalidPrograms(string source)
    {
        List<Token> tokens = Lexer.Run(source);

        Assert.Throws<InvalidOperationException>(() => Parser.Run(tokens));
    }

    [Theory]
    [InlineData("int main(void) { return 1foo; }")]
    [InlineData("int main(void) { return @b; }")]
    [InlineData("int main(void) { return 0 $ }")]
    public void LexerRejectsInvalidPrograms(string source)
    {
        Assert.Throws<InvalidOperationException>(() => Lexer.Run(source));
    }
}
