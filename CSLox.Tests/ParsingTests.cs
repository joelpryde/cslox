using Xunit;

namespace CSLox.Tests;

public class ParsingTests
{
    [Fact]
    public void TestInputParsingOfBasicExpression()
    {
        var scanner = new Scanner("-123 * 45.67");
        var tokens = scanner.scanTokens();
        var parser = new Parser(tokens);
        var expression = parser.Parse();
        
        Assert.NotNull(expression);
        if (expression != null)
            Assert.Equal("(* (- 123) 45.67)", new ASTPrinter().Print(expression));
    }
}