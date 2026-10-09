namespace ClaudeTests;

public class ParserTests
{
    [Fact]
    public void ParsesReturnTwoProgram()
    {
        C.Program program = Parser.Run(Lexer.Run("int main(void) { return 2; }"));

        Assert.Equal(new C.Program(new C.Function("main", new C.Return(new C.Constant(2)))), program);
    }

    [Theory]
    [InlineData("int foo(void) { return 0; }", "foo", 0)]
    [InlineData("int _bar(void) { return 2147483647; }", "_bar", int.MaxValue)]
    public void PreservesIdentifierAndConstant(string source, string identifier, int value)
    {
        C.Program program = Parser.Run(Lexer.Run(source));

        Assert.Equal(identifier, program.Function.Identifier);
        Assert.Equal(value, program.Function.Statement.Expression.Value);
    }

    [Theory]
    [InlineData("", "Unexpected end of input.")]
    [InlineData("main(void) { return 2; }", "Expected Int.")]
    [InlineData("int (void) { return 2; }", "Expected an identifier.")]
    [InlineData("int return(void) { return 2; }", "Expected an identifier.")]
    [InlineData("int main void) { return 2; }", "Expected OpenParenthesis.")]
    [InlineData("int main() { return 2; }", "Expected Void.")]
    [InlineData("int main(void { return 2; }", "Expected CloseParenthesis.")]
    [InlineData("int main(void) return 2; }", "Expected OpenBrace.")]
    [InlineData("int main(void) { }", "Expected Return.")]
    [InlineData("int main(void) { 2; }", "Expected Return.")]
    [InlineData("int main(void) { return; }", "Expected an integer constant.")]
    [InlineData("int main(void) { return x; }", "Expected an integer constant.")]
    [InlineData("int main(void) { return 2 }", "Expected Semicolon.")]
    [InlineData("int main(void) { return 2;; }", "Expected CloseBrace.")]
    [InlineData("int main(void) { return 2; ", "Unexpected end of input.")]
    [InlineData("int main(void) { return 2; } foo", "Unexpected tokens after the function.")]
    public void RejectsInvalidPrograms(string source, string message)
    {
        List<Token> tokens = Lexer.Run(source);

        var exception = Assert.Throws<InvalidOperationException>(() => Parser.Run(tokens));
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void EveryTruncatedProgramReportsEndOfInput()
    {
        List<Token> full = Lexer.Run("int main(void) { return 2; }");

        for (int length = 0; length < full.Count; length++)
        {
            List<Token> prefix = full.Take(length).ToList();
            var exception = Assert.Throws<InvalidOperationException>(() => Parser.Run(prefix));
            Assert.Equal("Unexpected end of input.", exception.Message);
        }
    }

    [Fact]
    public void ConsumesTheCallersTokenList()
    {
        // Pins current behavior: the parser removes tokens from the list it is given.
        List<Token> tokens = Lexer.Run("int main(void) { return 2; }");

        Parser.Run(tokens);

        Assert.Empty(tokens);
    }
}
