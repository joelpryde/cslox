namespace CSLox;

static class CSLox
{
    static readonly Interpreter _interpreter = new();
    static bool _hadError;
    static bool _hadRuntimeError;

    static void Main(string[] args)
    {
        if (args.Length > 1)
            Console.WriteLine("Usage: cslox [script]");
        else if (args.Length == 1)
            RunFile(args[0]);
        else
            RunPrompt();
    }
    
    static void RunFile(string s)
    {
        var fileContents = File.ReadAllText(s);
        Run(fileContents);
        if (_hadError)
            Environment.Exit(65);
        if (_hadRuntimeError)
            Environment.Exit(70);
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

        if (_hadError)
            return;

        var output = _interpreter.Interpret(expression);
        Console.WriteLine(output);
    }

    internal static void Error(int line, string message) => Report(line, "", message);
    internal static void Error(Token token, string message) => 
        Report(token.line, token.type == TokenType.EOF ? " at end" : $" at '{token.lexeme}`", message);

    static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where} : {message}");
        _hadError = true;
    }

    public static void RuntimeError(RuntimeError error)
    {
        Console.Error.WriteLine($"{error.Message} \n[line {error._token.line}]");
        _hadRuntimeError = true;
    }
}