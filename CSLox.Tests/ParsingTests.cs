using System.Linq;
using Xunit;

namespace CSLox.Tests;

public class ParsingTests
{
    [Fact]
    public void TestInputParsingOfBasicExpression()
    {
        var scanner = new Scanner("-123 * 45.67;");
        var tokens = scanner.scanTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();
        
        Assert.NotEmpty(statements);
        Assert.Equal("(* (- 123) 45.67)", new ASTPrinter().Print(statements.First().expression));
    }
}