const std = @import("std");

const Token = union(enum) {
    identifier: []const u8,
    int_keyword,
    void_keyword,
    return_keyword,
    constant: u32,
    open_parenthesis,
    close_parenthesis,
    open_brace,
    close_brace,
    semicolon,
};

const Tokens = struct {
    // ponytail: fixed capacity keeps the example allocation-free; use ArrayList for larger inputs.
    items: [32]Token = undefined,
    len: usize = 0,

    fn append(self: *Tokens, token: Token) !void {
        if (self.len == self.items.len)
            return error.TooManyTokens;

        self.items[self.len] = token;
        self.len += 1;
    }
};

const Program = struct { function: Function };
const Function = struct { identifier: []const u8, statement: Return };
const Return = struct { expression: Constant };
const Constant = struct { value: u32 };

pub fn main() !void {
    const input = "int main(void) { return 2; }";
    var tokens = try lex(input);
    var parser = Parser{ .tokens = tokens.items[0..tokens.len] };
    const program = try parser.parseProgram();

    std.debug.assert(std.mem.eql(u8, program.function.identifier, "main"));
    std.debug.assert(program.function.statement.expression.value == 2);
    std.debug.print(
        "Program {{ Function = Function {{ Identifier = {s}, Statement = Return {{ Expression = Constant {{ Value = {} }} }} }} }}\n",
        .{ program.function.identifier, program.function.statement.expression.value },
    );
}

fn lex(input: []const u8) !Tokens {
    var tokens = Tokens{};
    var index: usize = 0;

    while (index < input.len) {
        if (std.ascii.isWhitespace(input[index])) {
            index += 1;
            continue;
        }

        if (std.ascii.isAlphabetic(input[index]) or input[index] == '_') {
            const start = index;
            index += 1;
            while (index < input.len and
                (std.ascii.isAlphanumeric(input[index]) or input[index] == '_'))
            {
                index += 1;
            }

            const word = input[start..index];
            const token: Token = if (std.mem.eql(u8, word, "int"))
                .int_keyword
            else if (std.mem.eql(u8, word, "void"))
                .void_keyword
            else if (std.mem.eql(u8, word, "return"))
                .return_keyword
            else
                .{ .identifier = word };
            try tokens.append(token);
            continue;
        }

        if (std.ascii.isDigit(input[index])) {
            const start = index;
            while (index < input.len and std.ascii.isDigit(input[index]))
                index += 1;

            try tokens.append(.{
                .constant = try std.fmt.parseInt(u32, input[start..index], 10),
            });
            continue;
        }

        const token: Token = switch (input[index]) {
            '(' => .open_parenthesis,
            ')' => .close_parenthesis,
            '{' => .open_brace,
            '}' => .close_brace,
            ';' => .semicolon,
            else => return error.UnexpectedCharacter,
        };
        try tokens.append(token);
        index += 1;
    }

    return tokens;
}

const Parser = struct {
    tokens: []const Token,
    current: usize = 0,

    fn parseProgram(self: *Parser) !Program {
        const program = Program{ .function = try self.parseFunction() };
        if (self.current != self.tokens.len)
            return error.UnexpectedToken;
        return program;
    }

    fn parseFunction(self: *Parser) !Function {
        try self.expect(.int_keyword);
        const identifier = try self.parseIdentifier();
        try self.expect(.open_parenthesis);
        try self.expect(.void_keyword);
        try self.expect(.close_parenthesis);
        try self.expect(.open_brace);
        const statement = try self.parseStatement();
        try self.expect(.close_brace);

        return .{ .identifier = identifier, .statement = statement };
    }

    fn parseStatement(self: *Parser) !Return {
        try self.expect(.return_keyword);
        const expression = try self.parseExpression();
        try self.expect(.semicolon);
        return .{ .expression = expression };
    }

    fn parseExpression(self: *Parser) !Constant {
        return .{ .value = try self.parseInteger() };
    }

    fn parseIdentifier(self: *Parser) ![]const u8 {
        return switch (try self.take()) {
            .identifier => |identifier| identifier,
            else => error.UnexpectedToken,
        };
    }

    fn parseInteger(self: *Parser) !u32 {
        return switch (try self.take()) {
            .constant => |constant| constant,
            else => error.UnexpectedToken,
        };
    }

    fn expect(self: *Parser, expected: std.meta.Tag(Token)) !void {
        if (std.meta.activeTag(try self.take()) != expected)
            return error.UnexpectedToken;
    }

    fn take(self: *Parser) !Token {
        if (self.current == self.tokens.len)
            return error.EndOfInput;

        defer self.current += 1;
        return self.tokens[self.current];
    }
};
