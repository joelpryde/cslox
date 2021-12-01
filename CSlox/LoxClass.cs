namespace CSLox;

class LoxClass : ILoxCallable
{
    internal readonly string _name;
    readonly Dictionary<string, LoxFunction> _methods;
    readonly LoxClass? _superClass;

    public LoxClass(string name, LoxClass? superClass, Dictionary<string, LoxFunction> methods)
    {
        _name = name;
        _superClass = superClass;
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

    public LoxFunction? FindMethod(string name)
    {
        if (_methods.ContainsKey(name))
            return _methods[name];
        
        return _superClass?.FindMethod(name);
    }
}