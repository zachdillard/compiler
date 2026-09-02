List<string> tokens = [];

var program = ParseProgram(tokens);

ProgramNode ParseProgram(List<string> tokens)
{
    return null;
}

FunctionNode ParseFunction(List<string> tokens)
{
    return null;
}

StatementNode ParseStatement(List<string> tokens)
{
    Expect("return", tokens);
    ExpressionNode returnValue = ParseExpression(tokens);
    Expect(";", tokens);
    return new StatementNode(returnValue);
}

ExpressionNode ParseExpression(List<string> tokens)
{
    return new ExpressionNode(0);
}

void Expect(string expected, List<string> tokens)
{
    string actual = TakeToken(tokens);
    if (actual != expected)
        throw new InvalidOperationException();
}

string TakeToken(List<string> tokens)
{
    string token = tokens.First();
    tokens.RemoveAt(0);
    return token;
}

abstract record Node;
record ProgramNode(FunctionNode FunctionDefinition) : Node;
record FunctionNode(string Name, StatementNode Body) : Node;
record StatementNode(ExpressionNode Return) : Node;
record ExpressionNode(int Constant) : Node;
