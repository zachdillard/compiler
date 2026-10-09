namespace ClaudeTests;

public class LexerTests
{
    private static readonly string[] ReturnTwo =
        ["int", "Identifier(main)", "(", "void", ")", "{", "return", "Constant(2)", ";", "}"];

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n\r\n ")]
    public void EmptyOrWhitespaceInputProducesNoTokens(string source)
    {
        Assert.Empty(Lexer.Run(source));
    }

    [Theory]
    [InlineData("int main(void) { return 2; }")]
    [InlineData("int main(void){return 2;}")]
    [InlineData("  int\tmain ( void )\n{\r\n  return\t2 ;\n}\n")]
    public void LexesReturnTwoProgram(string source)
    {
        Assert.Equal(ReturnTwo, TokenText.Lex(source));
    }

    [Theory]
    [InlineData("int", "int")]
    [InlineData("void", "void")]
    [InlineData("return", "return")]
    [InlineData("main", "Identifier(main)")]
    [InlineData("42", "Constant(42)")]
    [InlineData("(", "(")]
    [InlineData(")", ")")]
    [InlineData("{", "{")]
    [InlineData("}", "}")]
    [InlineData(";", ";")]
    public void LexesEachTokenKind(string source, string expected)
    {
        Assert.Equal([expected], TokenText.Lex(source));
    }

    [Theory]
    [InlineData("integer")]
    [InlineData("int_x")]
    [InlineData("voidy")]
    [InlineData("returned")]
    [InlineData("_return")]
    [InlineData("Int")]
    [InlineData("RETURN")]
    public void KeywordLookalikesAreIdentifiers(string source)
    {
        Assert.Equal([$"Identifier({source})"], TokenText.Lex(source));
    }

    [Theory]
    [InlineData("_")]
    [InlineData("_foo")]
    [InlineData("a1")]
    [InlineData("x_y_9")]
    [InlineData("camelCase")]
    public void LexesIdentifiers(string source)
    {
        Assert.Equal([$"Identifier({source})"], TokenText.Lex(source));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("007", 7)]
    [InlineData("2147483647", int.MaxValue)]
    public void LexesConstants(string source, int expected)
    {
        Assert.Equal([$"Constant({expected})"], TokenText.Lex(source));
    }

    [Theory]
    [InlineData("123abc")]
    [InlineData("1foo")]
    [InlineData("2_")]
    public void RejectsConstantFollowedByWordCharacter(string source)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Lexer.Run(source));
        Assert.Equal($"Unexpected character: '{source[0]}'", exception.Message);
    }

    [Theory]
    [InlineData("@")]
    [InlineData("\\")]
    [InlineData("`")]
    [InlineData("#")]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData("é")]
    public void RejectsUnsupportedCharacters(string source)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Lexer.Run(source));
        Assert.Equal($"Unexpected character: '{source}'", exception.Message);
    }

    [Fact]
    public void ErrorNamesFirstUnexpectedCharacterAfterValidTokens()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Lexer.Run("int main(void) { return 2 @ }"));
        Assert.Equal("Unexpected character: '@'", exception.Message);
    }

    [Fact]
    public void ConstantTooLargeForIntThrowsOverflow()
    {
        // Pins current behavior: int.Parse overflows instead of producing a lexer error,
        // and Compiler.Run does not catch OverflowException.
        Assert.Throws<OverflowException>(() => Lexer.Run("2147483648"));
    }

    [Fact]
    public void NonAsciiLettersAfterFirstCharacterAreAccepted()
    {
        // Pins current behavior: \w is Unicode-aware, so only the first character is limited to ASCII.
        Assert.Equal(["Identifier(aé)"], TokenText.Lex("aé"));
    }
}
