namespace MonkeyLangInterpreter
{
    internal class Lexer
    {

        public static Dictionary<string, TokenType> Keywords = new()
        {
            {"fn", TokenType.FUNCTION },
            {"let", TokenType.LET },
            {"true", TokenType.TRUE },
            {"false", TokenType.FALSE },
            {"if", TokenType.IF },
            {"else", TokenType.ELSE },
            {"return", TokenType.RETURN }
        };

        private string input;
        private int position;
        private int readPosition;
        private byte ch;
        public Lexer(string txt)
        {
            input = txt;
            ReadChar();
        }

        public Token NextToken()
        {
            Token token = new();
            SkipWhitespace();
            switch (ch)
            {
                case (byte)'=':
                    if (PeekChar() == '=')
                    {
                        char c = (char)ch;
                        ReadChar();
                        string l = c.ToString() + (char)ch;
                        token = new Token(TokenType.EQ, l);
                    }
                    else
                    {
                        token = new Token(TokenType.ASSIGN, (char)ch);
                    }
                    break;
                case (byte)'+':
                    token = new Token(TokenType.PLUS, (char)ch);
                    break;
                case (byte)'-':
                    token = new Token(TokenType.MINUS, (char)ch);
                    break;
                case (byte)'!':
                    if (PeekChar() == '=')
                    {
                        char c = (char)ch;
                        ReadChar();
                        string l = c.ToString() + (char)ch;
                        token = new Token(TokenType.NOT_EQ, l);
                    }
                    else
                    {
                        token = new Token(TokenType.BANG, (char)ch);
                    }
                    break;
                case (byte)'/':
                    token = new Token(TokenType.SLASH, (char)ch);
                    break;
                case (byte)'<':
                    token = new Token(TokenType.LT, (char)ch);
                    break;
                case (byte)'>':
                    token = new Token(TokenType.GT, (char)ch);
                    break;
                case (byte)'*':
                    token = new Token(TokenType.ASTERISK, (char)ch);
                    break;
                case (byte)';':
                    token = new Token(TokenType.SEMICOLON, (char)ch);
                    break;
                case (byte)'(':
                    token = new Token(TokenType.LPAREN, (char)ch);
                    break;
                case (byte)')':
                    token = new Token(TokenType.RPAREN, (char)ch);
                    break;
                case (byte)',':
                    token = new Token(TokenType.COMMA, (char)ch);
                    break;
                case (byte)'{':
                    token = new Token(TokenType.LBRACE, (char)ch);
                    break;
                case (byte)'}':
                    token = new Token(TokenType.RBRACE, (char)ch);
                    break;
                case (byte)'"':
                    token = new Token(TokenType.STRING, ReadString());
                    break;
                case (byte)'[':
                    token = new Token(TokenType.LBRACKET, (char)ch);
                    break;
                case (byte)']':
                    token = new Token(TokenType.RBRACKET, (char)ch);
                    break;
                case (byte)':':
                    token = new Token(TokenType.COLON, (char)ch);
                    break;
                case 0:
                    token = new Token(TokenType.EOF, "");
                    break;
                default:
                    if (IsLetter(ch))
                    {
                        token.Literal = ReadIdentifier();
                        token.TokenType = LookupIdent(token.Literal);
                        return token;
                    }
                    else if (IsDigit(ch))
                    {
                        token.Literal = ReadNumber();
                        token.TokenType = TokenType.INT;
                        return token;
                    }
                    else
                    {
                        token = new Token(TokenType.ILLEGAL, (char)ch);
                    }
                    break;

            }
            ReadChar();
            return token;
        }

        public string ReadString()
        {
            int pos = position + 1;
            while (true)
            {
                ReadChar();
                if (ch == '"' || ch == 0)
                {
                    break;
                }
            }
            return input.Substring(pos, position - pos);
        }

        public void ReadChar()
        {
            if (readPosition >= input.Length)
            {
                ch = 0;
            }
            else
            {
                ch = (byte)input[readPosition];
                position = readPosition;
                readPosition++;
            }
        }

        private string ReadIdentifier()
        {
            string s = "";
            while (IsLetter(ch))
            {
                s += (char)ch;
                ReadChar();
            }
            return s;
        }

        private string ReadNumber()
        {
            string s = "";
            while (IsDigit(ch))
            {
                s += (char)ch;
                ReadChar();
            }
            return s;
        }

        private bool IsLetter(byte ch)
        {
            return ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ch == '_';
        }

        private bool IsDigit(byte ch)
        {
            return '0' <= ch && ch <= '9';
        }

        private TokenType LookupIdent(string ident)
        {
            TokenType tok;
            if (Keywords.TryGetValue(ident, out tok))
                return tok;
            return TokenType.IDENT;
        }

        private void SkipWhitespace()
        {
            while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r')
            {
                ReadChar();
            }
        }

        private byte PeekChar()
        {
            if (readPosition >= input.Length)
            {
                return 0;
            }
            return (byte)input[readPosition];
        }
    }
}
