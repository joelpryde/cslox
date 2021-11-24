using System.Data;
using System.Diagnostics;
using System.Text;

namespace CSLox;

class Interpreter : IExpressionVisitor, IStatementVisitor
{
    Environment _environment = new();
    
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

    public object? VisitVariableExpressionSyntax(VariableExpressionSyntax variableExpressionSyntax) => _environment.Get(variableExpressionSyntax.name);
    public object? VisitAssignmentExpressionSyntax(AssignmentExpressionSyntax assignmentSyntaxSyntax)
    {
        var value = Evaluate(assignmentSyntaxSyntax.value);
        _environment.Assign(assignmentSyntaxSyntax.name, value);
        return value;
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
        Console.WriteLine(Stringify(value));
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

    void ExecuteBlock(List<StatementSyntax> blockStatements, Environment environment)
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
}

public class RuntimeError : Exception
{
    internal Token _token;
    
    public RuntimeError(Token token, string message) : base(message) => _token = token;
}