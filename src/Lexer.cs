using System.Globalization;
using System.Text.RegularExpressions;

public partial class Lexer
{
  public static TokenStream Run(string input, string fileName = "<input>", bool preprocessed = false)
  {
    List<LocatedToken> tokens = [];
    int lineNumber = 1;
    SourceSpan end = new(fileName, 1, 1, 0, "", preprocessed);

    foreach (string line in Regex.Split(input, "\r\n|\r|\n"))
    {
      // GCC's marker describes the file and line of the next source line.
      Match marker = LineMarkerRegex.Match(line);
      if (preprocessed && marker.Success && int.TryParse(marker.Groups[1].Value, out int mappedLine))
      {
        lineNumber = mappedLine;
        fileName = marker.Groups[2].Value.Replace("\\\"", "\"").Replace("\\\\", "\\");
        end = new SourceSpan(fileName, lineNumber, 1, 0, "", preprocessed);
        continue;
      }

      int offset = 0;
      while (offset < line.Length)
      {
        if (char.IsWhiteSpace(line[offset]))
        {
          offset++;
          continue;
        }

        SourceSpan Span(int length) => new(fileName, lineNumber, offset + 1, length, line, preprocessed);
        string remaining = line[offset..];
        if (char.IsAsciiDigit(line[offset]))
        {
          string literal = NumberRegex.Match(remaining).Value;
          if (literal.Any(character => !char.IsAsciiDigit(character)))
            throw new DiagnosticException($"invalid integer constant '{literal}'; only decimal digits are supported.", Span(literal.Length));
          if (!int.TryParse(literal, NumberStyles.None, CultureInfo.InvariantCulture, out int value))
            throw new DiagnosticException($"integer constant '{literal}' is outside the supported int range (0 to {int.MaxValue}).", Span(literal.Length));
          tokens.Add(new LocatedToken(new Constant(value), literal, Span(literal.Length)));
          offset += literal.Length;
          continue;
        }

        bool matched = false;
        foreach ((Regex pattern, Func<string, Token> create) in Patterns)
        {
          Match match = pattern.Match(remaining);
          if (!match.Success || match.Index != 0)
            continue;

          tokens.Add(new LocatedToken(create(match.Value), match.Value, Span(match.Length)));
          offset += match.Length;
          matched = true;
          break;
        }

        if (!matched)
        {
          char character = line[offset];
          string message = character switch
          {
            '-' when tokens.Count > 0 && tokens[^1].Kind is Return => "unary '-' is not supported yet.",
            '+' or '-' or '*' or '/' or '%' or '~' or '!' or '&' or '|' or '^' or '<' or '>' or '=' =>
              $"operator '{character}' is not supported yet.",
            _ => $"unexpected character '{character}'."
          };
          throw new DiagnosticException(message, Span(1));
        }
      }

      end = new SourceSpan(fileName, lineNumber, line.Length + 1, 0, line, preprocessed);
      lineNumber++;
    }

    return new TokenStream(tokens, end);
  }

  private static readonly (Regex Pattern, Func<string, Token> Create)[] Patterns =
  [
    (IntRegex, _ => new Int()),
    (VoidRegex, _ => new Void()),
    (ReturnRegex, _ => new Return()),
    (IdentifierRegex, text => new Identifier(text)),
    (OpenParenthesisRegex, _ => new OpenParenthesis()),
    (CloseParenthesisRegex, _ => new CloseParenthesis()),
    (OpenBraceRegex, _ => new OpenBrace()),
    (CloseBraceRegex, _ => new CloseBrace()),
    (SemicolonRegex, _ => new Semicolon())
  ];

  [GeneratedRegex("""^\s*#\s*(?:line\s+)?([0-9]+)\s+"((?:\\.|[^"\\])*)"(?:\s+[0-9]+)*\s*$""")]
  private static partial Regex LineMarkerRegex { get; }

  // Keep a malformed numeric token together instead of reporting only its first digit.
  [GeneratedRegex(@"^[0-9][\w.]*")]
  private static partial Regex NumberRegex { get; }

  [GeneratedRegex(@"^\bint\b")]
  private static partial Regex IntRegex { get; }

  [GeneratedRegex(@"^\bvoid\b")]
  private static partial Regex VoidRegex { get; }

  [GeneratedRegex(@"^\breturn\b")]
  private static partial Regex ReturnRegex { get; }

  [GeneratedRegex(@"^[a-zA-Z_]\w*\b")]
  private static partial Regex IdentifierRegex { get; }

  [GeneratedRegex(@"^\(")]
  private static partial Regex OpenParenthesisRegex { get; }

  [GeneratedRegex(@"^\)")]
  private static partial Regex CloseParenthesisRegex { get; }

  [GeneratedRegex(@"^{")]
  private static partial Regex OpenBraceRegex { get; }

  [GeneratedRegex(@"^}")]
  private static partial Regex CloseBraceRegex { get; }

  [GeneratedRegex(@"^;")]
  private static partial Regex SemicolonRegex { get; }
}
