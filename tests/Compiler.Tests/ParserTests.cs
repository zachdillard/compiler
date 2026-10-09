using Xunit;

namespace CompilerIntegrationTests;

public sealed class ParserTests
{
  [Fact]
  public void ParsesFunctionAndConsumesTokens()
  {
    List<Token> tokens = Lexer.Run("int main(void) { return 2; }");

    C.Program program = Parser.Run(tokens);

    Assert.Equal(new C.Program(new C.Function("main", new C.Return(new C.Constant(2)))), program);
    Assert.Empty(tokens);
  }

  [Theory]
  [InlineData("")]
  [InlineData("int main(void) { return 2;")]
  [InlineData("int main(void) { return 2 }")]
  [InlineData("int main() { return 2; }")]
  [InlineData("int 2(void) { return 2; }")]
  [InlineData("int main(void) { return main; }")]
  [InlineData("int main(void) { return 2; } int extra(void) { return 3; }")]
  public void RejectsInvalidSyntax(string source)
  {
    List<Token> tokens = Lexer.Run(source);

    Assert.Throws<InvalidOperationException>(() => Parser.Run(tokens));
  }
}
