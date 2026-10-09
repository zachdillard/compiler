using Xunit;

namespace CompilerSpecificationTests;

public sealed class ParserSpecificationTests
{
  // Token inputs are constructed directly so lexer behavior cannot mask parser failures.
  private static List<Token> FunctionTokens(string name = "main", int value = 42) =>
  [
    new Int(), new Identifier(name), new OpenParenthesis(), new Void(),
    new CloseParenthesis(), new OpenBrace(), new Return(), new Constant(value),
    new Semicolon(), new CloseBrace()
  ];

  [Theory]
  [InlineData("main", 42)]
  [InlineData("_answer42", 0)]
  [InlineData("maximum", int.MaxValue)]
  public void ParsesTheDocumentedFunctionGrammar(string name, int value)
  {
    C.Program expected = new(new C.Function(name, new C.Return(new C.Constant(value))));

    Assert.Equal(expected, Parser.Run(FunctionTokens(name, value)));
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
  public void RejectsEveryIncompletePrefixOfAFunction(int tokenCount)
  {
    List<Token> tokens = FunctionTokens().Take(tokenCount).ToList();

    Assert.ThrowsAny<Exception>(() => Parser.Run(tokens));
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
  public void RejectsARequiredTokenInTheWrongGrammarPosition(int position)
  {
    List<Token> tokens = FunctionTokens();
    // Neither token is valid in the position where it is substituted.
    tokens[position] = position == 1 ? new Constant(42) : new Identifier("unexpected");

    Assert.ThrowsAny<Exception>(() => Parser.Run(tokens));
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
  public void RejectsAMissingRequiredToken(int position)
  {
    List<Token> tokens = FunctionTokens();
    tokens.RemoveAt(position);

    Assert.ThrowsAny<Exception>(() => Parser.Run(tokens));
  }

  [Fact]
  public void RejectsTokensAfterTheFunction()
  {
    List<Token> tokens = FunctionTokens();
    tokens.Add(new Semicolon());

    Assert.ThrowsAny<Exception>(() => Parser.Run(tokens));
  }

  [Fact]
  public void RejectsASecondFunction()
  {
    List<Token> tokens = FunctionTokens();
    tokens.AddRange(FunctionTokens("other", 0));

    Assert.ThrowsAny<Exception>(() => Parser.Run(tokens));
  }

  [Fact]
  public void RejectsASecondReturnStatement()
  {
    List<Token> tokens = FunctionTokens();
    tokens.InsertRange(9, [new Return(), new Constant(0), new Semicolon()]);

    Assert.ThrowsAny<Exception>(() => Parser.Run(tokens));
  }
}
