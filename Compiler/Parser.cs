using C = Compiler.Syntax.C;

namespace Compiler;

class Parser
{
    public static C.Program Run(List<Token> tokens)
    {
        C.Function function = ParseFunction(tokens);
        if (tokens.Count > 0)
            throw new CompilerException("Unexpected token after the end of the program.");

        return new C.Program(function);
    }

    private static C.Function ParseFunction(List<Token> tokens)
    {
        Expect<Int>(tokens);
        string identifier = ParseIdentifier(tokens);
        Expect<OpenParenthesis>(tokens);
        Expect<Void>(tokens);
        Expect<CloseParenthesis>(tokens);
        Expect<OpenBrace>(tokens);
        C.Return statement = ParseStatement(tokens);
        Expect<CloseBrace>(tokens);

        return new C.Function(identifier, statement);
    }

    private static C.Return ParseStatement(List<Token> tokens)
    {
        Expect<Return>(tokens);
        C.Constant expression = ParseExpression(tokens);
        Expect<Semicolon>(tokens);

        return new C.Return(expression);
    }

    private static C.Constant ParseExpression(List<Token> tokens)
    {
        return new C.Constant(ParseInt(tokens));
    }

    private static string ParseIdentifier(List<Token> tokens)
    {
        if (TakeToken(tokens) is Identifier identifier)
            return identifier.Value;

        throw new CompilerException("Expected an identifier.");
    }

    private static int ParseInt(List<Token> tokens)
    {
        if (TakeToken(tokens) is Constant constant)
            return constant.Value;

        throw new CompilerException("Expected a constant.");
    }

    private static void Expect<T>(List<Token> tokens)
    {
        if (TakeToken(tokens) is not T)
            throw new CompilerException($"Expected {typeof(T).Name}.");
    }

    private static Token TakeToken(List<Token> tokens)
    {
        if (tokens.Count == 0)
            throw new CompilerException("Unexpected end of input.");

        Token token = tokens.First();
        tokens.RemoveAt(0);
        return token;
    }
}
