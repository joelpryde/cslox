namespace CSLox;

interface ILoxCallable
{
    int Arity();
    object? Call(Interpreter interpreter, List<object?> arguments);
}

class LoxFunction : ILoxCallable
{
    readonly FunctionDeclarationStatementSyntax _functionDeclaration;
    readonly Environment _closure;

    public LoxFunction(FunctionDeclarationStatementSyntax functionDeclaration, Environment closure)
    {
        _functionDeclaration = functionDeclaration;
        _closure = closure;
    }

    public int Arity() => _functionDeclaration.parameters.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(_closure);
        for (var i = 0; i < _functionDeclaration.parameters.Count; i++)
            environment.Define(_functionDeclaration.parameters[i].lexeme, arguments[i]);

        try
        {
            interpreter.ExecuteBlock(_functionDeclaration.bodyStatements, environment);
        }
        catch (CallReturnException returnException)
        {
            return returnException._returnValue;
        }
        
        return null;
    }
    
    public override string ToString() => $"<fn {_functionDeclaration.name.lexeme}>";
}

class ClockLoxCallable : ILoxCallable
{
    public int Arity() => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments) =>
        (double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000.0;

    public override string ToString() => "<native fn>";
}