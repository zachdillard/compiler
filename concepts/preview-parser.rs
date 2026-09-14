#[derive(Clone, Copy, PartialEq)]
enum Token<'a> {
    Identifier(&'a str),
    Int,
    Void,
    Return,
    Constant(u32),
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    Semicolon,
}

struct Program<'a> {
    function: Function<'a>,
}

struct Function<'a> {
    identifier: &'a str,
    statement: Return,
}

struct Return {
    expression: Constant,
}

struct Constant {
    value: u32,
}

fn main() {
    let tokens = lex("int main(void) { return 2; }").expect("lex failed");
    let mut parser = Parser::new(&tokens);
    let program = parser.parse_program().expect("parse failed");

    assert_eq!(program.function.identifier, "main");
    assert_eq!(program.function.statement.expression.value, 2);
    println!(
        "Program {{ Function = Function {{ Identifier = {}, Statement = Return {{ Expression = Constant {{ Value = {} }} }} }} }}",
        program.function.identifier, program.function.statement.expression.value
    );
}

fn lex(input: &str) -> Result<Vec<Token<'_>>, &'static str> {
    let bytes = input.as_bytes();
    let mut tokens = Vec::new();
    let mut index = 0;

    while index < bytes.len() {
        if bytes[index].is_ascii_whitespace() {
            index += 1;
            continue;
        }

        if bytes[index].is_ascii_alphabetic() || bytes[index] == b'_' {
            let start = index;
            index += 1;
            while index < bytes.len()
                && (bytes[index].is_ascii_alphanumeric() || bytes[index] == b'_')
            {
                index += 1;
            }

            tokens.push(match &input[start..index] {
                "int" => Token::Int,
                "void" => Token::Void,
                "return" => Token::Return,
                identifier => Token::Identifier(identifier),
            });
            continue;
        }

        if bytes[index].is_ascii_digit() {
            let start = index;
            while index < bytes.len() && bytes[index].is_ascii_digit() {
                index += 1;
            }
            let value = input[start..index]
                .parse()
                .map_err(|_| "invalid integer")?;
            tokens.push(Token::Constant(value));
            continue;
        }

        tokens.push(match bytes[index] {
            b'(' => Token::OpenParenthesis,
            b')' => Token::CloseParenthesis,
            b'{' => Token::OpenBrace,
            b'}' => Token::CloseBrace,
            b';' => Token::Semicolon,
            _ => return Err("unexpected character"),
        });
        index += 1;
    }

    Ok(tokens)
}

struct Parser<'a> {
    tokens: &'a [Token<'a>],
    current: usize,
}

impl<'a> Parser<'a> {
    fn new(tokens: &'a [Token<'a>]) -> Self {
        Self { tokens, current: 0 }
    }

    fn parse_program(&mut self) -> Result<Program<'a>, &'static str> {
        let program = Program {
            function: self.parse_function()?,
        };
        if self.current != self.tokens.len() {
            return Err("unexpected trailing token");
        }
        Ok(program)
    }

    fn parse_function(&mut self) -> Result<Function<'a>, &'static str> {
        self.expect(Token::Int)?;
        let identifier = self.parse_identifier()?;
        self.expect(Token::OpenParenthesis)?;
        self.expect(Token::Void)?;
        self.expect(Token::CloseParenthesis)?;
        self.expect(Token::OpenBrace)?;
        let statement = self.parse_statement()?;
        self.expect(Token::CloseBrace)?;
        Ok(Function {
            identifier,
            statement,
        })
    }

    fn parse_statement(&mut self) -> Result<Return, &'static str> {
        self.expect(Token::Return)?;
        let expression = self.parse_expression()?;
        self.expect(Token::Semicolon)?;
        Ok(Return { expression })
    }

    fn parse_expression(&mut self) -> Result<Constant, &'static str> {
        Ok(Constant {
            value: self.parse_integer()?,
        })
    }

    fn parse_identifier(&mut self) -> Result<&'a str, &'static str> {
        match self.take()? {
            Token::Identifier(identifier) => Ok(identifier),
            _ => Err("expected identifier"),
        }
    }

    fn parse_integer(&mut self) -> Result<u32, &'static str> {
        match self.take()? {
            Token::Constant(constant) => Ok(constant),
            _ => Err("expected integer"),
        }
    }

    fn expect(&mut self, expected: Token<'a>) -> Result<(), &'static str> {
        if self.take()? != expected {
            return Err("unexpected token");
        }
        Ok(())
    }

    fn take(&mut self) -> Result<Token<'a>, &'static str> {
        let token = self
            .tokens
            .get(self.current)
            .copied()
            .ok_or("unexpected end of input")?;
        self.current += 1;
        Ok(token)
    }
}
