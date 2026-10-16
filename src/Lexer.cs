using System.Text.RegularExpressions;

public partial class Lexer
{
  public static List<Token> Run(string input)
  {
    List<Token> tokens = [];
    int position = 0;

    while (position < input.Length)
    {
      if (char.IsWhiteSpace(input[position]))
      {
        position++;
        continue;
      }

      bool matched = false;

      foreach ((Regex pattern, Func<string, Token> create) in Patterns)
      {
        Match match = pattern.Match(input, position);
        if (!match.Success)
          continue;

        tokens.Add(create(match.Value));
        position += match.Length;
        matched = true;
        break;
      }

      if (!matched)
        throw new InvalidOperationException($"Unexpected character: '{input[position]}'");
    }

    return tokens;
  }

  private static readonly (Regex Pattern, Func<string, Token> Create)[] Patterns =
  [
    (IntRegex, _ => new Int()),
    (VoidRegex, _ => new Void()),
    (ReturnRegex, _ => new Return()),
    (IdentifierRegex, text => new Identifier(text)),
    (ConstantRegex, text => new Constant(int.Parse(text))),
    (OpenParenthesisRegex, _ => new OpenParenthesis()),
    (CloseParenthesisRegex, _ => new CloseParenthesis()),
    (OpenBraceRegex, _ => new OpenBrace()),
    (CloseBraceRegex, _ => new CloseBrace()),
    (SemicolonRegex, _ => new Semicolon())
  ];

  // \G anchors each pattern to the current source position.
  [GeneratedRegex(@"\G\bint\b")]
  private static partial Regex IntRegex { get; }

  [GeneratedRegex(@"\G\bvoid\b")]
  private static partial Regex VoidRegex { get; }

  [GeneratedRegex(@"\G\breturn\b")]
  private static partial Regex ReturnRegex { get; }

  [GeneratedRegex(@"\G[a-zA-Z_]\w*\b")]
  private static partial Regex IdentifierRegex { get; }

  [GeneratedRegex(@"\G[0-9]+\b")]
  private static partial Regex ConstantRegex { get; }

  [GeneratedRegex(@"\G\(")]
  private static partial Regex OpenParenthesisRegex { get; }

  [GeneratedRegex(@"\G\)")]
  private static partial Regex CloseParenthesisRegex { get; }

  [GeneratedRegex(@"\G{")]
  private static partial Regex OpenBraceRegex { get; }

  [GeneratedRegex(@"\G}")]
  private static partial Regex CloseBraceRegex { get; }

  [GeneratedRegex(@"\G;")]
  private static partial Regex SemicolonRegex { get; }
}
