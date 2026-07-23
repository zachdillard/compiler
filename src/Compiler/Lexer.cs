using System.Text.RegularExpressions;

public partial class Lexer 
{
    private partial record struct IdentifierToken
    {
        [GeneratedRegex(@"[a-zA-Z_]\w*\b", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
        public required string Value { get; set; }
    }

    private partial record struct ConstantToken
    {
        [GeneratedRegex(@"[0-9]+\b", RegexOptions.Compiled)]
        public static partial Regex Pattern { get; }
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

    private readonly union Token(
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

    public void Run(string input)
    {
        string lexeme = string.Empty;
        foreach (char character in input)
        {
        }
    }
}
