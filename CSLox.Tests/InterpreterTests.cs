using Xunit;

namespace CSLox.Tests;

public class InterpreterTests
{
    public class TestcConsoleWriter : IConsoleWriter
    {
        public string _output = string.Empty;

        public void ConsoleWriteLine(string output)
        {
            _output += output + System.Environment.NewLine;   
        }
    }
    
    string Interpret(string source)
    {
        var testConsoleWriter = new TestcConsoleWriter();
        CSLox.s_consoleWriter = testConsoleWriter;

        var scanner = new Scanner(source);
        var tokens = scanner.scanTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();
        var interpreter = new Interpreter();
        
        interpreter.Interpret(statements);
        return testConsoleWriter._output;
    }

    [Fact]
    public void TestInterpretBasicMultiplation()
    {
        var output = Interpret("print -10 * 2;");
        Assert.Equal($"-20{System.Environment.NewLine}", output);
    }
    
    [Fact]
    public void TestInterpretGroupingOfAssigments()
    {
        var output = Interpret(
@"var a = ""global a"";
var b = ""global b"";
var c = ""global c"";
{
    var a = ""outer a"";
    var b = ""outer b"";
    {
        var a = ""inner a"";
        print a;
        print b;
        print c;
    }
    print a;
    print b;
    print c;
}
print a;
print b;
print c;""");
        Assert.Equal(
@"inner a
outer b
global c
outer a
outer b
global c
global a
global b
global c" + System.Environment.NewLine, 
            output);
    }
    
    [Fact]
    public void TestBasicBranching()
    {
        var output = Interpret(
@"var yes = true;
var no = false;
if (yes)
    print ""yes is true"";
else
    print ""no is true"";
if (no)
    print ""else no is true"";
else
    print ""else no is not true"";");
        Assert.Equal(
@$"yes is true
else no is not true" + System.Environment.NewLine, output);
    }
    
    [Fact]
    public void TestBranchingWithLogicalOperators()
    {
        var output = Interpret(
@"var yes = true;
var no = false;
if (yes and no)
    print ""yes and no are true"";
else
    print ""yes and no are not true"";
if (yes or no)
    print ""yes or no are true"";
else
    print ""yes or no are not true"";");
        Assert.Equal(
@$"yes and no are not true
yes or no are true" + System.Environment.NewLine, output);
    }
    
    [Fact]
    public void TestWhileLoop()
    {
        var output = Interpret(
@"var i = 0;
var x = 0;
while (i < 10)
{
    x = x + 1;
    i = i + 1;
}
print x;");
        Assert.Equal(
            @$"10" + System.Environment.NewLine, output);
    }
}