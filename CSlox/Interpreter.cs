using System.Data;
using System.Text;

namespace CSLox;

class Interpreter : IExpressionVisitor, IStatementVisitor
{
    readonly Environment _globals = new();
    Environment _environment;
    Dictionary<ExpressionSyntax, int> _localsDistance = new();
    
    public Interpreter()
    {
        _environment = _globals;
        _globals.Define("clock", new ClockLoxCallable());
    }
    
    public string Interpret(List<StatementSyntax> statements)
    {
        try
        {
            var builder = new StringBuilder();
            foreach (var statement in statements)
            {
                var value = Execute(statement);
                if (value != null)
                    builder.Append(Stringify(value));
            }

            return builder.ToString();
        }
        catch (RuntimeError error)
        {
            CSLox.RuntimeError(error);
            return string.Empty;
        }
    }

    object? Execute(StatementSyntax statement) => statement.Accept(this);

    public object? VisitLiteralExpressionSyntax(LiteralExpressionSyntax literalExpressionSyntax) => literalExpressionSyntax.literalValue;

    public object? VisitGroupingExpressionSyntax(GroupingExpressionSyntax groupingExpressionSyntax) => Evaluate(groupingExpressionSyntax);
    
    public object? VisitUnaryExpressionSyntax(UnaryExpressionSyntax unaryExpressionSyntax)
    {
        var rightObject = Evaluate(unaryExpressionSyntax.rightExpression);

        return unaryExpressionSyntax.operatorToken.type switch
        {
            TokenType.BANG => !IsTruthy(rightObject),
            TokenType.MINUS when rightObject is not double => throw new RuntimeError(unaryExpressionSyntax.operatorToken, "Operand must be a number."),
            TokenType.MINUS => -(double)rightObject,
            _ => null
        };
    }

    public object? VisitVariableExpressionSyntax(VariableExpressionSyntax variableExpressionSyntax)
    {
        return LookupVariable(variableExpressionSyntax.name, variableExpressionSyntax);
    }

    object? LookupVariable(Token name, VariableExpressionSyntax expression)
    {
        if (_localsDistance.ContainsKey(expression))
            return _environment.GetAt(_localsDistance[expression], name.lexeme);
        
        return _globals.Get(expression.name);
    }

    public object? VisitAssignmentExpressionSyntax(AssignmentExpressionSyntax assignmentExpression)
    {
        var value = Evaluate(assignmentExpression.value);
        
        if (_localsDistance.ContainsKey(assignmentExpression))
            _environment.AssignAt(_localsDistance[assignmentExpression], assignmentExpression.name, value);
        else
            _globals.Assign(assignmentExpression.name, value);
        
        return value;
    }

    public object? VisitLogicalExpressionSyntax(LogicalExpressionSyntax logicalExpression)
    {
        var leftValue = Evaluate(logicalExpression.leftExpression);

        if (logicalExpression.operatorToken.type == TokenType.OR)
        {
            if (IsTruthy(leftValue))
                return leftValue;
        }
        else
        {
            if (!IsTruthy(leftValue))
                return leftValue;
        }

        return Evaluate(logicalExpression.rightExpression);
    }

    public object? VisitCallExpressionSyntax(CallExpressionSyntax callExpression)
    {
        var callee = Evaluate(callExpression.callee);
        var arguments = callExpression.arguments.Select(Evaluate);
        if (callee == null) throw new InvalidOperationException();
        var function = (ILoxCallable)callee;
        
        return function.Call(this, arguments.ToList());
    }

    bool IsTruthy(object? testObject)
    {
        return testObject switch
        {
            null => false,
            bool boolValue => boolValue, 
            _ => true
        };
    }

    public object? VisitBinaryExpressionSyntax(BinaryExpressionSyntax binaryExpressionSyntax)
    {
        var leftObject = Evaluate(binaryExpressionSyntax.leftExpression);
        var rightObject = Evaluate(binaryExpressionSyntax.rightExpression);

        return binaryExpressionSyntax.operatorToken.type switch
        {
            TokenType.BANG_EQUAL => !IsEqual(leftObject, rightObject),
            TokenType.EQUAL_EQUAL => IsEqual(leftObject, rightObject),
            TokenType.GREATER when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binaryExpressionSyntax),
            TokenType.GREATER => (double)leftObject > (double)rightObject,
            TokenType.GREATER_EQUAL when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binaryExpressionSyntax),
            TokenType.GREATER_EQUAL => (double)leftObject >= (double)rightObject,
            TokenType.LESS when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binaryExpressionSyntax),
            TokenType.LESS => (double)leftObject < (double)rightObject,
            TokenType.LESS_EQUAL when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binaryExpressionSyntax),
            TokenType.LESS_EQUAL => (double)leftObject <= (double)rightObject,
            TokenType.MINUS when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binaryExpressionSyntax),
            TokenType.MINUS => (double)leftObject - (double)rightObject,
            TokenType.PLUS when (leftObject is double leftDouble) && (rightObject is double rightDouble) => leftDouble + rightDouble,
            TokenType.PLUS when (leftObject is string leftString) && (rightObject is string rightString) => leftString + rightString,
            TokenType.PLUS => throw new RuntimeError(binaryExpressionSyntax.operatorToken, "Operands must be two numbers or strings"),
            TokenType.SLASH when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binaryExpressionSyntax),
            TokenType.SLASH => (double)leftObject / (double)rightObject,
            TokenType.STAR when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binaryExpressionSyntax),
            TokenType.STAR => (double)leftObject * (double)rightObject,
            
            _ => throw new InvalidOperationException()
        };
    }

    static RuntimeError GenerateInvalidBinaryOperandsError(BinaryExpressionSyntax binaryExpressionSyntax) => new(binaryExpressionSyntax.operatorToken, "Operands must be numbers");

    bool IsEqual(object? leftObject, object? rightObject)
    {
        if (leftObject == null && rightObject == null)
            return true;
        if (leftObject == null) return false;
        return leftObject.Equals(rightObject);
    }

    object? Evaluate(ExpressionSyntax expressionSyntax) => expressionSyntax.Accept(this);
    
    public object? VisitExpressionStatementSyntax(ExpressionStatementSyntax expressionStatement)
    {
        Evaluate(expressionStatement.expression);
        return null;
    }

    public object? VisitPrintStatementSyntax(PrintStatementSyntax printStatement)
    {
        var value = Evaluate(printStatement.expression);
        CSLox.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitVariableDeclarationStatementSyntax(VariableDeclarationStatementSyntax variableDeclarationStatement)
    {
        object? value = null;
        if (variableDeclarationStatement.initializer != null)
            value = Evaluate(variableDeclarationStatement.initializer);
        
        _environment.Define(variableDeclarationStatement.name.lexeme, value);
        return null;
    }

    public object? VisitBlockStatementSyntax(BlockStatementSyntax blockStatement)
    {
        ExecuteBlock(blockStatement.statements, new Environment(_environment));
        return null;
    }

    public object? VisitIfStatementSyntax(IfStatementSyntax ifStatement)
    {
        if (IsTruthy(Evaluate(ifStatement.conditionExpression)))
            Execute(ifStatement.thenBranchStatement);
        else if (ifStatement.elseBranchStatement != null)
            Execute(ifStatement.elseBranchStatement);
        return null;
    }

    public object? VisitWhileStatementSyntax(WhileStatementSyntax whileStatement)
    {
        while (IsTruthy(Evaluate(whileStatement.conditionExpression)))
            Execute(whileStatement.bodyStatement);
        return null;
    }

    public object? VisitFunctionDeclarationStatementSyntax(FunctionDeclarationStatementSyntax functionDeclarationStatement)
    {
        var function = new LoxFunction(functionDeclarationStatement, _environment);
        _environment.Define(functionDeclarationStatement.name.lexeme, function);
        return null;
    }

    public object? VisitReturnStatementSyntax(ReturnStatementSyntax returnStatement)
    {
        object? value = null;
        if (returnStatement.valueExpression != null)
            value = Evaluate(returnStatement.valueExpression);
        
        throw new CallReturnException(value);
    }

    internal void ExecuteBlock(List<StatementSyntax> blockStatements, Environment environment)
    {
        var previousEnvironment = _environment;
        try
        {
            _environment = environment;
            foreach (var statement in blockStatements)
                Execute(statement);
        }
        finally
        {
            _environment = previousEnvironment;
        }
    }

    string Stringify(object? obj)
    {
        if (obj == null)
            return "nil";
        if (obj is double)
        {
            var text = obj.ToString() ?? "";
            return text.EndsWith(".0") ? text[..^2] : text;
        }

        return obj.ToString() ?? "";
    }

    public void Resolve(ExpressionSyntax variableExpression, int depth)
    {
        if (_localsDistance.ContainsKey(variableExpression))
            throw new InvalidOperationException();
        _localsDistance[variableExpression] = depth;
    }
}

class CallReturnException : Exception
{
    internal readonly object? _returnValue;
    
    public CallReturnException(object? returnValue) => _returnValue = returnValue;
}

public class RuntimeError : Exception
{
    internal Token _token;
    
    public RuntimeError(Token token, string message) : base(message) => _token = token;
}