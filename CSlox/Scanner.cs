namespace CSLox;

internal class Scanner
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

    bool isDigit(char c) => c is >= '0' and <= '9';
    bool isAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    bool isAlphaNumeric(char c) => isAlpha(c) || isDigit(c);
    bool isAtEnd() => _current >= _source.Length;
    char advance() => _source[_current++];
    void addToken(TokenType type, object? literal) => _tokens.Add(new Token(type, _source.Substring(_start, _current-_start), literal, _line));
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
            
            case '!': addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '=': addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '<': addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            
            case '/':
                if (match('/'))
                    while (peek() != '\n' && isAtEnd())
                        advance();
                else
                    addToken(TokenType.SLASH);
                break;
            
            case ' ':
            case '\r':
            case '\t':
                // ignore whitespace
                break;

            case '\n':
                _line++;
                break;
            
            case '"': scanString(); break;

            default:
                if (isDigit(c))
                    scanNumber();
                else if (isAlpha(c))
                    scanIdentifier();
                else
                    CSLox.Error(_line, $"Unexpected character {c}.");
                break;
        }
    }

    static Dictionary<string, TokenType> s_KeywordMap = new()
    {
        {"and", TokenType.AND},
        {"class", TokenType.CLASS},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"for", TokenType.FOR},
        {"fun", TokenType.FUN},
        {"if", TokenType.IF},
        {"nil", TokenType.NIL},
        {"or", TokenType.OR},
        {"print", TokenType.PRINT},
        {"return", TokenType.RETURN},
        {"super", TokenType.SUPER},
        {"this", TokenType.THIS},
        {"var", TokenType.VAR},
        {"while", TokenType.WHILE}
    };
    
    void scanIdentifier()
    {
        while (isAlphaNumeric(peek()))
            advance();

        var identifierText = _source.Substring(_start, _current - _start);
        if (s_KeywordMap.TryGetValue(identifierText, out var keywordType))
            addToken(TokenType.IDENTIFIER, keywordType);
        else
            addToken(TokenType.IDENTIFIER);
    }

    void scanNumber()
    {
        while (isDigit(peek()))
            advance();
        
        // Look for fractional part
        if (peek() == '.' && isDigit(peekNext()))
        {
            advance(); // Consume the "."

            while (isDigit(peek()))
                advance();
        }
        
        addToken(TokenType.NUMBER, double.Parse(_source.Substring(_start, _current-_start)));
    }

    void scanString()
    {
        while (peek() != '"' && !isAtEnd())
        {
            if (peek() == '\n')
                _line++;
            advance();
        }

        if (isAtEnd())
        {
            CSLox.Error(_line, "Unterminated string");
            return;
        }

        advance(); // closing "
        
        // Trim quotes
        var value = _source.Substring(_start + 1, _current - _start - 1);
        addToken(TokenType.STRING, value);
    }

    char peek()
    {
        if (isAtEnd())
            return '\0';
        return _source[_current];
    }
    
    char peekNext()
    {
        if (_current + 1 >= _source.Length)
            return '\0';
        return _source[_current + 1];
    }

    bool match(char expected)
    {
        if (isAtEnd()) 
            return false;
        if (_source[_current] != expected)
            return false;
        _current++;
        return true;
    }
}