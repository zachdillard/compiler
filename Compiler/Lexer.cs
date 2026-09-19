using System.Text.RegularExpressions;

partial class Lexer
{
    public static List<Token> Run(string input)
    {
        List<Token> tokens = [];

        while (input != string.Empty)
        {
            input = input.TrimStart();
            if (input == string.Empty)
                break;

            bool matched = false;

            foreach ((Type type, Regex pattern) in Patterns)
            {
                Match match = pattern.Match(input);
                if (!match.Success || match.Index != 0)
                    continue;

                tokens.Add(CreateToken(type, match.Value));
                input = input[match.Length..];
                matched = true;
                break;
            }

            if (!matched)
                throw new CompilerException($"Unexpected character: '{input[0]}'");
        }

        return tokens;
    }

    public static string Describe(Token token) => token switch
    {
        Identifier identifier => $"identifier: {identifier.Value}",
        Int => "keyword: int",
        Void => "keyword: void",
        Return => "keyword: return",
        Constant constant => $"constant: {constant.Value}",
        OpenParenthesis => "open_parenthesis: (",
        CloseParenthesis => "close_parenthesis: )",
        OpenBrace => "open_brace: {",
        CloseBrace => "close_brace: }",
        Semicolon => "semicolon: ;",
        _ => throw new CompilerException("Unknown token.")
    };

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
        _ => throw new CompilerException($"Unknown token type: {type}.")
    };

    private static readonly List<KeyValuePair<Type, Regex>> Patterns =
    [
        new(typeof(Int), IntPattern),
        new(typeof(Void), VoidPattern),
        new(typeof(Return), ReturnPattern),
        new(typeof(Identifier), IdentifierPattern),
        new(typeof(Constant), ConstantPattern),
        new(typeof(OpenParenthesis), OpenParenthesisPattern),
        new(typeof(CloseParenthesis), CloseParenthesisPattern),
        new(typeof(OpenBrace), OpenBracePattern),
        new(typeof(CloseBrace), CloseBracePattern),
        new(typeof(Semicolon), SemicolonPattern)
    ];

    [GeneratedRegex(@"[a-zA-Z_]\w*\b")]
    private static partial Regex IdentifierPattern { get; }

    [GeneratedRegex(@"\bint\b")]
    private static partial Regex IntPattern { get; }

    [GeneratedRegex(@"\bvoid\b")]
    private static partial Regex VoidPattern { get; }

    [GeneratedRegex(@"\breturn\b")]
    private static partial Regex ReturnPattern { get; }

    [GeneratedRegex(@"[0-9]+\b")]
    private static partial Regex ConstantPattern { get; }

    [GeneratedRegex(@"\(")]
    private static partial Regex OpenParenthesisPattern { get; }

    [GeneratedRegex(@"\)")]
    private static partial Regex CloseParenthesisPattern { get; }

    [GeneratedRegex(@"{")]
    private static partial Regex OpenBracePattern { get; }

    [GeneratedRegex(@"}")]
    private static partial Regex CloseBracePattern { get; }

    [GeneratedRegex(@";")]
    private static partial Regex SemicolonPattern { get; }
}
