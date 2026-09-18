public class Parser
{
    public C.Program Parse(IReadOnlyList<Token> tokens)
    {
        int position = 0;

        Token Take()
        {
            if (position == tokens.Count)
                throw new InvalidOperationException("Unexpected end of input.");
            return tokens[position++];
        }

        Token Expect(TokenKind kind)
        {
            Token token = Take();
            if (token.Kind != kind)
                throw new InvalidOperationException($"Expected {kind}, found '{token.Text}'.");
            return token;
        }

        Expect(TokenKind.Int);
        string identifier = Expect(TokenKind.Identifier).Text;
        Expect(TokenKind.OpenParenthesis);
        Expect(TokenKind.Void);
        Expect(TokenKind.CloseParenthesis);
        Expect(TokenKind.OpenBrace);
        Expect(TokenKind.Return);
        string value = Expect(TokenKind.Constant).Text;
        if (!int.TryParse(value, out int constant))
            throw new InvalidOperationException($"Integer constant is out of range: {value}.");
        Expect(TokenKind.Semicolon);
        Expect(TokenKind.CloseBrace);
        if (position != tokens.Count)
            throw new InvalidOperationException($"Unexpected token: '{tokens[position].Text}'.");

        return new C.Program(new C.Function(identifier, new C.Return(new C.Constant(constant))));
    }
}
