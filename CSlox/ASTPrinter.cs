using System.Text;
namespace CSLox;

public class ASTPrinter : IExpressionVisitor
{
    public string Print(ExpressionSyntax expression) => expression.Accept(this)?.ToString() ?? string.Empty;

    public object VisitBinaryExpressionSyntax(BinaryExpressionSyntax binaryExpressionSyntax) => 
        parenthesize(binaryExpressionSyntax.operatorToken.lexeme, binaryExpressionSyntax.leftExpression, binaryExpressionSyntax.rightExpression);
    
    public object VisitGroupingExpressionSyntax(GroupingExpressionSyntax groupingExpressionSyntax) => 
        parenthesize("group", groupingExpressionSyntax.expression);

    public object VisitLiteralExpressionSyntax(LiteralExpressionSyntax literalExpressionSyntax) => literalExpressionSyntax.literalValue?.ToString() ?? "nil";

    public object VisitUnaryExpressionSyntax(UnaryExpressionSyntax unaryExpressionSyntax) => 
        parenthesize(unaryExpressionSyntax.operatorToken.lexeme, unaryExpressionSyntax.rightExpression);

    public object? VisitVariableExpressionSyntax(VariableExpressionSyntax variableExpressionSyntax) => variableExpressionSyntax.name;
    
    public object? VisitAssignmentExpressionSyntax(AssignmentExpressionSyntax assignmentExpression) => throw new NotImplementedException();
    public object? VisitLogicalExpressionSyntax(LogicalExpressionSyntax logicalExpression) => throw new NotImplementedException();
    public object? VisitCallExpressionSyntax(CallExpressionSyntax callExpression) => throw new NotImplementedException();
    public object? VisitGetExpressionSyntax(GetExpressionSyntax getExpression) => throw new NotImplementedException();
    public object? VisitSetExpressionSyntax(SetExpressionSyntax setExpression) => throw new NotImplementedException();
    public object? VisitThisExpressionSyntax(ThisExpressionSyntax thisExpression) => throw new NotImplementedException();

    string parenthesize(string name, params ExpressionSyntax[] expressionSyntaxes)
    {
        var builder = new StringBuilder();
        
        builder.Append($"({name}");
        foreach (var expression in expressionSyntaxes)
            builder.Append($" {expression.Accept(this)}");
        builder.Append(')');
        
        return builder.ToString();
    }
}