namespace CSLox;

class LoxInstance
{
    LoxClass _klass;
    Dictionary<string, object?> _fields = new();

    public LoxInstance(LoxClass klass) => _klass = klass;

    public override string ToString() => $"{_klass._name} instance";

    public object? Get(Token name)
    {
        if (_fields.ContainsKey(name.lexeme))
            return _fields[name.lexeme];

        if (_klass.FindMethod(name.lexeme))
        {
            var method = _klass._methods[name.lexeme];
            return method.Bind(this);
        }

        throw new RuntimeError(name, $"Undefined property {name.lexeme}.");
    }

    public void Set(Token name, object? objectValue)
    {
        _fields[name.lexeme] = objectValue;
    }
}