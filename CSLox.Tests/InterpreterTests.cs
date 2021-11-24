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
        Assert.NotNull(output);
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
        Assert.NotNull(output);
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
        Assert.NotNull(output);
        Assert.Equal(
@$"yes is true
else no is not true" + System.Environment.NewLine, output);
    }
}