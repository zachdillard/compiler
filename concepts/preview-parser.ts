type Token =
  | { kind: "identifier"; value: string }
  | { kind: "int" }
  | { kind: "void" }
  | { kind: "return" }
  | { kind: "constant"; value: number }
  | { kind: "openParenthesis" }
  | { kind: "closeParenthesis" }
  | { kind: "openBrace" }
  | { kind: "closeBrace" }
  | { kind: "semicolon" };

type Program = { function: FunctionNode };
type FunctionNode = { identifier: string; statement: ReturnNode };
type ReturnNode = { expression: ConstantNode };
type ConstantNode = { value: number };

const patterns: [RegExp, (text: string) => Token][] = [
  [/^int\b/, () => ({ kind: "int" })],
  [/^void\b/, () => ({ kind: "void" })],
  [/^return\b/, () => ({ kind: "return" })],
  [/^[a-zA-Z_]\w*\b/, (text) => ({ kind: "identifier", value: text })],
  [/^[0-9]+\b/, (text) => ({ kind: "constant", value: Number(text) })],
  [/^\(/, () => ({ kind: "openParenthesis" })],
  [/^\)/, () => ({ kind: "closeParenthesis" })],
  [/^{/, () => ({ kind: "openBrace" })],
  [/^}/, () => ({ kind: "closeBrace" })],
  [/^;/, () => ({ kind: "semicolon" })],
];

function lex(input: string): Token[] {
  const tokens: Token[] = [];

  while (input !== "") {
    input = input.trimStart();
    if (input === "") break;

    const match = patterns.find(([pattern]) => pattern.test(input));
    if (!match) throw new Error(`unexpected character: '${input[0]}'`);

    const [pattern, createToken] = match;
    const text = pattern.exec(input)![0];
    tokens.push(createToken(text));
    input = input.slice(text.length);
  }

  return tokens;
}

class Parser {
  tokens: Token[];
  current = 0;

  constructor(tokens: Token[]) {
    this.tokens = tokens;
  }

  parseProgram(): Program {
    const program = { function: this.parseFunction() };
    if (this.current !== this.tokens.length) {
      throw new Error("unexpected trailing token");
    }
    return program;
  }

  parseFunction(): FunctionNode {
    this.expect("int");
    const identifier = this.parseIdentifier();
    this.expect("openParenthesis");
    this.expect("void");
    this.expect("closeParenthesis");
    this.expect("openBrace");
    const statement = this.parseStatement();
    this.expect("closeBrace");
    return { identifier, statement };
  }

  parseStatement(): ReturnNode {
    this.expect("return");
    const expression = this.parseExpression();
    this.expect("semicolon");
    return { expression };
  }

  parseExpression(): ConstantNode {
    return { value: this.parseInteger() };
  }

  parseIdentifier(): string {
    const token = this.take();
    if (token.kind !== "identifier") throw new Error("expected identifier");
    return token.value;
  }

  parseInteger(): number {
    const token = this.take();
    if (token.kind !== "constant") throw new Error("expected integer");
    return token.value;
  }

  expect(expected: Token["kind"]): void {
    if (this.take().kind !== expected) throw new Error("unexpected token");
  }

  take(): Token {
    const token = this.tokens[this.current];
    if (!token) throw new Error("unexpected end of input");
    this.current++;
    return token;
  }
}

const program = new Parser(lex("int main(void) { return 2; }")).parseProgram();
if (program.function.identifier !== "main" || program.function.statement.expression.value !== 2) {
  throw new Error("unexpected syntax tree");
}
console.log(
  `Program { Function = Function { Identifier = ${program.function.identifier}, Statement = Return { Expression = Constant { Value = ${program.function.statement.expression.value} } } } }`,
);
