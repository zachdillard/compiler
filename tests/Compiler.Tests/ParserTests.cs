using Xunit;

namespace Compiler.Tests;

public class ParserTests
{
  [Theory]
  [InlineData("main", 0)]
  [InlineData("_entry1", 42)]
  [InlineData("maximum", int.MaxValue)]
  public void Run_ValidTokens_ReturnsExpectedProgram(string identifier, int value)
  {
    var tokens = FunctionTokens(identifier, value);

    var program = Parser.Run(tokens);

    Assert.Equal(new C.Program(new C.Function(identifier, new C.Return(new C.Constant(value)))), program);
    Assert.Empty(tokens);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(6)]
  [InlineData(7)]
  [InlineData(8)]
  [InlineData(9)]
  public void Run_TruncatedTokens_ThrowsUnexpectedEnd(int length)
  {
    var tokens = FunctionTokens("main", 42).GetRange(0, length);

    var exception = Assert.Throws<InvalidOperationException>(() => Parser.Run(tokens));

    Assert.Equal("Unexpected end of input.", exception.Message);
  }

  [Theory]
  [InlineData(0, "Expected Int.")]
  [InlineData(1, "Expected an identifier.")]
  [InlineData(2, "Expected OpenParenthesis.")]
  [InlineData(3, "Expected Void.")]
  [InlineData(4, "Expected CloseParenthesis.")]
  [InlineData(5, "Expected OpenBrace.")]
  [InlineData(6, "Expected Return.")]
  [InlineData(7, "Expected an integer constant.")]
  [InlineData(8, "Expected Semicolon.")]
  [InlineData(9, "Expected CloseBrace.")]
  public void Run_IncorrectToken_ThrowsExpectedTokenDiagnostic(int position, string expected)
  {
    var tokens = FunctionTokens("main", 42);
    // Semicolon is incorrect everywhere except its own grammar position.
    tokens[position] = position == 8 ? new Int() : new Semicolon();

    var exception = Assert.Throws<InvalidOperationException>(() => Parser.Run(tokens));

    Assert.Equal(expected, exception.Message);
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void Run_TrailingTokens_ThrowsDiagnostic(bool secondFunction)
  {
    var tokens = FunctionTokens("main", 42);
    if (secondFunction)
      tokens.AddRange(FunctionTokens("other", 0));
    else
      tokens.Add(new Semicolon());

    var exception = Assert.Throws<InvalidOperationException>(() => Parser.Run(tokens));

    Assert.Equal("Unexpected tokens after the function.", exception.Message);
  }

  private static List<Token> FunctionTokens(string identifier, int value) =>
  [
    new Int(), new Identifier(identifier), new OpenParenthesis(), new Void(),
    new CloseParenthesis(), new OpenBrace(), new Return(), new Constant(value),
    new Semicolon(), new CloseBrace()
  ];
}
