namespace CSLox;

public abstract record StatementSyntax(ExpressionSyntax expression)
{
    public abstract object? Accept(IStatementVisitor visitor);
}

public record PrintStatementSyntax(ExpressionSyntax expression) : StatementSyntax(expression)
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitPrintStatementSyntax(this);
}

public record ExpressionStatementSyntax(ExpressionSyntax expression) : StatementSyntax(expression)
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitExpressionStatementSyntax(this);
}

public record VariableDeclarationStatementSyntax(Token name, ExpressionSyntax expression) : StatementSyntax(expression)
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitVariableDeclarationStatementSyntax(this);
}

public interface IStatementVisitor
{
    object? VisitExpressionStatementSyntax(ExpressionStatementSyntax expressionStatement);
    object? VisitPrintStatementSyntax(PrintStatementSyntax printStatement);
    object? VisitVariableDeclarationStatementSyntax(VariableDeclarationStatementSyntax variableDeclarationStatement);
}