using Xunit;

namespace CompilerIntegrationTests;

public sealed class LexerTests
{
  [Theory]
  [InlineData("int main(void) { return 42; }")]
  [InlineData("int main(void){return 42;}")]
  public void Run_ValidSource_ReturnsExpectedTokens(string source)
  {
    List<LocatedToken> tokens = Lexer.Run(source).Tokens;

    Assert.Collection(tokens,
      token => Assert.True(token.Kind is Int),
      token => Assert.True(token.Kind is Identifier { Value: "main" }),
      token => Assert.True(token.Kind is OpenParenthesis),
      token => Assert.True(token.Kind is Void),
      token => Assert.True(token.Kind is CloseParenthesis),
      token => Assert.True(token.Kind is OpenBrace),
      token => Assert.True(token.Kind is Return),
      token => Assert.True(token.Kind is Constant { Value: 42 }),
      token => Assert.True(token.Kind is Semicolon),
      token => Assert.True(token.Kind is CloseBrace));
  }

  [Fact]
  public void Run_MixedWhitespace_ReturnsExpectedTokens()
  {
    List<LocatedToken> tokens = Lexer.Run(" \t\r\nint\tmain (\nvoid\r)\f{\vreturn\t42 ; } \n").Tokens;

    Assert.Collection(tokens,
      token => Assert.True(token.Kind is Int),
      token => Assert.True(token.Kind is Identifier { Value: "main" }),
      token => Assert.True(token.Kind is OpenParenthesis),
      token => Assert.True(token.Kind is Void),
      token => Assert.True(token.Kind is CloseParenthesis),
      token => Assert.True(token.Kind is OpenBrace),
      token => Assert.True(token.Kind is Return),
      token => Assert.True(token.Kind is Constant { Value: 42 }),
      token => Assert.True(token.Kind is Semicolon),
      token => Assert.True(token.Kind is CloseBrace));
  }

  [Theory]
  [InlineData("int_value")]
  [InlineData("voided")]
  [InlineData("return2")]
  [InlineData("_entry1")]
  [InlineData("Int")]
  public void Run_KeywordPrefixes_ReturnsIdentifiers(string name)
  {
    LocatedToken token = Assert.Single(Lexer.Run(name).Tokens);

    Assert.True(token.Kind is Identifier identifier && identifier.Value == name);
  }

  [Fact]
  public void Run_EmptyInput_ReturnsNoTokens()
  {
    Assert.Empty(Lexer.Run(string.Empty).Tokens);
  }

  [Fact]
  public void Run_WhitespaceInput_ReturnsNoTokens()
  {
    Assert.Empty(Lexer.Run(" \t\r\n\f\v").Tokens);
  }

  [Theory]
  [InlineData("0", 0)]
  [InlineData("42", 42)]
  [InlineData("00042", 42)]
  [InlineData("2147483647", int.MaxValue)]
  public void Run_IntegerConstant_ReturnsExpectedValue(string source, int expected)
  {
    LocatedToken token = Assert.Single(Lexer.Run(source).Tokens);

    Assert.True(token.Kind is Constant constant && constant.Value == expected);
  }

  [Theory]
  [InlineData("2147483648")]
  [InlineData("999999999999999999999999")]
  public void Run_OverflowingConstant_ReportsLiteralAndRange(string source)
  {
    var error = Assert.Throws<DiagnosticException>(() => Lexer.Run(source, "number.c"));

    Assert.Equal($"integer constant '{source}' is outside the supported int range (0 to 2147483647).", error.Message);
    Assert.Equal(new SourceSpan("number.c", 1, 1, source.Length, source), error.Span);
  }

  [Theory]
  [InlineData("@", "unexpected character '@'.")]
  [InlineData("int main(void){return -1;}", "unary '-' is not supported yet.")]
  [InlineData("int main(void){return 1+2;}", "operator '+' is not supported yet.")]
  public void Run_UnsupportedCharacter_ThrowsDiagnostic(string source, string expected)
  {
    var error = Assert.Throws<DiagnosticException>(() => Lexer.Run(source));

    Assert.Equal(expected, error.Message);
    Assert.Equal(1, error.Span.Length);
  }

  [Theory]
  [InlineData("123abc")]
  [InlineData("123_abc")]
  [InlineData("1.5")]
  public void Run_MalformedNumber_ReportsWholeLiteral(string source)
  {
    var error = Assert.Throws<DiagnosticException>(() => Lexer.Run(source));

    Assert.Equal($"invalid integer constant '{source}'; only decimal digits are supported.", error.Message);
    Assert.Equal(source.Length, error.Span.Length);
  }

  [Fact]
  public void Run_Whitespace_PreservesTokenSpansAndSpelling()
  {
    var stream = Lexer.Run("\r\n\treturn 00042;\r}", "program.c");

    Assert.Equal(new SourceSpan("program.c", 2, 2, 6, "\treturn 00042;"), stream.Tokens[0].Span);
    Assert.Equal("00042", stream.Tokens[1].Text);
    Assert.Equal(new SourceSpan("program.c", 2, 9, 5, "\treturn 00042;"), stream.Tokens[1].Span);
    Assert.Equal(new SourceSpan("program.c", 3, 1, 1, "}"), stream.Tokens[3].Span);
    Assert.Equal(new SourceSpan("program.c", 3, 2, 0, "}"), stream.End);
  }

  [Fact]
  public void Run_LineMarkers_MapIncludesAndReturnToSource()
  {
    var stream = Lexer.Run("# 20 \"header.h\" 1\nreturn\n# 4 \"program.c\" 2\n42;\n", preprocessed: true);

    Assert.Equal(new SourceSpan("header.h", 20, 1, 6, "return", true), stream.Tokens[0].Span);
    Assert.Equal(new SourceSpan("program.c", 4, 1, 2, "42;", true), stream.Tokens[1].Span);
    Assert.Equal(new SourceSpan("program.c", 5, 1, 0, "", true), stream.End);
  }

  [Fact]
  public void Run_LineMarker_DecodesEscapedFilename()
  {
    var stream = Lexer.Run("""
      # 7 "dir\\file\"name.c"
      int
      """, preprocessed: true);

    Assert.Equal("dir\\file\"name.c", stream.Tokens[0].Span.File);
    Assert.Equal(7, stream.Tokens[0].Span.Line);
  }

  [Fact]
  public void Run_LineDirective_MapsErrorToLogicalSource()
  {
    var error = Assert.Throws<DiagnosticException>(() =>
      Lexer.Run("#line 40 \"logical.c\"\n  @", preprocessed: true));

    Assert.Equal("unexpected character '@'.", error.Message);
    Assert.Equal(new SourceSpan("logical.c", 40, 3, 1, "  @", true), error.Span);
  }

  [Fact]
  public void Run_EmptyInput_PreservesEndOfInputLocation()
  {
    Assert.Equal(new SourceSpan("empty.c", 1, 1, 0, ""), Lexer.Run("", "empty.c").End);
  }

  [Fact]
  public void Run_RawSource_DoesNotSilentlyDiscardDirectives()
  {
    var error = Assert.Throws<DiagnosticException>(() => Lexer.Run("# 1 \"file.c\""));

    Assert.Equal("unexpected character '#'.", error.Message);
  }
}
