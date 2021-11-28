namespace CSLox;

public readonly record struct Token(TokenType type, string lexeme, object? literal, int line, int spanBegin, int spanEnd);