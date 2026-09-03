public enum TokenType
{
    Identifier,
    Keyword,
    Constant,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    Semicolon
}

public record Token(TokenType Type, string Value)
{
    public override string ToString() => $"{Name}: {Value}";

    private string Name => Type switch
    {
        TokenType.Identifier => "identifier",
        TokenType.Keyword => "keyword",
        TokenType.Constant => "constant",
        TokenType.OpenParenthesis => "open_parenthesis",
        TokenType.CloseParenthesis => "close_parenthesis",
        TokenType.OpenBrace => "open_brace",
        TokenType.CloseBrace => "close_brace",
        TokenType.Semicolon => "semicolon",
        _ => throw new InvalidOperationException("Unknown token type.")
    };
}
