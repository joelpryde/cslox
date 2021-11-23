using Xunit;

namespace CSLox.Tests;

public class SyntaxTests
{
    [Fact]
    public void TestSyntaxConstruction()
    {
        var newExpr = new BinarySyntax(
               new UnarySyntax(
                   new Token(TokenType.MINUS, "-", null, 1),
                   new LiteralSyntax(123)),
               new Token(TokenType.STAR, "*", null, 1),
               new GroupingSyntax(new LiteralSyntax(45.67)));
        
        Assert.Equal("(* (- 123) (group 45.67))", new ASTPrinter().Print(newExpr));
    }
}