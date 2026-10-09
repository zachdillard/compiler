using Xunit;

namespace CompilerSpecificationTests;

public sealed class LexerSpecificationTests
{
  [Fact]
  public void TokenizesAChapterOneFunctionInSourceOrder()
  {
    List<Token> tokens = Lexer.Run("int main(void){return 42;}");

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

  [Theory]
  [InlineData("")]
  [InlineData(" \t\r\n\v\f")]
  public void EmptyOrWhitespaceInputHasNoTokens(string source)
  {
    Assert.Empty(Lexer.Run(source));
  }

  [Fact]
  public void WhitespaceSeparatesTokensWithoutChangingTheirMeaning()
  {
    List<Token> tokens = Lexer.Run(" \tint\r\nname\v(\fvoid )\n{ return\t0 ; } \r\n");

    Assert.Collection(tokens,
      token => Assert.True(token is Int),
      token => Assert.True(token is Identifier { Value: "name" }),
      token => Assert.True(token is OpenParenthesis),
      token => Assert.True(token is Void),
      token => Assert.True(token is CloseParenthesis),
      token => Assert.True(token is OpenBrace),
      token => Assert.True(token is Return),
      token => Assert.True(token is Constant { Value: 0 }),
      token => Assert.True(token is Semicolon),
      token => Assert.True(token is CloseBrace));
  }

  [Theory]
  [InlineData("a")]
  [InlineData("_")]
  [InlineData("_answer42")]
  [InlineData("Main")]
  [InlineData("int_value")]
  [InlineData("integer")]
  [InlineData("void0")]
  [InlineData("returnValue")]
  [InlineData("INT")]
  public void IdentifiersAreNotSplitIntoKeywords(string source)
  {
    Token token = Assert.Single(Lexer.Run(source));

    Assert.True(token is Identifier { Value: var name } && name == source);
  }

  [Theory]
  [InlineData("0", 0)]
  [InlineData("42", 42)]
  [InlineData("2147483647", int.MaxValue)]
  public void IntegerConstantsPreserveTheirValue(string source, int expected)
  {
    Token token = Assert.Single(Lexer.Run(source));

    Assert.True(token is Constant { Value: var value } && value == expected);
  }

  [Theory]
  [InlineData("@")]
  [InlineData("int main(void) { return 1 + 2; }")]
  [InlineData("-1")]
  [InlineData("1.5")]
  [InlineData("123abc")]
  [InlineData("123_name")]
  [InlineData("2147483648")]
  [InlineData("999999999999999999999999999999")]
  public void RejectsUnsupportedOrMalformedTokens(string source)
  {
    Assert.ThrowsAny<Exception>(() => Lexer.Run(source));
  }
}
