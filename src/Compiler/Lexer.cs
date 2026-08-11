using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;

// int main(void) { return 2; }

public partial class Lexer 
{
    public static void Start(string input)
    {
        string lexeme;

        while (input != string.Empty)
        {
            input = input.TrimStart(); 

            foreach (Regex pattern in Patterns)
            {
                
            }
        }
    }

    public static void Run(string input)
    {
        int length = 1;
        ReadOnlySpan<char> lexeme;
        for (int index = 0; index < input.Length; index++)
        {
            if (char.IsWhiteSpace(input[index]))
            {
                continue;
            }

            lexeme = input.AsSpan(index, length);
            foreach (Regex pattern in Patterns)
            {
                if (pattern.IsMatch(lexeme))
                {
                    break;
                }
            }
        }
    }

    private static bool IsMatch()
    {
        return false;
    }

    private static readonly Regex[] Patterns =
    [
        IdentifierPattern,
        ConstantPattern,
        IntKeywordPattern,
        VoidKeywordPattern,
        ReturnKeywordPattern,
        OpenParenthesisPattern,
        CloseParenthesisPattern,
        OpenBracePattern,
        CloseBracePattern,
        SemicolonPattern,
    ];

    [GeneratedRegex(@"[a-zA-Z_]\w*\b", RegexOptions.Compiled)]
    private static partial Regex IdentifierPattern { get; }

    [GeneratedRegex(@"[0-9]+\b", RegexOptions.Compiled)]
    private static partial Regex ConstantPattern { get; }

    [GeneratedRegex(@"int\b", RegexOptions.Compiled)]
    private static partial Regex IntKeywordPattern { get; }

    [GeneratedRegex(@"void\b", RegexOptions.Compiled)]
    private static partial Regex VoidKeywordPattern { get; }

    [GeneratedRegex(@"return\b", RegexOptions.Compiled)]
    private static partial Regex ReturnKeywordPattern { get; }

    [GeneratedRegex(@"\(", RegexOptions.Compiled)]
    private static partial Regex OpenParenthesisPattern { get; }

    [GeneratedRegex(@"\)", RegexOptions.Compiled)]
    private static partial Regex CloseParenthesisPattern { get; }

    [GeneratedRegex(@"\{", RegexOptions.Compiled)]
    private static partial Regex OpenBracePattern { get; }

    [GeneratedRegex(@"\}", RegexOptions.Compiled)]
    private static partial Regex CloseBracePattern { get; }

    [GeneratedRegex(@";", RegexOptions.Compiled)]
    private static partial Regex SemicolonPattern { get; }

    private partial record struct IdentifierToken
    {
        public string Value;
    }

    private partial record struct ConstantToken
    {
        public string Value;
    }

    private partial record struct IntKeywordToken
    {
    }

    private partial record struct VoidKeywordToken
    {
    }

    private partial record struct ReturnKeywordToken
    {
    }

    private partial record struct OpenParenthesisToken
    {
    }

    private partial record struct CloseParenthesisToken
    {
    }

    private partial record struct OpenBraceToken
    {
    }

    private partial record struct CloseBraceToken
    {
    }

    private partial record struct SemicolonToken
    {
    }
}
