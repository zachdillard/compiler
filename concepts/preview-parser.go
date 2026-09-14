package main

import (
	"fmt"
	"strconv"
)

type TokenKind int

const (
	Identifier TokenKind = iota
	IntKeyword
	VoidKeyword
	ReturnKeyword
	ConstantToken
	OpenParenthesis
	CloseParenthesis
	OpenBrace
	CloseBrace
	Semicolon
)

type Token struct {
	Kind   TokenKind
	Text   string
	Number int
}

type Program struct{ Function Function }
type Function struct {
	Identifier string
	Statement  Return
}
type Return struct{ Expression Constant }
type Constant struct{ Value int }

func main() {
	tokens := lex("int main(void) { return 2; }")
	parser := Parser{tokens: tokens}
	program := parser.parseProgram()

	if program.Function.Identifier != "main" || program.Function.Statement.Expression.Value != 2 {
		panic("unexpected syntax tree")
	}

	fmt.Printf(
		"Program { Function = Function { Identifier = %s, Statement = Return { Expression = Constant { Value = %d } } } }\n",
		program.Function.Identifier,
		program.Function.Statement.Expression.Value,
	)
}

func lex(input string) []Token {
	var tokens []Token

	for index := 0; index < len(input); {
		if input[index] == ' ' || input[index] == '\t' || input[index] == '\n' || input[index] == '\r' {
			index++
			continue
		}

		if isIdentifierStart(input[index]) {
			start := index
			index++
			for index < len(input) && isIdentifierPart(input[index]) {
				index++
			}

			word := input[start:index]
			kind := Identifier
			switch word {
			case "int":
				kind = IntKeyword
			case "void":
				kind = VoidKeyword
			case "return":
				kind = ReturnKeyword
			}
			tokens = append(tokens, Token{Kind: kind, Text: word})
			continue
		}

		if isDigit(input[index]) {
			start := index
			for index < len(input) && isDigit(input[index]) {
				index++
			}
			value, err := strconv.Atoi(input[start:index])
			if err != nil {
				panic(err)
			}
			tokens = append(tokens, Token{Kind: ConstantToken, Number: value})
			continue
		}

		kind, found := map[byte]TokenKind{
			'(': OpenParenthesis,
			')': CloseParenthesis,
			'{': OpenBrace,
			'}': CloseBrace,
			';': Semicolon,
		}[input[index]]
		if !found {
			panic(fmt.Sprintf("unexpected character: %q", input[index]))
		}
		tokens = append(tokens, Token{Kind: kind})
		index++
	}

	return tokens
}

func isIdentifierStart(character byte) bool {
	return character >= 'a' && character <= 'z' || character >= 'A' && character <= 'Z' || character == '_'
}

func isIdentifierPart(character byte) bool {
	return isIdentifierStart(character) || isDigit(character)
}

func isDigit(character byte) bool {
	return character >= '0' && character <= '9'
}

type Parser struct {
	tokens  []Token
	current int
}

func (parser *Parser) parseProgram() Program {
	program := Program{Function: parser.parseFunction()}
	if parser.current != len(parser.tokens) {
		panic("unexpected trailing token")
	}
	return program
}

func (parser *Parser) parseFunction() Function {
	parser.expect(IntKeyword)
	identifier := parser.parseIdentifier()
	parser.expect(OpenParenthesis)
	parser.expect(VoidKeyword)
	parser.expect(CloseParenthesis)
	parser.expect(OpenBrace)
	statement := parser.parseStatement()
	parser.expect(CloseBrace)
	return Function{Identifier: identifier, Statement: statement}
}

func (parser *Parser) parseStatement() Return {
	parser.expect(ReturnKeyword)
	expression := parser.parseExpression()
	parser.expect(Semicolon)
	return Return{Expression: expression}
}

func (parser *Parser) parseExpression() Constant {
	return Constant{Value: parser.parseInteger()}
}

func (parser *Parser) parseIdentifier() string {
	token := parser.take()
	if token.Kind != Identifier {
		panic("expected identifier")
	}
	return token.Text
}

func (parser *Parser) parseInteger() int {
	token := parser.take()
	if token.Kind != ConstantToken {
		panic("expected integer")
	}
	return token.Number
}

func (parser *Parser) expect(expected TokenKind) {
	if parser.take().Kind != expected {
		panic("unexpected token")
	}
}

func (parser *Parser) take() Token {
	if parser.current == len(parser.tokens) {
		panic("unexpected end of input")
	}
	token := parser.tokens[parser.current]
	parser.current++
	return token
}
