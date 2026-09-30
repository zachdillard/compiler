using System.Text.RegularExpressions;

public partial class Lexer
{
    public static void Run(string input)
    {
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

                Console.WriteLine(pattern switch
                {
                    var identifier when identifier == IdentifierRegex =>
                        $"{(Array.IndexOf(Keywords, value) >= 0 ? "keyword" : "identifier")}: {value}",
                    var constant when constant == ConstantRegex => $"constant: {value}",
                    var openParenthesis when openParenthesis == OpenParenthesisRegex => $"open_parenthesis: {value}",
                    var closeParenthesis when closeParenthesis == CloseParenthesisRegex => $"close_parenthesis: {value}",
                    var openBrace when openBrace == OpenBraceRegex => $"open_brace: {value}",
                    var closeBrace when closeBrace == CloseBraceRegex => $"close_brace: {value}",
                    var semicolon when semicolon == SemicolonRegex => $"semicolon: {value}",
                    _ => throw new InvalidOperationException("Unknown lexer pattern.")
                });

                input = input[match.Length..];
                break;
            }

            if (!matched)
            {
                throw new InvalidOperationException($"Unexpected character: '{input[0]}'");
            }
        }
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
