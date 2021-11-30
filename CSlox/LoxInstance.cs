namespace CSLox;

class LoxInstance
{
    LoxClass _klass;
    
    public LoxInstance(LoxClass klass) => _klass = klass;

    public override string ToString() => $"{_klass._name} instance";
}