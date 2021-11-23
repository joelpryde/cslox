using System.Text;
namespace CSLox;

public class ASTPrinter : IExpressionVisitor
{
    public string Print(ExpressionSyntax expression) => expression.Accept(this);

    public string VisitBinarySyntax(BinarySyntax binarySyntax) => 
        parenthesize(binarySyntax.operatorToken.lexeme, binarySyntax.leftExpression, binarySyntax.rightExpression);
    
    public string VisitGroupingSyntax(GroupingSyntax groupingSyntax) => 
        parenthesize("group", groupingSyntax.expression);

    public string VisitLiteralSyntax(LiteralSyntax literalSyntax) => literalSyntax.literalValue?.ToString() ?? "nil";

    public string VisitUnarySyntax(UnarySyntax unarySyntax) => 
        parenthesize(unarySyntax.operatorToken.lexeme, unarySyntax.rightExpression);

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