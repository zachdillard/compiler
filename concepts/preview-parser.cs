#!/usr/bin/env -S dotnet --
#:property LangVersion=preview

List<Token> tokens =
[
    new Int(),
    new Identifier("main"),
    new OpenParenthesis(),
    new Void(),
    new CloseParenthesis(),
    new OpenBrace(),
    new Return(),
    new Constant(2),
    new Semicolon(),
    new CloseBrace()
];

Syntax.Program program = ParseProgram(tokens);
Console.WriteLine(program);

Syntax.Program ParseProgram(List<Token> tokens)
{
    return new Syntax.Program(ParseFunction(tokens));
}

Syntax.Function ParseFunction(List<Token> tokens)
{
    Expect<Int>(tokens);
    string identifier = ParseIdentifier(tokens);
    Expect<OpenParenthesis>(tokens);
    Expect<Void>(tokens);
    Expect<CloseParenthesis>(tokens);
    Expect<OpenBrace>(tokens);
    Syntax.Return statement = ParseStatement(tokens);
    Expect<CloseBrace>(tokens);

    return new Syntax.Function(identifier, statement);
}

Syntax.Return ParseStatement(List<Token> tokens)
{
    Expect<Return>(tokens);
    Syntax.Constant expression = ParseExpression(tokens);
    Expect<Semicolon>(tokens);

    return new Syntax.Return(expression);
}

Syntax.Constant ParseExpression(List<Token> tokens)
{
    return new Syntax.Constant(ParseInt(tokens));
}

string ParseIdentifier(List<Token> tokens)
{
    if (TakeToken(tokens) is Identifier identifier)
        return identifier.Value;

    throw new InvalidOperationException();
}

int ParseInt(List<Token> tokens)
{
    if (TakeToken(tokens) is Constant constant)
        return constant.Value;

    throw new InvalidOperationException();
}

void Expect<T>(List<Token> tokens)
{
    if (TakeToken(tokens) is not T)
        throw new InvalidOperationException();
}

Token TakeToken(List<Token> tokens)
{
    Token token = tokens.First();
    tokens.RemoveAt(0);
    return token;
}

readonly union Token
(
    Identifier,
    Int,
    Void,
    Return,
    Constant,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    Semicolon
);

record Identifier(string Value);
record Int();
record Void();
record Return();
record Constant(int Value);
record OpenParenthesis();
record CloseParenthesis();
record OpenBrace();
record CloseBrace();
record Semicolon();

namespace Syntax
{
    record Program(Function Function);
    record Function(string Identifier, Return Statement);
    record Return(Constant Expression);
    record Constant(int Value);
}
