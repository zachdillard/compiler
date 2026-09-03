public abstract record Ast;

public record ProgramNode(FunctionNode Function) : Ast;

public record FunctionNode(string Identifier, StatementNode Statement) : Ast;

public abstract record StatementNode : Ast;

public record ReturnStatementNode(ExpressionNode Expression) : StatementNode;

public abstract record ExpressionNode : Ast;

public record ConstantExpressionNode(int Constant) : ExpressionNode;
