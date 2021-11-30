namespace CSLox;

class LoxClass : ILoxCallable
{
    internal string _name;
    
    public LoxClass(string name) => _name = name;
    public override string ToString() => _name;
    public int Arity() => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);
        return instance;
    }
}