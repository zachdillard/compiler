using C = Syntax.C;

public class Parser
{
  public static C.Program Run(TokenStream tokens)
  {
    Reader reader = new(tokens);
    C.Program program = new(reader.ParseFunction());
    if (reader.Current is { } trailing)
    {
      string message = trailing.Kind is Int
        ? "additional functions or declarations are not supported yet; expected end of input."
        : $"expected end of input after the function; found '{trailing.Text}'.";
      throw new DiagnosticException(message, trailing.Span);
    }
    return program;
  }

  private sealed class Reader(TokenStream input)
  {
    private int position;
    public LocatedToken? Current => position < input.Tokens.Count ? input.Tokens[position] : null;

    public C.Function ParseFunction()
    {
      Expect<Int>("'int'", "at the start of the function");
      string identifier = ParseIdentifier();
      LocatedToken openingParenthesis = Expect<OpenParenthesis>("'('", "after the function name");
      Expect<Void>("'void'", "in the parameter list (only void parameters are supported)");
      Expect<CloseParenthesis>("')'", "after the parameter list", openingParenthesis);
      LocatedToken openingBrace = Expect<OpenBrace>("'{'", "to open the function body");
      C.Return statement = ParseStatement();
      Expect<CloseBrace>("'}'", "to close the function body", openingBrace);

      return new C.Function(identifier, statement);
    }

    private string ParseIdentifier()
    {
      if (Current is { } token && token.Kind is Identifier identifier)
      {
        position++;
        return identifier.Value;
      }
      throw Error("an identifier", "for the function name");
    }

    private C.Return ParseStatement()
    {
      Expect<Return>("'return'", "in the function body (only a return statement is supported)");
      C.Constant expression = ParseExpression();
      Expect<Semicolon>("';'", "after the return statement");
      return new C.Return(expression);
    }

    private C.Constant ParseExpression()
    {
      if (Current is { } token && token.Kind is Constant constant)
      {
        position++;
        return new C.Constant(constant.Value);
      }
      throw Error("an integer constant", "after 'return' (only integer constant expressions are supported)");
    }

    private LocatedToken Expect<T>(string expected, string context, LocatedToken? opening = null)
    {
      if (Current is { } token && token.Kind is T)
      {
        position++;
        return token;
      }
      throw Error(expected, context, opening);
    }

    private DiagnosticException Error(string expected, string context, LocatedToken? opening = null)
    {
      string actual = Current is { } found ? $"'{found.Text}'" : "end of input";
      DiagnosticNote? note = opening is null ? null
        : new DiagnosticNote($"to match this '{opening.Text}'.", opening.Span);
      return new DiagnosticException($"expected {expected} {context}; found {actual}.",
        Current?.Span ?? input.End, note);
    }
  }
}
