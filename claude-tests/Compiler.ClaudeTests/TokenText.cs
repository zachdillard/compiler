namespace ClaudeTests;

// Renders tokens as short strings so token lists are easy to compare and read in failures.
public static class TokenText
{
    public static string Describe(Token token) => token switch
    {
        Identifier identifier => $"Identifier({identifier.Value})",
        Constant constant => $"Constant({constant.Value})",
        Int => "int",
        Void => "void",
        Return => "return",
        OpenParenthesis => "(",
        CloseParenthesis => ")",
        OpenBrace => "{",
        CloseBrace => "}",
        Semicolon => ";",
        _ => "unknown"
    };

    public static string[] Lex(string source) => Lexer.Run(source).Select(Describe).ToArray();
}
