public readonly union Token
(
  Identifier,
  Int,
  Void,
  Return,
  Constant,
  OpenParenthesis,
  CloseParenthesis,
  OpenBrace,
  CloseBrace,
  Semicolon
);

public record Identifier(string Value);
public record Int();
public record Void();
public record Return();
public record Constant(int Value);
public record OpenParenthesis();
public record CloseParenthesis();
public record OpenBrace();
public record CloseBrace();
public record Semicolon();

public sealed record LocatedToken(Token Kind, string Text, SourceSpan Span);

public sealed record TokenStream(List<LocatedToken> Tokens, SourceSpan End);
