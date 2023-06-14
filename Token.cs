namespace MonkeyLangInterpreter
{

    public enum TokenType
    {
        ILLEGAL,
        EOF,
        IDENT,
        INT,
        ASSIGN,
        PLUS,
        MINUS,
        BANG,
        ASTERISK,
        SLASH,
        LT,
        GT,
        COMMA,
        SEMICOLON,
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        FUNCTION,
        LET,
        IF,
        ELSE,
        TRUE,
        FALSE,
        RETURN,
        EQ,
        NOT_EQ,
        STRING,
        LBRACKET,
        RBRACKET,
        COLON
    }

    public enum PrecedenceType
    {
        _, //iota
        LOWEST,
        EQUALS, // ==
        LESSGREATER, // > or <
        SUM, // +
        PRODUCT, // *
        PREFIX, // -X or !X
        CALL,
        INDEX

    }



    public struct Token
    {
        public TokenType TokenType;
        public string Literal;
        public Token()
        {
            TokenType = TokenType.EOF;
            Literal = "";
        }
        public Token(TokenType type, string l)
        {
            TokenType = type;
            Literal = l;
        }
        public Token(TokenType type, char l)
        {
            TokenType = type;
            Literal = "" + l;
        }
        public void Print()
        {
            Console.WriteLine($"{{Type:{TokenType} Literal:{Literal}}}");
        }
    }
}
