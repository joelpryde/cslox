class Scanner
{
    string _source;
    List<Token> _tokens = new();
    int _start, _current; 
    int _line = 1;
    
    public Scanner(string source)
    {
        _source = source;
    }

    public List<Token> scanTokens()
    {
        while (!isAtEnd())
        {
            _start = _current;
            scanToken();
        }
        
        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    bool isAtEnd() => _current >= _source.Length;
    char advance() => _source[_current++];
    void addToken(TokenType type, object? literal) => _tokens.Add(new Token(type, _source.Substring(_start, _current), literal, _line));
    void addToken(TokenType type) => addToken(type, null);
    
    void scanToken()
    {
        var c = advance();
        
        switch (c)
        {
            case '(': addToken(TokenType.LEFT_PAREN); break;
            case ')': addToken(TokenType.RIGHT_PAREN); break;
            case '{': addToken(TokenType.LEFT_BRACE); break;
            case '}': addToken(TokenType.RIGHT_BRACE); break;
            case ',': addToken(TokenType.COMMA); break;
            case '.': addToken(TokenType.DOT); break;
            case '-': addToken(TokenType.MINUS); break;
            case '+': addToken(TokenType.PLUS); break;
            case ';': addToken(TokenType.SEMICOLON); break;
            case '*': addToken(TokenType.STAR); break;
            
            default:
                CSLox.Error(_line, $"Unexpected character {c}.");
                break;
        }
    }
}