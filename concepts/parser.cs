List<string> tokens = ["int", "main", "(", "void", ")", "{", "return", "2", ";", "}"];
List<string> keywords = ["int", "void", "return"];

var program = ParseProgram(tokens);

ProgramNode ParseProgram(List<string> tokens)
{
    return new ProgramNode(ParseFunction(tokens));
}

FunctionNode ParseFunction(List<string> tokens)
{
    Expect("int", tokens);

    string identifier = TakeToken(tokens);
    if (keywords.Contains(identifier))
        throw new InvalidOperationException();

    Expect("(", tokens);
    Expect("void", tokens);
    Expect(")", tokens);
    Expect("{", tokens);
    StatementNode statement = ParseStatement(tokens);
    Expect("}", tokens);

    return new FunctionNode(identifier, statement);
}

StatementNode ParseStatement(List<string> tokens)
{
    Expect("return", tokens);
    ExpressionNode expression = ParseExpression(tokens);
    Expect(";", tokens);

    return new ReturnStatementNode(expression);
}

ExpressionNode ParseExpression(List<string> tokens)
{
    string token = TakeToken(tokens);
    if (int.TryParse(token, out int constant) == false)
        throw new InvalidOperationException();

    return new ConstantExpressionNode(constant);
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

abstract record AST;
record ProgramNode(FunctionNode Function) : AST;
record FunctionNode(string Identifier, StatementNode Statement) : AST;
record StatementNode() : AST;
record ReturnStatementNode(ExpressionNode Expression) : StatementNode;
record ExpressionNode() : AST;
record ConstantExpressionNode(int Constant) : ExpressionNode;
