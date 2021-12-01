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

public record WhileStatementSyntax(ExpressionSyntax conditionExpression, StatementSyntax bodyStatement) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitWhileStatementSyntax(this);
}

public record FunctionDeclarationStatementSyntax(Token name, List<Token> parameters, List<StatementSyntax> bodyStatements) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitFunctionDeclarationStatementSyntax(this);
}

public record ReturnStatementSyntax(Token keywordToken, ExpressionSyntax? valueExpression) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitReturnStatementSyntax(this);
}

public record ClassStatementSyntax(Token name, VariableExpressionSyntax? superClass, List<FunctionDeclarationStatementSyntax> methods) : StatementSyntax()
{
    public override object? Accept(IStatementVisitor visitor) => visitor.VisitClassStatementSyntax(this);
}

public interface IStatementVisitor
{
    object? VisitExpressionStatementSyntax(ExpressionStatementSyntax expressionStatement);
    object? VisitPrintStatementSyntax(PrintStatementSyntax printStatement);
    object? VisitVariableDeclarationStatementSyntax(VariableDeclarationStatementSyntax variableDeclarationStatement);
    object? VisitBlockStatementSyntax(BlockStatementSyntax blockStatement);
    object? VisitIfStatementSyntax(IfStatementSyntax ifStatement);
    object? VisitWhileStatementSyntax(WhileStatementSyntax whileStatementSyntax);
    object? VisitFunctionDeclarationStatementSyntax(FunctionDeclarationStatementSyntax functionDeclarationStatement);
    object? VisitReturnStatementSyntax(ReturnStatementSyntax returnStatement);
    object? VisitClassStatementSyntax(ClassStatementSyntax classStatement);
}