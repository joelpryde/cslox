namespace CSLox;

enum FunctionType
{
    None,
    Function
}

class Resolver : IExpressionVisitor, IStatementVisitor
{
    readonly Interpreter _interpreter;
    readonly Stack<Dictionary<string, bool>> _scopes = new();
    FunctionType _currentFunctionType = FunctionType.None;

    public Resolver(Interpreter interpreter) => _interpreter = interpreter;

    public void Resolve(List<StatementSyntax> statements)
    {
        foreach (var statement in statements)
            Resolve(statement);
    }
    
    void Resolve(StatementSyntax statement) => statement.Accept(this);

    void Resolve(ExpressionSyntax expression) => expression.Accept(this);

    void ResolveLocal(ExpressionSyntax expression, Token name)
    {
        for (var i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes.ElementAt(_scopes.Count - i - 1).ContainsKey(name.lexeme))
            {
                _interpreter.Resolve(expression, _scopes.Count - 1 - i);
                return;
            }
        }
    }

    void ResolveFunction(FunctionDeclarationStatementSyntax functionDeclarationStatement, FunctionType functionType)
    {
        var enclosingFunctionType = _currentFunctionType;
        _currentFunctionType = functionType;
        
        BeginScope();
        foreach (var parameterToken in functionDeclarationStatement.parameters)
        {
            Declare(parameterToken);
            Define(parameterToken);
            
        }
        Resolve(functionDeclarationStatement.bodyStatements);
        EndScope();
        _currentFunctionType = enclosingFunctionType;
    }

    void BeginScope() => _scopes.Push(new Dictionary<string, bool>());

    void EndScope() => _scopes.Pop();

    void Declare(Token name)
    {
        if (_scopes.Count == 0)
            return;

        var scope = _scopes.Peek();
        if (scope.ContainsKey(name.lexeme))
            CSLox.Error(name, "Already a variable with this name in this scope.");
        
        scope[name.lexeme] = false;
    }

    void Define(Token name)
    { 
        if (_scopes.Count == 0)
            return;

        _scopes.Peek()[name.lexeme] = true;
    }

    public object? VisitBinaryExpressionSyntax(BinaryExpressionSyntax binaryExpressionSyntax)
    {
        Resolve(binaryExpressionSyntax.leftExpression);
        Resolve(binaryExpressionSyntax.rightExpression);

        return null;
    }

    public object? VisitGroupingExpressionSyntax(GroupingExpressionSyntax groupingExpressionSyntax)
    {
        Resolve(groupingExpressionSyntax.expression);
        
        return null;
    }

    public object? VisitLiteralExpressionSyntax(LiteralExpressionSyntax literalExpressionSyntax)
    {
        return null;
    }

    public object? VisitUnaryExpressionSyntax(UnaryExpressionSyntax unaryExpressionSyntax)
    {
        Resolve(unaryExpressionSyntax.rightExpression);
        
        return null;
    }

    public object? VisitVariableExpressionSyntax(VariableExpressionSyntax variableExpressionSyntax)
    {
        if (_scopes.Count != 0 && 
            _scopes.Peek().ContainsKey(variableExpressionSyntax.name.lexeme) &&
            !_scopes.Peek()[variableExpressionSyntax.name.lexeme])
            CSLox.Error(variableExpressionSyntax.name, "Can't read local variable in its own initializer.");
        ResolveLocal(variableExpressionSyntax, variableExpressionSyntax.name);
        
        return null;
    }

    public object? VisitAssignmentExpressionSyntax(AssignmentExpressionSyntax assignmentExpression)
    {
        Resolve(assignmentExpression.value);
        ResolveLocal(assignmentExpression, assignmentExpression.name);
        
        return null;
    }

    public object? VisitLogicalExpressionSyntax(LogicalExpressionSyntax logicalExpression)
    {
        Resolve(logicalExpression.leftExpression);
        Resolve(logicalExpression.rightExpression);

        return null;
    }

    public object? VisitCallExpressionSyntax(CallExpressionSyntax callExpression)
    {
        Resolve(callExpression.callee);
        foreach (var argument in callExpression.arguments)
            Resolve(argument);

        return null;
    }

    public object? VisitExpressionStatementSyntax(ExpressionStatementSyntax expressionStatement)
    {
        Resolve(expressionStatement.expression);
        
        return null;
    }

    public object? VisitPrintStatementSyntax(PrintStatementSyntax printStatement)
    {
        Resolve(printStatement.expression);
        
        return null;
    }

    public object? VisitVariableDeclarationStatementSyntax(VariableDeclarationStatementSyntax variableDeclarationStatement)
    {
        Declare(variableDeclarationStatement.name);
        if (variableDeclarationStatement.initializer != null)
            Resolve(variableDeclarationStatement.initializer);
        Define(variableDeclarationStatement.name);
        
        return null;
    }

    public object? VisitBlockStatementSyntax(BlockStatementSyntax blockStatement)
    {
        BeginScope();
        Resolve(blockStatement.statements);
        EndScope();
        
        return null;
    }

    public object? VisitIfStatementSyntax(IfStatementSyntax ifStatement)
    {
        Resolve(ifStatement.conditionExpression);
        Resolve(ifStatement.thenBranchStatement);
        if (ifStatement.elseBranchStatement != null)
            Resolve(ifStatement.elseBranchStatement);
        
        return null;
    }

    public object? VisitWhileStatementSyntax(WhileStatementSyntax whileStatementSyntax)
    {
        Resolve(whileStatementSyntax.conditionExpression);
        Resolve(whileStatementSyntax.bodyStatement);

        return null;
    }

    public object? VisitFunctionDeclarationStatementSyntax(FunctionDeclarationStatementSyntax functionDeclarationStatement)
    {
        Declare(functionDeclarationStatement.name);
        Define(functionDeclarationStatement.name);
        ResolveFunction(functionDeclarationStatement, FunctionType.Function);
        
        return null;
    }

    public object? VisitReturnStatementSyntax(ReturnStatementSyntax returnStatement)
    {
        if (_currentFunctionType == FunctionType.None)
            CSLox.Error(returnStatement.keywordToken, "Can't return from top-level code.");
            
        if (returnStatement.valueExpression != null)
            Resolve(returnStatement.valueExpression);
        
        return null;
    }
}