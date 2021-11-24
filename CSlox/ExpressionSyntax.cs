namespace CSLox;

public abstract record ExpressionSyntax()
{
    public abstract object? Accept(IExpressionVisitor visitor);
}

public record BinaryExpressionSyntax(ExpressionSyntax leftExpression, Token operatorToken, ExpressionSyntax rightExpression) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitBinaryExpressionSyntax(this);
}

public record GroupingExpressionSyntax(ExpressionSyntax expression) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitGroupingExpressionSyntax(this);
}

public record LiteralExpressionSyntax(object? literalValue) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitLiteralExpressionSyntax(this);
}

public record UnaryExpressionSyntax(Token operatorToken, ExpressionSyntax rightExpression) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitUnaryExpressionSyntax(this);
}

public record VariableExpressionSyntax(Token name) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitVariableExpressionSyntax(this);
}

public interface IExpressionVisitor
{
    object? VisitBinaryExpressionSyntax(BinaryExpressionSyntax binaryExpressionSyntax);
    object? VisitGroupingExpressionSyntax(GroupingExpressionSyntax groupingExpressionSyntax);
    object? VisitLiteralExpressionSyntax(LiteralExpressionSyntax literalExpressionSyntax);
    object? VisitUnaryExpressionSyntax(UnaryExpressionSyntax unaryExpressionSyntax);
    object? VisitVariableExpressionSyntax(VariableExpressionSyntax variableExpressionSyntax);
}