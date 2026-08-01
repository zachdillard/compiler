using System.Text.RegularExpressions;

public partial class Lexer 
{
    public static void Run(string input)
    {
        int length = 1;
        for (int index = 0; index < input.Length; index++)
        {
            ReadOnlySpan<char> lexeme = input.AsSpan(index, length);
        }
    }

    private readonly union Token
    (
        IdentifierToken,
        ConstantToken,
        IntKeywordToken,
        VoidKeywordToken,
        ReturnKeywordToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        OpenBraceToken,
        CloseBraceToken,
        SemicolonToken
    );

    [GeneratedRegex(@"[a-zA-Z_]\w*\b", RegexOptions.Compiled)]
    public static partial Regex IdentifierPattern { get; }

    private partial record struct IdentifierToken
    {
        public required string Value { get; set; }
    }

    [GeneratedRegex(@"[0-9]+\b", RegexOptions.Compiled)]
    public static partial Regex ConstantPattern { get; }

    private partial record struct ConstantToken
    {
        public required string Value { get; set; }
    }

    private partial record struct IntKeywordToken
    {
        [GeneratedRegex(@"int\b", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }

    private partial record struct VoidKeywordToken
    {
        [GeneratedRegex(@"void\b", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }

    private partial record struct ReturnKeywordToken
    {
        [GeneratedRegex(@"return\b", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }

    private partial record struct OpenParenthesisToken
    {
        [GeneratedRegex(@"\(", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }

    private partial record struct CloseParenthesisToken
    {
        [GeneratedRegex(@"\)", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }

    private partial record struct OpenBraceToken
    {
        [GeneratedRegex(@"\{", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }

    private partial record struct CloseBraceToken
    {
        [GeneratedRegex(@"\}", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }

    private partial record struct SemicolonToken
    {
        [GeneratedRegex(@";", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
    }
}
