namespace CSLox;

public abstract record ExpressionSyntax()
{
    public abstract string Accept(IExpressionVisitor visitor);
}

public record BinarySyntax(ExpressionSyntax leftExpression, Token operatorToken, ExpressionSyntax rightExpression) : ExpressionSyntax()
{
    public override string Accept(IExpressionVisitor visitor) => visitor.VisitBinarySyntax(this);
}

public record GroupingSyntax(ExpressionSyntax expression) : ExpressionSyntax()
{
    public override string Accept(IExpressionVisitor visitor) => visitor.VisitGroupingSyntax(this);
}

public record LiteralSyntax(object? literalValue) : ExpressionSyntax()
{
    public override string Accept(IExpressionVisitor visitor) => visitor.VisitLiteralSyntax(this);
}

public record UnarySyntax(Token operatorToken, ExpressionSyntax rightExpression) : ExpressionSyntax()
{
    public override string Accept(IExpressionVisitor visitor) => visitor.VisitUnarySyntax(this);
}

public interface IExpressionVisitor
{
    string VisitBinarySyntax(BinarySyntax binarySyntax);
    string VisitGroupingSyntax(GroupingSyntax groupingSyntax);
    string VisitLiteralSyntax(LiteralSyntax literalSyntax);
    string VisitUnarySyntax(UnarySyntax unarySyntax);
}