using System.Data;
using System.Diagnostics;

namespace CSLox;

internal class Interpreter : IExpressionVisitor
{
    public object? VisitLiteralSyntax(LiteralSyntax literalSyntax) => literalSyntax.literalValue;

    public object? VisitGroupingSyntax(GroupingSyntax groupingSyntax) => Evaluate(groupingSyntax);
    
    public object? VisitUnarySyntax(UnarySyntax unarySyntax)
    {
        var rightExpression = Evaluate(unarySyntax.rightExpression);
        if (rightExpression == null)
            throw new InvalidOperationException();
        
        return unarySyntax.operatorToken.type switch
        {
            TokenType.BANG => !IsTruthy(rightExpression),
            TokenType.MINUS => -(double)rightExpression,
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
        var leftExpression = Evaluate(binarySyntax.leftExpression);
        var rightExpression = Evaluate(binarySyntax.rightExpression);
        
        // !!! Need to handle possible null values for non-equals operators
        return binarySyntax.operatorToken.type switch
        {
            TokenType.BANG_EQUAL => !IsEqual(leftExpression, rightExpression),
            TokenType.EQUAL_EQUAL => IsEqual(leftExpression, rightExpression),
            TokenType.GREATER => (double)leftExpression > (double)rightExpression,
            TokenType.GREATER_EQUAL => (double)leftExpression >= (double)rightExpression,
            TokenType.LESS => (double)leftExpression < (double)rightExpression,
            TokenType.LESS_EQUAL => (double)leftExpression <= (double)rightExpression,
            TokenType.MINUS => (double)leftExpression - (double)rightExpression,
            TokenType.PLUS =>
                (leftExpression is double leftDouble) && (rightExpression is double rightDouble) ? leftDouble + rightDouble :
                (leftExpression is string leftString) && (rightExpression is string rightString) ? leftString + rightString :
                throw new InvalidOperationException(),
            TokenType.SLASH => (double)leftExpression / (double)rightExpression,
            TokenType.STAR => (double)leftExpression * (double)rightExpression,
            _ => throw new InvalidOperationException()
        };
    }

    bool IsEqual(object? leftObject, object? rightObject)
    {
        if (leftObject == null && rightObject == null)
            return true;
        if (leftObject == null) return false;
        return leftObject.Equals(rightObject);
    }

    object? Evaluate(ExpressionSyntax expressionSyntax)
    {
        throw new NotImplementedException();
    }
}