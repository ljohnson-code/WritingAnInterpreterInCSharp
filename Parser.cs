namespace MonkeyLangInterpreter
{

    internal struct prefixParseFn
    {
        public Func<Parser, AST.IExpression> fn;
    }
    internal struct infixParseFn
    {
        public Func<Parser, AST.IExpression, AST.IExpression> fn;
    }

    internal class Parser
    {
        public static Dictionary<TokenType, PrecedenceType> Precedences = new()
        {
            {TokenType.EQ, PrecedenceType.EQUALS },
            {TokenType.NOT_EQ, PrecedenceType.EQUALS },
            {TokenType.LT, PrecedenceType.LESSGREATER },
            {TokenType.GT, PrecedenceType.LESSGREATER },
            {TokenType.PLUS, PrecedenceType.SUM },
            {TokenType.MINUS, PrecedenceType.SUM },
            {TokenType.SLASH, PrecedenceType.PRODUCT },
            {TokenType.ASTERISK, PrecedenceType.PRODUCT },
            {TokenType.LPAREN, PrecedenceType.CALL },
            {TokenType.LBRACKET, PrecedenceType.INDEX }
        };


        public Lexer l;
        public Token curToken;
        public Token peekToken;
        public List<string> errors = new();
        public Dictionary<TokenType, prefixParseFn> prefixParseFns = new();
        public Dictionary<TokenType, infixParseFn> infixParseFns = new();
        public Parser(Lexer lexer)
        {
            l = lexer;
            curToken = new Token();
            peekToken = new Token();
            NextToken();
            NextToken();

            #region Prefix
            Func<Parser, AST.IExpression> parseIdentifier = (Parser p) =>
            {
                return new AST.Identifier() { Token = p.curToken, Value = p.curToken.Literal };
            };
            RegisterPrefix(TokenType.IDENT, new prefixParseFn() { fn = parseIdentifier });


            Func<Parser, AST.IExpression> parseIntegerLiteral = (Parser p) =>
            {

                AST.IntegerLiteral lit = new() { Token = p.curToken };
                long value;
                if (long.TryParse(p.curToken.Literal, out value))
                {
                    lit.Value = value;
                    return lit;
                }
                string msg = $"Could not parse {p.curToken.Literal} as integer";
                p.errors.Add(msg);
                return null;

            };

            RegisterPrefix(TokenType.INT, new prefixParseFn() { fn = parseIntegerLiteral });


            Func<Parser, AST.IExpression> parsePrefixExpression = (Parser p) =>
            {
                AST.PrefixExpression expression = new() { Token = p.curToken, Operator = p.curToken.Literal };
                p.NextToken();
                expression.Right = p.ParseExpression((int)PrecedenceType.PREFIX);

                return expression;
            };

            RegisterPrefix(TokenType.BANG, new prefixParseFn() { fn = parsePrefixExpression });
            RegisterPrefix(TokenType.MINUS, new prefixParseFn() { fn = parsePrefixExpression });

            Func<Parser, AST.IExpression> parseBoolean = (Parser p) =>
            {
                return new AST.Boolean() { Token = p.curToken, Value = p.CurTokenIs(TokenType.TRUE) };
            };

            RegisterPrefix(TokenType.TRUE, new prefixParseFn() { fn = parseBoolean });
            RegisterPrefix(TokenType.FALSE, new prefixParseFn() { fn = parseBoolean });

            Func<Parser, AST.IExpression> parseGroupedExpression = (Parser p) =>
            {
                p.NextToken();
                AST.IExpression? exp = p.ParseExpression((int)PrecedenceType.LOWEST);
                if (!p.ExpectPeek(TokenType.RPAREN))
                {
                    return null;
                }
                return exp;
            };

            RegisterPrefix(TokenType.LPAREN, new prefixParseFn() { fn = parseGroupedExpression });

            Func<Parser, AST.IExpression> parseIfExpression = (Parser p) =>
            {
                AST.IfExpression expression = new() { Token = p.curToken };
                if (!p.ExpectPeek(TokenType.LPAREN))
                {
                    return null;
                }
                p.NextToken();
                expression.Condition = p.ParseExpression((int)PrecedenceType.LOWEST);
                if (!p.ExpectPeek(TokenType.RPAREN))
                {
                    return null;
                }
                if (!p.ExpectPeek(TokenType.LBRACE))
                {
                    return null;
                }

                expression.Consequence = p.ParseBlockStatement();

                if (p.PeekTokenIs(TokenType.ELSE))
                {
                    p.NextToken();

                    if (!p.ExpectPeek(TokenType.LBRACE))
                    {
                        return null;
                    }
                    expression.Alternative = p.ParseBlockStatement();
                }

                return expression;
            };

            RegisterPrefix(TokenType.IF, new prefixParseFn() { fn = parseIfExpression });

            Func<Parser, AST.IExpression> parseFunctionLiteral = (Parser p) =>
            {
                AST.FunctionLiteral lit = new() { Token = p.curToken };
                if (!p.ExpectPeek(TokenType.LPAREN))
                {
                    return null;
                }

                lit.Parameters = p.ParseFunctionParameters();

                if (!p.ExpectPeek(TokenType.LBRACE))
                {
                    return null;
                }

                lit.Body = p.ParseBlockStatement();

                return lit;
            };

            RegisterPrefix(TokenType.FUNCTION, new prefixParseFn() { fn = parseFunctionLiteral });


            Func<Parser, AST.IExpression> parseStringLiteral = (Parser p) =>
            {
                return new AST.StringLiteral() { Token = curToken, Value = curToken.Literal };
            };

            RegisterPrefix(TokenType.STRING, new prefixParseFn() { fn = parseStringLiteral });

            Func<Parser, AST.IExpression> parseArrayLiteral = (Parser p) =>
            {
                AST.ArrayLiteral array = new()
                {
                    Token = curToken,
                    Elements = ParseExpressionList(TokenType.RBRACKET)
                };
                return array;
            };

            RegisterPrefix(TokenType.LBRACKET, new prefixParseFn() { fn = parseArrayLiteral });

            Func<Parser, AST.IExpression> parseHashLiteral = (Parser p) =>
            {
                AST.HashLiteral hash = new()
                {
                    Pairs = new Dictionary<AST.IExpression, AST.IExpression>()
                };

                while (!p.PeekTokenIs(TokenType.RBRACE))
                {
                    p.NextToken();
                    AST.IExpression? key = p.ParseExpression((int)PrecedenceType.LOWEST);
                    if (!p.ExpectPeek(TokenType.COLON))
                    {
                        return null;
                    }
                    p.NextToken();
                    AST.IExpression? value = p.ParseExpression((int)PrecedenceType.LOWEST);
                    hash.Pairs.Add(key, value);

                    if (!p.PeekTokenIs(TokenType.RBRACE) && !p.ExpectPeek(TokenType.COMMA))
                    {
                        return null;
                    }

                }

                if (!p.ExpectPeek(TokenType.RBRACE))
                {
                    return null;
                }

                return hash;
            };

            RegisterPrefix(TokenType.LBRACE, new prefixParseFn() { fn = parseHashLiteral });

            #endregion

            #region InfixExpressions
            Func<Parser, AST.IExpression, AST.IExpression> parseInfixExpression = (Parser p, AST.IExpression left) =>
            {
                AST.InfixExpression expression = new()
                {
                    Token = p.curToken,
                    Operator = p.curToken.Literal,
                    Left = left
                };
                int precedence = p.CurPrecedence();
                p.NextToken();

                expression.Right = p.ParseExpression(precedence);

                return expression;
            };
            RegisterInfix(TokenType.PLUS, new infixParseFn() { fn = parseInfixExpression });
            RegisterInfix(TokenType.MINUS, new infixParseFn() { fn = parseInfixExpression });
            RegisterInfix(TokenType.SLASH, new infixParseFn() { fn = parseInfixExpression });
            RegisterInfix(TokenType.ASTERISK, new infixParseFn() { fn = parseInfixExpression });
            RegisterInfix(TokenType.EQ, new infixParseFn() { fn = parseInfixExpression });
            RegisterInfix(TokenType.NOT_EQ, new infixParseFn() { fn = parseInfixExpression });
            RegisterInfix(TokenType.LT, new infixParseFn() { fn = parseInfixExpression });
            RegisterInfix(TokenType.GT, new infixParseFn() { fn = parseInfixExpression });


            Func<Parser, AST.IExpression, AST.IExpression> parseCallExpression = (Parser p, AST.IExpression function) =>
            {
                AST.CallExpression exp = new()
                {
                    Token = p.curToken,
                    Function = function,
                    Arguments = p.ParseExpressionList(TokenType.RPAREN)
                };

                return exp;
            };

            RegisterInfix(TokenType.LPAREN, new infixParseFn() { fn = parseCallExpression });


            Func<Parser, AST.IExpression, AST.IExpression> parseIndexExpression = (Parser p, AST.IExpression left) =>
            {
                AST.IndexExpression exp = new() { Token = p.curToken, Left = left };
                p.NextToken();

                exp.Index = p.ParseExpression((int)PrecedenceType.LOWEST);
                if (!p.ExpectPeek(TokenType.RBRACKET))
                {
                    return null;
                }

                return exp;
            };
            RegisterInfix(TokenType.LBRACKET, new infixParseFn() { fn = parseIndexExpression });

            #endregion
        }

        public AST.IExpression[] ParseExpressionList(TokenType end)
        {
            List<AST.IExpression> list = new();
            if (PeekTokenIs(end))
            {
                NextToken();
                return list.ToArray();
            }

            NextToken();
            list.Add(ParseExpression((int)PrecedenceType.LOWEST));
            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken();
                NextToken();
                list.Add(ParseExpression((int)PrecedenceType.LOWEST));
            }
            if (!ExpectPeek(end))
            {
                return null;
            }

            return list.ToArray();
        }

        public AST.Identifier[] ParseFunctionParameters()
        {
            List<AST.Identifier> identifiers = new();

            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return identifiers.ToArray();
            }

            NextToken();
            AST.Identifier ident = new() { Token = curToken, Value = curToken.Literal };
            identifiers.Add(ident);

            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken();
                NextToken();
                AST.Identifier idt = new() { Token = curToken, Value = curToken.Literal };
                identifiers.Add(idt);
            }

            if (!ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }
            return identifiers.ToArray();
        }

        public AST.IExpression[] parseCallArguments()
        {
            List<AST.IExpression> args = new();
            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return args.ToArray();
            }
            NextToken();
            args.Add(ParseExpression((int)PrecedenceType.LOWEST));
            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken();
                NextToken();
                args.Add(ParseExpression((int)PrecedenceType.LOWEST));
            }
            if (!ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }
            return args.ToArray();
        }

        public AST.BlockStatement ParseBlockStatement()
        {
            AST.BlockStatement block = new() { Token = curToken };
            List<AST.IStatement> statements = new();
            NextToken();
            while (!CurTokenIs(TokenType.RBRACE) && !CurTokenIs(TokenType.EOF))
            {
                AST.IStatement? stmt = ParseStatement();
                if (stmt != null)
                {
                    statements.Add(stmt);
                }
                NextToken();
            }
            block.Statements = statements.ToArray();
            return block;
        }

        public void NextToken()
        {
            curToken = peekToken;
            peekToken = l.NextToken();
        }

        public int CurPrecedence()
        {
            try
            {
                return (int)Precedences[curToken.TokenType];
            }
            catch
            {
                return (int)PrecedenceType.LOWEST;
            }
        }

        public int PeekPrecedence()
        {
            try
            {
                return (int)Precedences[peekToken.TokenType];
            }
            catch
            {
                return (int)PrecedenceType.LOWEST;
            }
        }

        private AST.IStatement? ParseStatement()
        {
            switch (curToken.TokenType)
            {
                case TokenType.LET:
                    return ParseLetStatement();
                case TokenType.RETURN:
                    return ParseReturnStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        public AST.ExpressionStatement ParseExpressionStatement()
        {
            AST.ExpressionStatement stmt = new()
            {
                Token = curToken,
                Expression = ParseExpression((int)PrecedenceType.LOWEST)
            };

            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }


            return stmt;
        }


        public AST.IExpression? ParseExpression(int precedence)
        {
            try
            {
                prefixParseFn prefix = prefixParseFns[curToken.TokenType];
                if (prefix.fn == null)
                {
                    NoPrefixParseFnError(curToken.TokenType);
                    return null;
                }
                AST.IExpression? leftExp = prefix.fn(this);

                while ((!PeekTokenIs(TokenType.SEMICOLON)) && precedence < PeekPrecedence())
                {
                    infixParseFn infix = infixParseFns[peekToken.TokenType];
                    if (infix.fn == null)
                    {
                        NoPrefixParseFnError(curToken.TokenType);
                        return null;
                    }
                    NextToken();
                    leftExp = infix.fn(this, leftExp);
                }
                return leftExp;
            }
            catch (Exception ex)
            {
                if (ex is System.Collections.Generic.KeyNotFoundException)
                    NoPrefixParseFnError(curToken.TokenType);
                return null;
            }

        }

        public void RegisterPrefix(TokenType type, prefixParseFn func)
        {
            prefixParseFns[type] = func;
        }

        public void RegisterInfix(TokenType type, infixParseFn func)
        {
            infixParseFns[type] = func;
        }

        private void NoPrefixParseFnError(TokenType type)
        {
            string msg = "no prefix parse function for " + type + " found";
            errors.Add(msg);
        }

        private AST.ReturnStatement ParseReturnStatement()
        {
            AST.ReturnStatement stmt = new() { Token = curToken };
            NextToken();
            stmt.ReturnValue = ParseExpression((int)PrecedenceType.LOWEST);
            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;
        }

        private AST.LetStatement? ParseLetStatement()
        {
            AST.LetStatement stmt = new() { Token = curToken };

            if (!ExpectPeek(TokenType.IDENT))
            {
                return null;
            }

            stmt.Name = new AST.Identifier() { Token = curToken, Value = curToken.Literal };

            if (!ExpectPeek(TokenType.ASSIGN))
            {
                return null;
            }

            NextToken();

            stmt.Value = ParseExpression((int)PrecedenceType.LOWEST);
            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;

        }
        private bool CurTokenIs(TokenType t)
        {
            return curToken.TokenType == t;
        }
        private bool PeekTokenIs(TokenType t)
        {
            return peekToken.TokenType == t;
        }

        private void PeekError(TokenType t)
        {
            errors.Add($"expected next token to be {t}, got {peekToken.TokenType} instead");
        }

        private bool ExpectPeek(TokenType t)
        {
            if (PeekTokenIs(t))
            {
                NextToken();
                return true;
            }
            PeekError(t);
            return false;
        }

        public AST.Program? ParseProgram()
        {
            AST.Program prog = new();
            List<AST.IStatement> statements = new();
            while (curToken.TokenType != TokenType.EOF)
            {
                AST.IStatement? statement = ParseStatement();
                if (statement != null)
                {
                    statements.Add(statement);
                }
                NextToken();
            }
            prog.Statements = statements.ToArray();
            return prog;
        }
    }
}
