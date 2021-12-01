namespace CSLox;

class LoxClass : ILoxCallable
{
    internal string _name;
    internal readonly Dictionary<string, LoxFunction> _methods;

    public LoxClass(string name, Dictionary<string, LoxFunction> methods)
    {
        _name = name;
        _methods = methods;
    }

    public override string ToString() => _name;
    public int Arity()
    {
        var initializer = FindMethod("init");
        return initializer?.Arity() ?? 0;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);
        var initializer = FindMethod("init");
        initializer?.Bind(instance).Call(interpreter, arguments);
        
        return instance;
    }

    public LoxFunction? FindMethod(string name) => _methods.ContainsKey(name) ? _methods[name] : null;
}