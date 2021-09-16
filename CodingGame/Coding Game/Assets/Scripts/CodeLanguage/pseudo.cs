using System.Collections.Generic;

public class pseudo {

    public enum TokenType {
        TT_INT = 0,
        TT_FLOAT,
        PLUS,
        MINUS,
        MUL,
        DIV,
        LPAREN,
        RPAREN
    }

    protected string DIGITS = "0123456789";

    public TokenError Run(string fileName, string text) {
        Lexer lexer = new Lexer(fileName, text);

        TokenError tokenError = lexer.MakeTokens();

        if(tokenError.error != null) 
            return new TokenError(null, tokenError.error);

        // Generate Abstract Syntax Tree (AST)
        Parser parser = new Parser(tokenError.results);
        tokenError.ast = parser.Parse();

        return tokenError;
    }
}

public class NumberNode {
    Token token;

    public NumberNode(Token token) {
        this.token = token;
    }

    public string Display() {
        return token.Display();
    }
}

public class BinOpNode {

    NumberNode leftNode = null, rightNode = null;
    BinOpNode leftNodeBin = null, rightNodeBin = null;
    Token opToken;

    public BinOpNode(NumberNode leftNode, Token opToken, NumberNode rightNode) {
        this.leftNode = leftNode;
        this.rightNode = rightNode;
        this.opToken = opToken;
    }

    public BinOpNode(BinOpNode leftNode, Token opToken, BinOpNode rightNode) {
        leftNodeBin = leftNode;
        rightNodeBin = rightNode;
        this.opToken = opToken;
    }

    public BinOpNode(NumberNode rightNode) {
        this.rightNode = rightNode;
    }

    public BinOpNode(BinOpNode binOpNode) {
        leftNode = binOpNode.leftNode;
        rightNode = binOpNode.rightNode;
        leftNodeBin = binOpNode.leftNodeBin;
        rightNodeBin = binOpNode.rightNodeBin;
        opToken = binOpNode.opToken;
    }

    public string Display() {
        string result = "(";
        string comma = ", ";
        if (leftNode != null) result += leftNode.Display() + comma;
        if (leftNodeBin != null) result += leftNodeBin.Display() + comma;
        if (opToken != null) result += opToken.DisplayOperator() + comma;
        if (rightNode!= null) result += rightNode.Display();
        if (rightNodeBin != null) result += rightNodeBin.Display();
        result += ")";

        return result;
    }
}

class Parser : pseudo {
    Token[] tokens;
    int tokenIndex = -1;

    Token currentToken;

    public Parser(Token[] tokens) {
        this.tokens = tokens;
        Advance();
    }

    public BinOpNode Parse() {
        BinOpNode result = Expr();
        return result;
    }

    void Advance() {
        tokenIndex++;
        if (tokenIndex < tokens.Length)
            currentToken = tokens[tokenIndex];
    }

    NumberNode Factor() {
        Token tok = currentToken;
        if(tok.type == TokenType.TT_INT || tok.type == TokenType.TT_FLOAT) {
            Advance();
            return new NumberNode(tok);
        }
        return null;
    }

    BinOpNode Term() {
        NumberNode num = Factor();
        BinOpNode left = new BinOpNode(num);
        while (currentToken.type == TokenType.MUL || currentToken.type == TokenType.DIV) {
            Token opToken = currentToken;
            Advance();
            NumberNode right = Factor();
            left = new BinOpNode(num, opToken, right);
        }
        return left;
    }

    BinOpNode Expr() {
        BinOpNode left = Term();
        while (currentToken.type == TokenType.PLUS || currentToken.type == TokenType.MINUS) {
            Token opToken = currentToken;
            Advance();
            BinOpNode right = Term();
            left = new BinOpNode(left, opToken, right);
        }
        return left;
    }
}

public class TokenError {
    public Token[] results;
    public string error;
    public BinOpNode ast;

    public TokenError(Token[] results, string error) {
        this.results = results;
        this.error = error;
    }
}

public class Position {

    public int index, line, column;
    public string fileName, fileTxt;

    public Position(int index, int line, int column, string fileName, string fileTxt) {
        this.index = index;
        this.line = line;
        this.column = column;
        this.fileName = fileName;
        this.fileTxt = fileTxt;
    }

    public void Advance(char currentChar) {
        index++;
        column++;
        if (currentChar == '\n') {
            line++;
            column = 0;
        }
    }
}

class Error : pseudo {

    string details;
    string errorName;
    Position posStart;
    Position posEnd;

    public Error(Position posStart, Position posEnd, string errorName, string details) {
        this.errorName = errorName;
        this.details = details;
        this.posStart= posStart;
        this.posEnd = posEnd;
    }

    public virtual string Display() {
        string res = $"{errorName}: {details}";
        res += $"\nFile: {posStart.fileName}, line: {posStart.line + 1}, col: {posStart.column}";
        return res;
    }
}

class IllegalCharError : Error {
    public IllegalCharError(Position posStart, Position posEnd, string details) : base(posStart, posEnd, "Illegal Character", details) { }
}

class InvalidSyntaxError :Error {
    public InvalidSyntaxError(Position posStart, Position posEnd, string details = "") : base(posStart, posEnd, "Invalid Syntax", details) { }
}

public class Token : pseudo {

    public TokenType type;
    int Ivalue;
    float Fvalue;

    Position posStart;

    public Token(TokenType type, int Ivalue, Position posStart = null, Position posEnd = null) {
        this.type = type;
        this.Ivalue = Ivalue;

        this.posStart = posStart;
    }

    public Token(TokenType type, float Fvalue, Position posStart = null, Position posEnd = null) {
        this.type = type;
        this.Fvalue = Fvalue;
    }

    public Token(TokenType type) {
        this.type = type;
    }

    public string DisplayOperator() {
        string displayText = $"{type}";
        return displayText;
    }

    public string Display() {
        string activeVar = Ivalue.ToString();
        if (type == TokenType.TT_FLOAT)
            activeVar = Fvalue.ToString();

        string displayText = $"{type}: {activeVar}";
        return displayText;
    }
}

class Lexer : pseudo {

    string text;
    Position pos;
    char currectChar;
    string fileName;

    public Lexer(string fileName, string text) {
        this.fileName = fileName;
        this.text = text;
        pos = new Position(-1, 0, -1, fileName, text);
        currectChar = '\0';
        Advance();
    }

    void Advance() {
        pos.Advance(currectChar);
        if (pos.index < text.Length)
            currectChar = text[pos.index];
        else
            currectChar = '\0';
    }

    public TokenError MakeTokens() {
        List<Token> tokens = new List<Token>();

        for (int i = 0; currectChar != '\0'; i++) {
            if (currectChar == ' ' || currectChar == '\t') {
                Advance();
            }
            else if (DIGITS.Contains(currectChar.ToString())) {
                tokens.Add(MakeNumber());
            }
            else if (currectChar == '+') {
                tokens.Add(new Token(TokenType.PLUS));
                Advance();
            }
            else if (currectChar == '-') {
                tokens.Add(new Token(TokenType.MINUS));
                Advance();
            }
            else if (currectChar == '*') {
                tokens.Add(new Token(TokenType.MUL));
                Advance();
            }
            else if (currectChar == '/') {
                tokens.Add(new Token(TokenType.DIV));
                Advance();
            }
            else if (currectChar == '(') {
                tokens.Add(new Token(TokenType.LPAREN));
                Advance();
            }
            else if (currectChar == ')') {
                tokens.Add(new Token(TokenType.RPAREN));
                Advance();
            }
            else {
                Position posStart = pos;
                char badChar = currectChar;
                Advance();
                return new TokenError(null, new IllegalCharError(posStart, pos, "'" + badChar + "'").Display());
            }
        }
        return new TokenError(tokens.ToArray(), null);
    }

    Token MakeNumber() {
        string numStr = "";
        int dot_count = 0;

        while (currectChar != '\0' && (DIGITS + ".").Contains(currectChar.ToString())) {
            if (currectChar == '.') {
                if (dot_count == 1) break;
                dot_count++;
                numStr += '.';
            }
            else
                numStr += currectChar;

            Advance();
        }

        if (dot_count == 0)
            return new Token(TokenType.TT_INT, int.Parse(numStr));
        else
            return new Token(TokenType.TT_FLOAT, float.Parse(numStr));
    }
}