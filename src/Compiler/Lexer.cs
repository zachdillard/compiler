using System.Text.RegularExpressions;

public enum TokenKind
{
    Identifier, Int, Void, Return, Constant,
    OpenParenthesis, CloseParenthesis, OpenBrace, CloseBrace, Semicolon
}

public record Token(TokenKind Kind, string Text);

public partial class Lexer
{
    public static List<Token> Tokenize(string input) => Scan(input).ToList();

    private static IEnumerable<Token> Scan(string input)
    {
        while (input != string.Empty)
        {
            input = input.TrimStart();
            if (input == string.Empty)
                break;

            bool matched = false;
            foreach (Regex pattern in Patterns)
            {
                Match match = pattern.Match(input);
                if (!match.Success || match.Index != 0)
                    continue;

                string value = match.Value;
                TokenKind kind = pattern == IdentifierRegex ? value switch
                {
                    "int" => TokenKind.Int,
                    "void" => TokenKind.Void,
                    "return" => TokenKind.Return,
                    _ => TokenKind.Identifier
                } : pattern == ConstantRegex ? TokenKind.Constant
                  : pattern == OpenParenthesisRegex ? TokenKind.OpenParenthesis
                  : pattern == CloseParenthesisRegex ? TokenKind.CloseParenthesis
                  : pattern == OpenBraceRegex ? TokenKind.OpenBrace
                  : pattern == CloseBraceRegex ? TokenKind.CloseBrace
                  : TokenKind.Semicolon;

                yield return new Token(kind, value);
                input = input[match.Length..];
                matched = true;
                break;
            }

            if (!matched)
                throw new InvalidOperationException($"Unexpected character: '{input[0]}'");
        }
    }

    public static void Run(string input)
    {
        foreach (Token token in Scan(input))
        {
            string category = token.Kind switch
            {
                TokenKind.Identifier => "identifier",
                TokenKind.Int or TokenKind.Void or TokenKind.Return => "keyword",
                TokenKind.Constant => "constant",
                TokenKind.OpenParenthesis => "open_parenthesis",
                TokenKind.CloseParenthesis => "close_parenthesis",
                TokenKind.OpenBrace => "open_brace",
                TokenKind.CloseBrace => "close_brace",
                TokenKind.Semicolon => "semicolon",
                _ => throw new InvalidOperationException("Unknown token kind.")
            };
            Console.WriteLine($"{category}: {token.Text}");
        }
    }

    private static readonly Regex[] Patterns =
    [
        IdentifierRegex, ConstantRegex, OpenParenthesisRegex,
        CloseParenthesisRegex, OpenBraceRegex, CloseBraceRegex, SemicolonRegex
    ];

    [GeneratedRegex(@"[a-zA-Z_]\w*\b")]
    private static partial Regex IdentifierRegex { get; }

    [GeneratedRegex(@"[0-9]+\b")]
    private static partial Regex ConstantRegex { get; }

    [GeneratedRegex(@"\(")]
    private static partial Regex OpenParenthesisRegex { get; }

    [GeneratedRegex(@"\)")]
    private static partial Regex CloseParenthesisRegex { get; }

    [GeneratedRegex(@"{")]
    private static partial Regex OpenBraceRegex { get; }

    [GeneratedRegex(@"}")]
    private static partial Regex CloseBraceRegex { get; }

    [GeneratedRegex(@";")]
    private static partial Regex SemicolonRegex { get; }
}
