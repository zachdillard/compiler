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
