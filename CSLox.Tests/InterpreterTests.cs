using Xunit;

namespace CSLox.Tests;

public class InterpreterTests
{
    static string NL => System.Environment.NewLine;

    public class TestcConsoleWriter : IConsoleWriter
    {
        public string _output = string.Empty;

        public void ConsoleWriteLine(string output)
        {
            _output += output + NL;   
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
        var resolver = new Resolver(interpreter);
        
        resolver.Resolve(statements);
        interpreter.Interpret(statements);
        return testConsoleWriter._output;
    }

    [Fact]
    public void TestInterpretBasicMultiplation()
    {
        var output = Interpret("print -10 * 2;");
        Assert.Equal($"-20{NL}", output);
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
global c" + NL, 
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
else no is not true" + NL, output);
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
yes or no are true" + NL, output);
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
        Assert.Equal(@$"10" + NL, output);
    }
    
    [Fact]
    public void TestForLoop()
    {
        var output = Interpret(
@"var x = 0;
for (var i = 0; i < 10; i = i + 1)
{
    x = x + 1;
}
print x;");
        Assert.Equal(@$"10" + NL, output);
    }
    
    [Fact]
    public void TestBasicFunctionInvocation()
    {
        var output = Interpret(
@"fun sayHi(first, last)
{ 
    print ""Hi "" + first + "" "" + last;
}
sayHi(""L33t"", ""Hax0r"");");
        Assert.Equal(@$"Hi L33t Hax0r" + NL, output);
    }
    
    [Fact]
    public void TestFunctionReturningValue()
    {
        var output = Interpret(
@"fun someValue()
{ 
    return 3;
}
print someValue();");
        Assert.Equal(@$"3" + NL, output);
    }
    
    [Fact]
    public void TestFunctionWithBasicRecursion()
    {
        var output = Interpret(
@"fun someValue(x)
{
    if (x > 3)
        return 0;
    else
        return x + someValue(x + 1);
}
print someValue(0);");
        Assert.Equal(@$"6" + NL, output);
    }
    
    [Fact]
    public void TestFunctionWithClosure()
    {
        var output = Interpret(
@"fun makeCounter()
{
    var i = 0;
    fun count()
    {
        i = i + 1;
        print i;
    }
    return count;
}
var counter = makeCounter();
counter();
counter();");
        Assert.Equal(@$"1{NL}2{NL}", output);
    }
    
    [Fact]
    public void TestCorrectVariableBindingInScopes()
    {
        var output = Interpret(
@"var a = ""global"";
{
    fun showA()
    {
        print a;
    }
    showA();
    var a = ""block"";
    showA();
}");
        Assert.Equal(@$"global{NL}global{NL}", output);
    }
}