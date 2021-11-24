namespace CSLox;

class Environment
{
    Dictionary<string, object?> _values = new();
    internal void Define(string name, object? value) => _values[name] = value;
    internal object? Get(Token name) => 
        _values.ContainsKey(name.lexeme) ? _values[name.lexeme] : throw new RuntimeError(name, $"Undefined variable {name.lexeme}.");
}