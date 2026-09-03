using System.Text.RegularExpressions;

public partial class Lexer
{
    public static List<Token> Run(string input)
    {
        var tokens = new List<Token>();

        while (input != string.Empty)
        {
            input = input.TrimStart();
            if (input == string.Empty)
            {
                break;
            }

            var matched = false;

            foreach (var pattern in Patterns)
            {
                var match = pattern.Match(input);
                if (!match.Success || match.Index != 0)
                {
                    continue;
                }

                matched = true;
                var value = match.Value;

                tokens.Add(new Token(pattern switch
                {
                    var identifier when identifier == IdentifierRegex =>
                        Array.IndexOf(Keywords, value) >= 0 ? TokenType.Keyword : TokenType.Identifier,
                    var constant when constant == ConstantRegex => TokenType.Constant,
                    var openParenthesis when openParenthesis == OpenParenthesisRegex => TokenType.OpenParenthesis,
                    var closeParenthesis when closeParenthesis == CloseParenthesisRegex => TokenType.CloseParenthesis,
                    var openBrace when openBrace == OpenBraceRegex => TokenType.OpenBrace,
                    var closeBrace when closeBrace == CloseBraceRegex => TokenType.CloseBrace,
                    var semicolon when semicolon == SemicolonRegex => TokenType.Semicolon,
                    _ => throw new InvalidOperationException("Unknown lexer pattern.")
                }, value));

                input = input[match.Length..];
                break;
            }

            if (!matched)
            {
                throw new InvalidOperationException($"Unexpected character: '{input[0]}'");
            }
        }

        return tokens;
    }

    private static readonly Regex[] Patterns =
    [
        IdentifierRegex,
        ConstantRegex,
        OpenParenthesisRegex,
        CloseParenthesisRegex,
        OpenBraceRegex,
        CloseBraceRegex,
        SemicolonRegex
    ];

    private static readonly string[] Keywords = ["int", "void", "return"];

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
