namespace CSLox;

class Environment
{
    internal Environment? _enclosing = null;
    Dictionary<string, object?> _values = new();

    public Environment() {}
    public Environment(Environment enclosing) => _enclosing = enclosing;
    
    internal void Define(string name, object? value) => _values[name] = value;

    internal object? Get(Token name)
    {
        if (_values.ContainsKey(name.lexeme))
            return _values[name.lexeme];

        if (_enclosing != null)
            return _enclosing.Get(name);
        
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }
    
    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.lexeme))
            _values[name.lexeme] = value;
        else if (_enclosing != null)
            _enclosing.Assign(name, value);
        else 
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public object? GetAt(int distance, string name)
    {
        var ancestor = Ancestor(distance); 
        return ancestor?._values[name];
    }

    Environment Ancestor(int distance)
    {
        var environment = this;
        for (var i = 0; i < distance; i++)
            environment = environment._enclosing;

        return environment;
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)._values[name.lexeme] = value;
    }
}