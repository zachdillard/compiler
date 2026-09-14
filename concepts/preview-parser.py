import re
from dataclasses import dataclass
from enum import Enum, auto
from typing import Optional, Union


class TokenKind(Enum):
    IDENTIFIER = auto()
    INT = auto()
    VOID = auto()
    RETURN = auto()
    CONSTANT = auto()
    OPEN_PARENTHESIS = auto()
    CLOSE_PARENTHESIS = auto()
    OPEN_BRACE = auto()
    CLOSE_BRACE = auto()
    SEMICOLON = auto()


@dataclass(frozen=True)
class Token:
    kind: TokenKind
    value: Optional[Union[str, int]] = None


PATTERNS = [
    (TokenKind.INT, re.compile(r"int\b")),
    (TokenKind.VOID, re.compile(r"void\b")),
    (TokenKind.RETURN, re.compile(r"return\b")),
    (TokenKind.IDENTIFIER, re.compile(r"[a-zA-Z_]\w*\b")),
    (TokenKind.CONSTANT, re.compile(r"[0-9]+\b")),
    (TokenKind.OPEN_PARENTHESIS, re.compile(r"\(")),
    (TokenKind.CLOSE_PARENTHESIS, re.compile(r"\)")),
    (TokenKind.OPEN_BRACE, re.compile(r"{")),
    (TokenKind.CLOSE_BRACE, re.compile(r"}")),
    (TokenKind.SEMICOLON, re.compile(r";")),
]


@dataclass(frozen=True)
class Program:
    function: "Function"


@dataclass(frozen=True)
class Function:
    identifier: str
    statement: "Return"


@dataclass(frozen=True)
class Return:
    expression: "Constant"


@dataclass(frozen=True)
class Constant:
    value: int


def lex(source: str) -> list[Token]:
    tokens = []
    while source:
        source = source.lstrip()
        if not source:
            break

        for kind, pattern in PATTERNS:
            match = pattern.match(source)
            if match:
                text = match.group()
                value = int(text) if kind is TokenKind.CONSTANT else text if kind is TokenKind.IDENTIFIER else None
                tokens.append(Token(kind, value))
                source = source[len(text):]
                break
        else:
            raise ValueError(f"unexpected character: {source[0]!r}")

    return tokens


class Parser:
    def __init__(self, tokens: list[Token]):
        self.tokens = tokens
        self.current = 0

    def parse_program(self) -> Program:
        program = Program(self.parse_function())
        if self.current != len(self.tokens):
            raise ValueError("unexpected trailing token")
        return program

    def parse_function(self) -> Function:
        self.expect(TokenKind.INT)
        identifier = self.parse_identifier()
        self.expect(TokenKind.OPEN_PARENTHESIS)
        self.expect(TokenKind.VOID)
        self.expect(TokenKind.CLOSE_PARENTHESIS)
        self.expect(TokenKind.OPEN_BRACE)
        statement = self.parse_statement()
        self.expect(TokenKind.CLOSE_BRACE)
        return Function(identifier, statement)

    def parse_statement(self) -> Return:
        self.expect(TokenKind.RETURN)
        expression = self.parse_expression()
        self.expect(TokenKind.SEMICOLON)
        return Return(expression)

    def parse_expression(self) -> Constant:
        return Constant(self.parse_integer())

    def parse_identifier(self) -> str:
        token = self.take()
        if token.kind is not TokenKind.IDENTIFIER or not isinstance(token.value, str):
            raise ValueError("expected identifier")
        return token.value

    def parse_integer(self) -> int:
        token = self.take()
        if token.kind is not TokenKind.CONSTANT or not isinstance(token.value, int):
            raise ValueError("expected integer")
        return token.value

    def expect(self, expected: TokenKind) -> None:
        if self.take().kind is not expected:
            raise ValueError("unexpected token")

    def take(self) -> Token:
        if self.current == len(self.tokens):
            raise ValueError("unexpected end of input")
        token = self.tokens[self.current]
        self.current += 1
        return token


program = Parser(lex("int main(void) { return 2; }")).parse_program()
assert program.function.identifier == "main"
assert program.function.statement.expression.value == 2
print(
    f"Program {{ Function = Function {{ Identifier = {program.function.identifier}, "
    f"Statement = Return {{ Expression = Constant {{ Value = {program.function.statement.expression.value} }} }} }} }}"
)
