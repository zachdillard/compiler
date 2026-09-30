using System.Text.RegularExpressions;

public partial class Lexer
{
    public static List<Token> Run(string input)
    {
        List<Token> tokens = [];

        while (input != string.Empty)
        {
            input = input.TrimStart();
            if (input == string.Empty)
            {
                break;
            }

            bool matched = false;

            foreach ((Type type, Regex pattern) in Patterns)
            {
                Match match = pattern.Match(input);
                if (!match.Success || match.Index != 0)
                {
                    continue;
                }

                tokens.Add(CreateToken(type, match.Value));
                input = input[match.Length..];
                matched = true;
                break;
            }

            if (!matched)
            {
                throw new InvalidOperationException($"Unexpected character: '{input[0]}'");
            }
        }

        return tokens;
    }

    private static Token CreateToken(Type type, string value) => type switch
    {
        var _ when type == typeof(Identifier) => new Identifier(value),
        var _ when type == typeof(Int) => new Int(),
        var _ when type == typeof(Void) => new Void(),
        var _ when type == typeof(Return) => new Return(),
        var _ when type == typeof(Constant) => new Constant(int.Parse(value)),
        var _ when type == typeof(OpenParenthesis) => new OpenParenthesis(),
        var _ when type == typeof(CloseParenthesis) => new CloseParenthesis(),
        var _ when type == typeof(OpenBrace) => new OpenBrace(),
        var _ when type == typeof(CloseBrace) => new CloseBrace(),
        var _ when type == typeof(Semicolon) => new Semicolon(),
        _ => throw new InvalidOperationException("Unknown lexer pattern.")
    };

    private static readonly List<KeyValuePair<Type, Regex>> Patterns =
    [
        new(typeof(Int), IntRegex),
        new(typeof(Void), VoidRegex),
        new(typeof(Return), ReturnRegex),
        new(typeof(Identifier), IdentifierRegex),
        new(typeof(Constant), ConstantRegex),
        new(typeof(OpenParenthesis), OpenParenthesisRegex),
        new(typeof(CloseParenthesis), CloseParenthesisRegex),
        new(typeof(OpenBrace), OpenBraceRegex),
        new(typeof(CloseBrace), CloseBraceRegex),
        new(typeof(Semicolon), SemicolonRegex)
    ];

    [GeneratedRegex(@"\bint\b")]
    private static partial Regex IntRegex { get; }

    [GeneratedRegex(@"\bvoid\b")]
    private static partial Regex VoidRegex { get; }

    [GeneratedRegex(@"\breturn\b")]
    private static partial Regex ReturnRegex { get; }

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
