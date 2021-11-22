using System.Runtime.InteropServices;

static class CSLox
{
    static bool _hadError;

    static void Main(string[] args)
    {
        /*
        if (args.Length > 1)
            Console.WriteLine("Usage: cslox [script]");
        else if (args.Length == 1)
            runFile(args[0]);
        else
            runPrompt();
        */

        var newExpr = new BinarySyntax(
            new UnarySyntax(
                new Token(TokenType.MINUS, "-", null, 1),
                new LiteralSyntax(123)),
            new Token(TokenType.STAR, "*", null, 1),
            new GroupingSyntax(new LiteralSyntax(45.67)));
        Console.WriteLine(new ASTPrinter().Print(newExpr));
    }
    
    static void runFile(string s)
    {
        var fileContents = File.ReadAllText(s);
        run(fileContents);
        if (_hadError)
            Environment.Exit(65);
    }
    
    static void runPrompt()
    {
        string? line;
        Console.Write(">");;
        while ((line = Console.ReadLine()) != null)
        {
            run(line);
            _hadError = false;
            Console.Write(">");
        }
    }

    static void run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.scanTokens();
        
        // for now just print tokens
        foreach (var token in tokens)
            Console.WriteLine(token);
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where} : {message}");
        _hadError = true;
    }
}