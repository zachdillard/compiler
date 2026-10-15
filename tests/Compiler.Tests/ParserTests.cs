using Xunit;
using C = Syntax.C;

namespace CompilerIntegrationTests;

public sealed class ParserTests
{
  [Theory]
  [InlineData("main", 0)]
  [InlineData("_entry1", 42)]
  [InlineData("maximum", int.MaxValue)]
  public void ParsesFunction(string identifier, int value)
  {
    TokenStream tokens = Lexer.Run($"int {identifier}(void) {{ return {value}; }}");

    C.Program program = Parser.Run(tokens);

    Assert.Equal(new C.Program(new C.Function(identifier, new C.Return(new C.Constant(value)))), program);
  }

  [Theory]
  [InlineData(0, "'int' at the start of the function")]
  [InlineData(1, "an identifier for the function name")]
  [InlineData(2, "'(' after the function name")]
  [InlineData(3, "'void' in the parameter list (only void parameters are supported)")]
  [InlineData(4, "')' after the parameter list")]
  [InlineData(5, "'{' to open the function body")]
  [InlineData(6, "'return' in the function body (only a return statement is supported)")]
  [InlineData(7, "an integer constant after 'return' (only integer constant expressions are supported)")]
  [InlineData(8, "';' after the return statement")]
  [InlineData(9, "'}' to close the function body")]
  public void RejectsTruncatedTokensWithContext(int length, string expected)
  {
    TokenStream all = Lexer.Run("int main(void) { return 42; }");
    SourceSpan end = all.Tokens[length].Span with { Length = 0 };
    TokenStream truncated = new(all.Tokens.GetRange(0, length), end);

    var error = Assert.Throws<DiagnosticException>(() => Parser.Run(truncated));

    Assert.Equal($"expected {expected}; found end of input.", error.Message);
    Assert.Equal(end, error.Span);
  }

  [Theory]
  [InlineData("void main(void) { return 2; }", "'int' at the start of the function", "void", 1)]
  [InlineData("int 2(void) { return 2; }", "an identifier for the function name", "2", 5)]
  [InlineData("int main { return 2; }", "'(' after the function name", "{", 10)]
  [InlineData("int main() { return 2; }", "'void' in the parameter list (only void parameters are supported)", ")", 10)]
  [InlineData("int main(void { return 2; }", "')' after the parameter list", "{", 15)]
  [InlineData("int main(void) return 2; }", "'{' to open the function body", "return", 16)]
  [InlineData("int main(void) { int x; }", "'return' in the function body (only a return statement is supported)", "int", 18)]
  [InlineData("int main(void) { return main; }", "an integer constant after 'return' (only integer constant expressions are supported)", "main", 25)]
  [InlineData("int main(void) { return 2 }", "';' after the return statement", "}", 27)]
  [InlineData("int main(void) { return 2; return 3; }", "'}' to close the function body", "return", 28)]
  public void RejectsIncorrectTokenWithExpectedAndActual(string source, string expected, string actual, int column)
  {
    var error = Assert.Throws<DiagnosticException>(() => Parser.Run(Lexer.Run(source, "syntax.c")));

    Assert.Equal($"expected {expected}; found '{actual}'.", error.Message);
    Assert.Equal(new SourceSpan("syntax.c", 1, column, actual.Length, source), error.Span);
  }

  [Fact]
  public void MissingClosingBrace_PointsToOpeningBraceAndEndOfInput()
  {
    var error = Assert.Throws<DiagnosticException>(() =>
      Parser.Run(Lexer.Run("int main(void) {\n  return 2;\n", "program.c")));

    Assert.Equal("expected '}' to close the function body; found end of input.", error.Message);
    Assert.Equal(new SourceSpan("program.c", 3, 1, 0, ""), error.Span);
    Assert.Equal(new DiagnosticNote("to match this '{'.",
      new SourceSpan("program.c", 1, 16, 1, "int main(void) {")), error.Note);
  }

  [Fact]
  public void MissingClosingParenthesis_PointsToOpeningParenthesis()
  {
    const string source = "int main(void { return 2; }";
    var error = Assert.Throws<DiagnosticException>(() => Parser.Run(Lexer.Run(source)));

    Assert.Equal(new DiagnosticNote("to match this '('.",
      new SourceSpan("<input>", 1, 9, 1, source)), error.Note);
  }

  [Theory]
  [InlineData(";", "expected end of input after the function; found ';'.")]
  [InlineData("int other(void) { return 0; }", "additional functions or declarations are not supported yet; expected end of input.")]
  public void RejectsTrailingTokensWithDiagnostic(string trailing, string expected)
  {
    var error = Assert.Throws<DiagnosticException>(() =>
      Parser.Run(Lexer.Run("int main(void) { return 42; }\n" + trailing)));

    Assert.Equal(expected, error.Message);
    Assert.Equal(2, error.Span.Line);
    Assert.Equal(1, error.Span.Column);
  }
}
