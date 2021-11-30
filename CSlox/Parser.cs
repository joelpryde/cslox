namespace CSLox;

class Parser
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
            if (Match(TokenType.CLASS))
                return ClassDeclarationRule();
            if (Match(TokenType.FUN))
                return FunctionDeclarationRule("function");
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

    StatementSyntax ClassDeclarationRule()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect class name.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

        var methods = new List<StatementSyntax>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            methods.Add(FunctionDeclarationRule("method"));

        Consume(TokenType.RIGHT_BRACE, "Expect '{' after class body.");

        return new ClassStatementSyntax(name, methods);
    }

    StatementSyntax FunctionDeclarationRule(string kind)
    {
        var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
        Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
        
        var parameters = new List<Token>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count > 255)
                    Error(Peek(), "Can't have more than 255 parameters.");
                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
        var body = BlockStatementRule();

        return new FunctionDeclarationStatementSyntax(name, parameters, body);
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
        if (Match(TokenType.FOR))
            return ForStatementRule();
        if (Match(TokenType.IF))
            return IfStatementRule();
        if (Match(TokenType.PRINT))
            return PrintStatementRule();
        if (Match(TokenType.RETURN))
            return ReturnStatementRule();
        if (Match(TokenType.WHILE))
            return WhileStatementRule();
        if (Match(TokenType.LEFT_BRACE))
            return new BlockStatementSyntax(BlockStatementRule());
        return ExpressionStatementRule();
    }

    StatementSyntax ReturnStatementRule()
    {
        var keyword = Previous();
        ExpressionSyntax? value = null;
        if (!Check(TokenType.SEMICOLON))
            value = EqualityRule();

        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new ReturnStatementSyntax(keyword, value);
    }

    StatementSyntax ForStatementRule()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        StatementSyntax? initializer = null;
        if (Match(TokenType.SEMICOLON))
            initializer = null;
        else if (Match(TokenType.VAR))
            initializer = VariableDeclarationRule();
        else
            initializer = ExpressionStatementRule();

        ExpressionSyntax? condition = null;
        if (!Check(TokenType.SEMICOLON))
            condition = ExpressionRule();
        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        ExpressionSyntax? increment = null;
        if (!Check((TokenType.RIGHT_PAREN)))
            increment = ExpressionRule();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        var body = StatementRule();

        // Construct 'for' syntax by lowering to 'while'
        if (increment != null)
            body = new BlockStatementSyntax(new List<StatementSyntax> { body, new ExpressionStatementSyntax(increment) });
        if (condition == null)
            condition = new LiteralExpressionSyntax(true);
        body = new WhileStatementSyntax(condition, body);
        if (initializer != null)
            body = new BlockStatementSyntax(new List<StatementSyntax> { initializer, body });
        
        return body;
    }

    StatementSyntax WhileStatementRule()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = ExpressionRule();
        Consume(TokenType.RIGHT_PAREN, "Expect '(' after condition.");
        var body = StatementRule();

        return new WhileStatementSyntax(condition, body);
    }

    StatementSyntax IfStatementRule()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = ExpressionRule();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        var thenBranch = StatementRule();
        StatementSyntax? elseBranch = null;
        if (Match(TokenType.ELSE))
            elseBranch = StatementRule();

        return new IfStatementSyntax(condition, thenBranch, elseBranch);
    }

    List<StatementSyntax> BlockStatementRule()
    {
        var statements = new List<StatementSyntax>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            var declaration = DeclarationRule();
            if (declaration != null)
                statements.Add(declaration);
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
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

    ExpressionSyntax ExpressionRule() => AssignmentRule();

    ExpressionSyntax AssignmentRule()
    {
        var expression = OrRule();

        if (Match(TokenType.EQUAL))
        {
            var equals = Previous();
            var value = AssignmentRule();

            if (expression is VariableExpressionSyntax variableExpressionSyntax)
            {
                var name = variableExpressionSyntax.name;
                return new AssignmentExpressionSyntax(name, value);
            }
            else if (expression is GetExpressionSyntax getExpression)
                return new SetExpressionSyntax(getExpression.objectSyntax, getExpression.name, value);

            Error(equals, "Invalid assignment target.");
        }

        return expression;
    }

    ExpressionSyntax OrRule()
    {
        var expression = AndRule();

        while (Match(TokenType.OR))
        {
            var operatorToken = Previous();
            var rightExpression = AndRule();
            expression = new LogicalExpressionSyntax(expression, operatorToken, rightExpression);
        }

        return expression;
    }

    ExpressionSyntax AndRule()
    {
        var expression = EqualityRule();

        while (Match(TokenType.AND))
        {
            var operatorToken = Previous();
            var rightExpression = EqualityRule();
            expression = new LogicalExpressionSyntax(expression, operatorToken, rightExpression);
        }

        return expression;
    }

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
        
        return CallRule();
    }

    ExpressionSyntax CallRule()
    {
        var expression = PrimaryRule();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
                expression = FinishCall(expression);
            else if (Match(TokenType.DOT))
            {
                var name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expression = new GetExpressionSyntax(expression, name);
            }
            else
                break;
        }

        return expression;
    }

    ExpressionSyntax FinishCall(ExpressionSyntax callee)
    {
        var arguments = new List<ExpressionSyntax>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                    Error(Peek(), "Can't have more than 255 arguments in function call.");
                arguments.Add(EqualityRule());
            } while (Match(TokenType.COMMA));
        }

        var parenToken = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new CallExpressionSyntax(callee, parenToken, arguments);
    }

    ExpressionSyntax PrimaryRule()
    {
        if (Match(TokenType.FALSE)) return new LiteralExpressionSyntax(false);
        if (Match(TokenType.TRUE)) return new LiteralExpressionSyntax(true);
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