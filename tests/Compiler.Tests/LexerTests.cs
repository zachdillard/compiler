using Xunit;

namespace CompilerIntegrationTests;

public sealed class LexerTests
{
  [Theory]
  [InlineData("int main(void) { return 42; }")]
  [InlineData("int main(void){return 42;}")]
  public void Run_ValidSource_ReturnsExpectedTokens(string source)
  {
    List<Token> tokens = Lexer.Run(source);

    Assert.Collection(tokens,
      token => Assert.True(token is Int),
      token => Assert.True(token is Identifier { Value: "main" }),
      token => Assert.True(token is OpenParenthesis),
      token => Assert.True(token is Void),
      token => Assert.True(token is CloseParenthesis),
      token => Assert.True(token is OpenBrace),
      token => Assert.True(token is Return),
      token => Assert.True(token is Constant { Value: 42 }),
      token => Assert.True(token is Semicolon),
      token => Assert.True(token is CloseBrace));
  }

  [Fact]
  public void Run_MixedWhitespace_ReturnsExpectedTokens()
  {
    List<Token> tokens = Lexer.Run(" \t\r\n\u2003int\tmain (\nvoid\r)\f{\vreturn\u00a0\u0085\t42 ; } \n");

    Assert.Collection(tokens,
      token => Assert.True(token is Int),
      token => Assert.True(token is Identifier { Value: "main" }),
      token => Assert.True(token is OpenParenthesis),
      token => Assert.True(token is Void),
      token => Assert.True(token is CloseParenthesis),
      token => Assert.True(token is OpenBrace),
      token => Assert.True(token is Return),
      token => Assert.True(token is Constant { Value: 42 }),
      token => Assert.True(token is Semicolon),
      token => Assert.True(token is CloseBrace));
  }

  [Fact]
  public void Run_LongInput_ReturnsEveryTokenInOrder()
  {
    const int count = 1024;
    string source = string.Join(" ", Enumerable.Range(0, count).Select(index => $"name{index};"));

    List<Token> tokens = Lexer.Run(source);

    Assert.Equal(count * 2, tokens.Count);
    for (int index = 0; index < count; index++)
    {
      Assert.True(tokens[index * 2] is Identifier identifier && identifier.Value == $"name{index}");
      Assert.True(tokens[index * 2 + 1] is Semicolon);
    }
  }

  [Theory]
  [InlineData("int_value")]
  [InlineData("voided")]
  [InlineData("return2")]
  [InlineData("_entry1")]
  [InlineData("Int")]
  public void Run_KeywordPrefixes_ReturnsIdentifiers(string name)
  {
    Token token = Assert.Single(Lexer.Run(name));

    Assert.True(token is Identifier identifier && identifier.Value == name);
  }

  [Fact]
  public void Run_EmptyInput_ReturnsNoTokens()
  {
    Assert.Empty(Lexer.Run(string.Empty));
  }

  [Fact]
  public void Run_WhitespaceInput_ReturnsNoTokens()
  {
    Assert.Empty(Lexer.Run(" \t\r\n\f\v"));
  }

  [Theory]
  [InlineData("0", 0)]
  [InlineData("42", 42)]
  [InlineData("00042", 42)]
  [InlineData("2147483647", int.MaxValue)]
  public void Run_IntegerConstant_ReturnsExpectedValue(string source, int expected)
  {
    Token token = Assert.Single(Lexer.Run(source));

    Assert.True(token is Constant constant && constant.Value == expected);
  }

  [Theory]
  [InlineData("2147483648")]
  [InlineData("999999999999999999999999")]
  public void Run_OverflowingConstant_ThrowsOverflowException(string source)
  {
    Assert.Throws<OverflowException>(() => Lexer.Run(source));
  }

  [Theory]
  [InlineData("@", "Unexpected character: '@'")]
  [InlineData("int @ main(void){return 42;}", "Unexpected character: '@'")]
  [InlineData("int main(void){return 42;} @ int", "Unexpected character: '@'")]
  [InlineData("int main(void){return -1;}", "Unexpected character: '-'")]
  [InlineData("int main(void){return 1+2;}", "Unexpected character: '+'")]
  public void Run_UnsupportedCharacter_ThrowsDiagnostic(string source, string expected)
  {
    InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Lexer.Run(source));

    Assert.Equal(expected, exception.Message);
  }

  [Theory]
  [InlineData("123abc")]
  [InlineData("123_abc")]
  [InlineData("int main(void){return 123abc;}")]
  [InlineData("int main(void){return 123_abc;}")]
  public void Run_NumericIdentifier_ThrowsDiagnostic(string source)
  {
    InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Lexer.Run(source));

    Assert.Equal("Unexpected character: '1'", exception.Message);
  }
}
