using System.Data;
using System.Diagnostics;
using System.Text;

namespace CSLox;

internal class Interpreter : IExpressionVisitor, IStatementVisitor
{
    public string Interpret(List<StatementSyntax> statements)
    {
        try
        {
            var builder = new StringBuilder();
            foreach (var statement in statements)
            {
                var value = Execute(statement);
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

    public object? VisitLiteralSyntax(LiteralSyntax literalSyntax) => literalSyntax.literalValue;

    public object? VisitGroupingSyntax(GroupingSyntax groupingSyntax) => Evaluate(groupingSyntax);
    
    public object? VisitUnarySyntax(UnarySyntax unarySyntax)
    {
        var rightObject = Evaluate(unarySyntax.rightExpression);

        return unarySyntax.operatorToken.type switch
        {
            TokenType.BANG => !IsTruthy(rightObject),
            TokenType.MINUS when rightObject is not double => throw new RuntimeError(unarySyntax.operatorToken, "Operand must be a number."),
            TokenType.MINUS => -(double)rightObject,
            _ => null
        };
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

    public object? VisitBinarySyntax(BinarySyntax binarySyntax)
    {
        var leftObject = Evaluate(binarySyntax.leftExpression);
        var rightObject = Evaluate(binarySyntax.rightExpression);

        return binarySyntax.operatorToken.type switch
        {
            TokenType.BANG_EQUAL => !IsEqual(leftObject, rightObject),
            TokenType.EQUAL_EQUAL => IsEqual(leftObject, rightObject),
            TokenType.GREATER when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binarySyntax),
            TokenType.GREATER => (double)leftObject > (double)rightObject,
            TokenType.GREATER_EQUAL when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binarySyntax),
            TokenType.GREATER_EQUAL => (double)leftObject >= (double)rightObject,
            TokenType.LESS when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binarySyntax),
            TokenType.LESS => (double)leftObject < (double)rightObject,
            TokenType.LESS_EQUAL when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binarySyntax),
            TokenType.LESS_EQUAL => (double)leftObject <= (double)rightObject,
            TokenType.MINUS when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binarySyntax),
            TokenType.MINUS => (double)leftObject - (double)rightObject,
            TokenType.PLUS when (leftObject is double leftDouble) && (rightObject is double rightDouble) => leftDouble + rightDouble,
            TokenType.PLUS when (leftObject is string leftString) && (rightObject is string rightString) => leftString + rightString,
            TokenType.PLUS => throw new RuntimeError(binarySyntax.operatorToken, "Operands must be two numbers or strings"),
            TokenType.SLASH when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binarySyntax),
            TokenType.SLASH => (double)leftObject / (double)rightObject,
            TokenType.STAR when leftObject is not double || rightObject is not double => throw GenerateInvalidBinaryOperandsError(binarySyntax),
            TokenType.STAR => (double)leftObject * (double)rightObject,
            
            _ => throw new InvalidOperationException()
        };
    }

    static RuntimeError GenerateInvalidBinaryOperandsError(BinarySyntax binarySyntax) => new(binarySyntax.operatorToken, "Operands must be numbers");

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
        return value;
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