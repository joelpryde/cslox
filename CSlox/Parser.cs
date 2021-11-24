using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace CSLox;

internal class Parser
{
    List<Token> _tokens;
    int _current;

    public Parser(List<Token> tokens) => _tokens = tokens;
    
    Token Peek() => _tokens[_current];
    Token Previous() => _tokens[_current - 1];
    bool IsAtEnd() => Peek().type == TokenType.EOF;
    bool Check(TokenType token) => !IsAtEnd() && Peek().type == token;

    Token Advance()
    {
        if (!IsAtEnd())
            _current++;
        return Previous();
    }

    bool Match(params TokenType[] tokens)
    {
        if (tokens.Any(Check))
        {
            Advance();
            return true;
        }

        return false;
    }

    public List<StatementSyntax> Parse()
    {
        try
        {
            var statements = new List<StatementSyntax>();
            while (!IsAtEnd())
            {
                var declaration = DeclarationRule();
                if (declaration != null)
                    statements.Add(declaration);
            }
            return statements;
        }
        catch (Exception)
        {
            return new List<StatementSyntax>();
        }
    }

    StatementSyntax? DeclarationRule()
    {
        try
        {
            if (Match(TokenType.VAR))
                return VariableDeclarationRule();
            return StatementRule();
        }
        catch (ParseException error)
        {
            SynchronizeError();
            return null;
        }
    }

    StatementSyntax? VariableDeclarationRule()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        ExpressionSyntax? initializer = null;
        if (Match(TokenType.EQUAL))
            initializer = ExpressionRule();

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new VariableDeclarationStatementSyntax(name, initializer);

    }

    StatementSyntax StatementRule()
    {
        if (Match(TokenType.PRINT))
            return PrintStatementRule();
        return ExpressionStatementRule();
    }
    
    StatementSyntax PrintStatementRule()
    {
        var expression = ExpressionRule();
        Consume(TokenType.SEMICOLON, "Expect '; after value.");
        return new PrintStatementSyntax(expression);
    }

    StatementSyntax ExpressionStatementRule()
    {
        var expression = ExpressionRule();
        Consume(TokenType.SEMICOLON, "Expect '; after expression.");
        return new ExpressionStatementSyntax(expression);
    }

    ExpressionSyntax ExpressionRule() => EqualityRule();

    ExpressionSyntax EqualityRule()
    {
        var expression = ComparisonRule();
        while (Match(TokenType.BANG, TokenType.EQUAL_EQUAL))
        {
            var operatorToken = Previous();
            var rightExpression = ComparisonRule();
            expression = new BinaryExpressionSyntax(expression, operatorToken, rightExpression);
        }

        return expression;
    }

    ExpressionSyntax ComparisonRule()
    {
        var expression = TerminalRule();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var operatorToken = Previous();
            var rightExpression = TerminalRule();
            expression = new BinaryExpressionSyntax(expression, operatorToken, rightExpression);
        }

        return expression;
    }

    ExpressionSyntax TerminalRule()
    {
        var expression = FactorRule();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            var operatorToken = Previous();
            var rightExpression = FactorRule();
            expression = new BinaryExpressionSyntax(expression, operatorToken, rightExpression);
        }

        return expression;
    }

    ExpressionSyntax FactorRule()
    {
        var expression = UnaryRule();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            var operatorToken = Previous();
            var rightExpression = UnaryRule();
            expression = new BinaryExpressionSyntax(expression, operatorToken, rightExpression);
        }

        return expression;
    }

    ExpressionSyntax UnaryRule()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            var operatorToken = Previous();
            var rightExpression = UnaryRule();
            return new UnaryExpressionSyntax(operatorToken, rightExpression);
        }

        return PrimaryRule();
    }

    ExpressionSyntax PrimaryRule()
    {
        if (Match(TokenType.FALSE)) return new LiteralExpressionSyntax(false);
        if (Match(TokenType.TRUE)) return new LiteralExpressionSyntax(false);
        if (Match(TokenType.NIL)) return new LiteralExpressionSyntax(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
            return new LiteralExpressionSyntax(Previous().literal);

        if (Match(TokenType.IDENTIFIER))
            return new VariableExpressionSyntax(Previous());

        if (Match(TokenType.LEFT_PAREN))
        {
            var expression = ExpressionRule();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new GroupingExpressionSyntax(expression);
        }

        throw Error(Peek(), "Expect expression.");
    }

    Token Consume(TokenType token, string errorMessage)
    {
        if (Check(token))
            return Advance();

        throw Error(Peek(), errorMessage);
    }

    class ParseException : Exception { }
    ParseException Error(Token token, string errorMessage)
    {
        CSLox.Error(token, errorMessage);
        return new ParseException();
    }

    void SynchronizeError()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.SEMICOLON)
                return;

            switch (Peek().type)
            {
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.PRINT:
                case TokenType.RETURN:
                case TokenType.VAR:
                case TokenType.WHILE:
                    return;
            }

            Advance();
        }
    }
}