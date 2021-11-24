namespace CSLox;

public abstract record ExpressionSyntax()
{
    public abstract object? Accept(IExpressionVisitor visitor);
}

public record BinarySyntax(ExpressionSyntax leftExpression, Token operatorToken, ExpressionSyntax rightExpression) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitBinarySyntax(this);
}

public record GroupingSyntax(ExpressionSyntax expression) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitGroupingSyntax(this);
}

public record LiteralSyntax(object? literalValue) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitLiteralSyntax(this);
}

public record UnarySyntax(Token operatorToken, ExpressionSyntax rightExpression) : ExpressionSyntax()
{
    public override object? Accept(IExpressionVisitor visitor) => visitor.VisitUnarySyntax(this);
}

public interface IExpressionVisitor
{
    object? VisitBinarySyntax(BinarySyntax binarySyntax);
    object? VisitGroupingSyntax(GroupingSyntax groupingSyntax);
    object? VisitLiteralSyntax(LiteralSyntax literalSyntax);
    object? VisitUnarySyntax(UnarySyntax unarySyntax);
}