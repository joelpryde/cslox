namespace CSLox;

public abstract record StatementSyntax()
{
    public abstract object? Accept(IStatementVisitor visitor);
}

public record PrintStatementSyntax(ExpressionSyntax expression) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitPrintStatementSyntax(this);
}

public record ExpressionStatementSyntax(ExpressionSyntax expression) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitExpressionStatementSyntax(this);
}

public record VariableDeclarationStatementSyntax(Token name, ExpressionSyntax? initializer) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitVariableDeclarationStatementSyntax(this);
}

public record IfStatementSyntax(ExpressionSyntax conditionExpression, StatementSyntax thenBranchStatement, StatementSyntax? elseBranchStatement) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitIfStatementSyntax(this);
}

public record BlockStatementSyntax(List<StatementSyntax> statements) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitBlockStatementSyntax(this);
}

public interface IStatementVisitor
{
    object? VisitExpressionStatementSyntax(ExpressionStatementSyntax expressionStatement);
    object? VisitPrintStatementSyntax(PrintStatementSyntax printStatement);
    object? VisitVariableDeclarationStatementSyntax(VariableDeclarationStatementSyntax variableDeclarationStatement);
    object? VisitBlockStatementSyntax(BlockStatementSyntax blockStatement);
    object? VisitIfStatementSyntax(IfStatementSyntax ifStatement);
}