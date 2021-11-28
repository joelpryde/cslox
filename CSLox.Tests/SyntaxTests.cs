using Xunit;

namespace CSLox.Tests;

public class SyntaxTests
{
    [Fact]
    public void TestSyntaxConstruction()
    {
        var newExpr = new BinaryExpressionSyntax(
               new UnaryExpressionSyntax(
                   new Token(TokenType.MINUS, "-", null, 1, 4, 5),
                   new LiteralExpressionSyntax(123)),
               new Token(TokenType.STAR, "*", null, 1, 1, 2),
               new GroupingExpressionSyntax(new LiteralExpressionSyntax(45.67)));
        
        Assert.Equal("(* (- 123) (group 45.67))", new ASTPrinter().Print(newExpr));
    }
}