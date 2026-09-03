public class Parser
{
    public static ProgramNode Run(IEnumerable<Token> tokens)
    {
        var remaining = tokens.ToList();
        var program = ParseProgram(remaining);

        if (remaining.Count > 0)
        {
            throw new InvalidOperationException($"Unexpected token after the program: '{remaining[0].Value}'.");
        }

        return program;
    }

    private static ProgramNode ParseProgram(List<Token> tokens)
    {
        return new ProgramNode(ParseFunction(tokens));
    }

    private static FunctionNode ParseFunction(List<Token> tokens)
    {
        Expect(TokenType.Keyword, "int", tokens);
        var identifier = Expect(TokenType.Identifier, tokens).Value;
        Expect(TokenType.OpenParenthesis, tokens);
        Expect(TokenType.Keyword, "void", tokens);
        Expect(TokenType.CloseParenthesis, tokens);
        Expect(TokenType.OpenBrace, tokens);
        var statement = ParseStatement(tokens);
        Expect(TokenType.CloseBrace, tokens);

        return new FunctionNode(identifier, statement);
    }

    private static StatementNode ParseStatement(List<Token> tokens)
    {
        Expect(TokenType.Keyword, "return", tokens);
        var expression = ParseExpression(tokens);
        Expect(TokenType.Semicolon, tokens);

        return new ReturnStatementNode(expression);
    }

    private static ExpressionNode ParseExpression(List<Token> tokens)
    {
        var token = Expect(TokenType.Constant, tokens);
        if (!int.TryParse(token.Value, out var constant))
        {
            throw new InvalidOperationException($"Constant is out of range: '{token.Value}'.");
        }

        return new ConstantExpressionNode(constant);
    }

    private static Token Expect(TokenType expected, List<Token> tokens)
    {
        var token = TakeToken(tokens);
        if (token.Type != expected)
        {
            throw new InvalidOperationException($"Expected {expected} but found '{token.Value}'.");
        }

        return token;
    }

    private static Token Expect(TokenType expected, string value, List<Token> tokens)
    {
        var token = Expect(expected, tokens);
        if (token.Value != value)
        {
            throw new InvalidOperationException($"Expected '{value}' but found '{token.Value}'.");
        }

        return token;
    }

    private static Token TakeToken(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Unexpected end of input.");
        }

        var token = tokens[0];
        tokens.RemoveAt(0);
        return token;
    }
}
