using System.Text;
namespace CSLox;

public class ASTPrinter : IExpressionVisitor
{
    public string Print(ExpressionSyntax expression) => expression.Accept(this)?.ToString() ?? string.Empty;

    public object VisitBinarySyntax(BinarySyntax binarySyntax) => 
        parenthesize(binarySyntax.operatorToken.lexeme, binarySyntax.leftExpression, binarySyntax.rightExpression);
    
    public object VisitGroupingSyntax(GroupingSyntax groupingSyntax) => 
        parenthesize("group", groupingSyntax.expression);

    public object VisitLiteralSyntax(LiteralSyntax literalSyntax) => literalSyntax.literalValue?.ToString() ?? "nil";

    public object VisitUnarySyntax(UnarySyntax unarySyntax) => 
        parenthesize(unarySyntax.operatorToken.lexeme, unarySyntax.rightExpression);

    public object? VisitVariableSyntax(VariableSyntax variableSyntax) => variableSyntax.name;

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