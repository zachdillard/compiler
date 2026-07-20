public class Lexer
{
    public union Token(Keyword, Identifier, Symbol, Constant);
    public sealed record Keyword(string Value);
    public sealed record Identifier(string Value);
    public sealed record Symbol(string Value);
    public sealed record Constant(string Value);

    public void Run(string input)
    {
        
    }
}
