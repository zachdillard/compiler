using C = Syntax.C;

public class Parser
{
  public static C.Program Run(List<Token> tokens)
  {
    TokenCursor cursor = new(tokens);
    C.Program program = new(ParseFunction(cursor));
    if (!cursor.AtEnd)
      throw new InvalidOperationException("Unexpected tokens after the function.");

    return program;
  }

  private static C.Function ParseFunction(TokenCursor tokens)
  {
    Expect<Int>(tokens);
    string identifier = ParseIdentifier(tokens);
    Expect<OpenParenthesis>(tokens);
    Expect<Void>(tokens);
    Expect<CloseParenthesis>(tokens);
    Expect<OpenBrace>(tokens);
    C.Return statement = ParseStatement(tokens);
    Expect<CloseBrace>(tokens);

    return new C.Function(identifier, statement);
  }

  private static C.Return ParseStatement(TokenCursor tokens)
  {
    Expect<Return>(tokens);
    C.Constant expression = ParseExpression(tokens);
    Expect<Semicolon>(tokens);

    return new C.Return(expression);
  }

  private static C.Constant ParseExpression(TokenCursor tokens)
  {
    return new C.Constant(ParseInt(tokens));
  }

  private static string ParseIdentifier(TokenCursor tokens)
  {
    if (tokens.Take() is Identifier identifier)
      return identifier.Value;

    throw new InvalidOperationException("Expected an identifier.");
  }

  private static int ParseInt(TokenCursor tokens)
  {
    if (tokens.Take() is Constant constant)
      return constant.Value;

    throw new InvalidOperationException("Expected an integer constant.");
  }

  private static void Expect<T>(TokenCursor tokens)
  {
    if (tokens.Take() is not T)
      throw new InvalidOperationException($"Expected {typeof(T).Name}.");
  }

  private sealed class TokenCursor(IReadOnlyList<Token> tokens)
  {
    private int position;

    public bool AtEnd => position == tokens.Count;

    public Token Take()
    {
      if (AtEnd)
        throw new InvalidOperationException("Unexpected end of input.");

      return tokens[position++];
    }
  }
}
