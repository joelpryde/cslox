using System.Runtime.InteropServices;

static class CSLox
{
    static bool _hadError;

    static void Main(string[] args)
    {
        if (args.Length > 1)
            Console.WriteLine("Usage: cslox [script]");
        else if (args.Length == 1)
            RunFile(args[0]);
        else
            RunPrompt();

        /*
        var newExpr = new BinarySyntax(
            new UnarySyntax(
                new Token(TokenType.MINUS, "-", null, 1),
                new LiteralSyntax(123)),
            new Token(TokenType.STAR, "*", null, 1),
            new GroupingSyntax(new LiteralSyntax(45.67)));
        Console.WriteLine(new ASTPrinter().Print(newExpr));
        */
    }
    
    static void RunFile(string s)
    {
        var fileContents = File.ReadAllText(s);
        Run(fileContents);
        if (_hadError)
            Environment.Exit(65);
    }
    
    static void RunPrompt()
    {
        string? line;
        Console.Write(">");;
        while ((line = Console.ReadLine()) != null)
        {
            Run(line);
            _hadError = false;
            Console.Write(">");
        }
    }

    static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.scanTokens();
        var parser = new Parser(tokens);
        var expression = parser.Parse();

        if (_hadError || expression == null)
            return;
        
        Console.WriteLine(new ASTPrinter().Print(expression));
    }

    internal static void Error(int line, string message) => Report(line, "", message);
    internal static void Error(Token token, string message) => 
        Report(token.line, token.type == TokenType.EOF ? " at end" : $" at '{token.lexeme}`", message);

    static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where} : {message}");
        _hadError = true;
    }
}