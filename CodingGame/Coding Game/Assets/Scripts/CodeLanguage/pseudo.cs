using System;
using System.Collections.Generic;

public class pseudo {

    public enum TokenType {
        INT = 1,
        FLOAT,
        IDENTIFIER,
        KEYWORD,
        PLUS,
        MINUS,
        MUL,
        DIV,
        POWER,
        EQ,
        LPAREN,
        RPAREN,
        EOF
    }

    protected string[] KEYWORDS = {
        "VAR"
    };

    protected string DIGITS = "0123456789";
    protected string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    protected string LETTERS_DIGITS = "0123456789" + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" + "_";


    public RTResult Run(string fileName, string text) {
        Lexer lexer = new Lexer(fileName, text);

        TokenError tokenError = lexer.MakeTokens();

        if(tokenError.error != null) 
            return new RTResult(null, tokenError.error);

        // Generate Abstract Syntax Tree (AST)
        Parser parser = new Parser(tokenError.results);
        ParseResult parseResult = parser.Parse();
        if (parseResult.error != null)
            return new RTResult(null, parseResult.error);

        // else Run Program
        Interpreter interpreter = new Interpreter();
        Context context = new Context("<console>");
        RTResult result = interpreter.Visit(parseResult.node, context);

        return result;
    }
}

public class NumberNode {
    public Token token;

    public NumberNode(Token token) {
        this.token = token;
    }

    public string Display() {
        return token.Display();
    }
}

public class VarAccessNode {

    Token varNameToken;
    Position posStart;
    Position posEnd;

    public VarAccessNode(Token varNameToken) {
        this.varNameToken = varNameToken;

        posStart = this.varNameToken.posStart;
        posEnd = this.varNameToken.posEnd;
    }
}

public class VarAssignNode {

    Token varNameToken;
    dynamic valueNode;
    Position posStart;
    Position posEnd;

    public VarAssignNode(Token varNameToken, dynamic valueNode) {
        this.varNameToken = varNameToken;
        this.valueNode = valueNode;

        posStart = this.varNameToken.posStart;
        posEnd = this.varNameToken.posEnd;
    }
}

public class UnaryOpNode {

    public Token opToken;
    public dynamic node;

    public UnaryOpNode(Token opToken, dynamic node) {
        this.opToken = opToken;
        this.node = node;
    }

    public string Display() {
        return $"{opToken.Display()}, {node.Display()}";
    }
}

public class BinOpNode {

    public dynamic leftNode = null, rightNode = null;
    public Token opToken;

    public BinOpNode(dynamic leftNode, Token opToken, dynamic rightNode) {
        this.leftNode = leftNode;
        this.rightNode = rightNode;
        this.opToken = opToken;
    }

    public BinOpNode(BinOpNode binOpNode) {
        if (binOpNode == null)      // this is there because then it gives an error for null reference exception
            return;

        leftNode = binOpNode.leftNode;
        rightNode = binOpNode.rightNode;
        opToken = binOpNode.opToken;
    }

    public string Display() {
        string result = "(";
        string comma = ", ";
        if (leftNode != null) result += leftNode.Display() + comma;
        if (opToken != null) result += opToken.DisplayOperator() + comma;
        if (rightNode != null) result += rightNode.Display();
        result += ")";

        return result;
    }
}

public class ParseResult {
    public Error error;

    public dynamic node;

    public Number number;

    public ParseResult(Error error, dynamic node) {
        this.error = error;
        this.node = node;
    }

    public ParseResult(Error error, Number number) {
        this.error = error;
        this.number = number;
    }

    public dynamic Register(ParseResult parseResult) {
        if (parseResult.error != null) 
            error = parseResult.error;

        if (parseResult.node != null)
            return parseResult.node;

        return null;
    }

    public NumberNode Register(dynamic node) {
        return node;
    }

    public ParseResult Success(dynamic node) {
        this.node = node;
        return this;
    }

    public ParseResult Failure(Error error) {
        this.error = error;
        return this;
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

    public ParseResult Parse() {
        ParseResult result = Expr();
        if (result.error == null && currentToken.type != TokenType.EOF)
            return result.Failure(new InvalidSyntaxError(currentToken.posStart, currentToken.posEnd, "Expected +, -, * or /"));
        return result;
    }

    NumberNode Advance() {
        tokenIndex++;
        if (tokenIndex < tokens.Length)
            currentToken = tokens[tokenIndex].Copy();
        return new NumberNode(currentToken);
    }



    ParseResult Atom() {
        ParseResult parseResult = new ParseResult(null, (NumberNode)null);
        Token tok = currentToken;

        if (tok.type == TokenType.INT || tok.type == TokenType.FLOAT) {   // Integers and floats
            parseResult.Register(Advance());
            NumberNode number = new NumberNode(tok);
            return parseResult.Success(number);
        }

        else if(tok.type == TokenType.IDENTIFIER) {
            parseResult.Register(Advance());
            return parseResult.Success(new VarAccessNode(tok));
        }

        else if (tok.type == TokenType.LPAREN) {     // Brackets ( )
            parseResult.Register(Advance());
            dynamic expr = parseResult.Register(Expr());

            if (parseResult.error != null)
                return parseResult;

            if (currentToken.type == TokenType.RPAREN) {
                parseResult.Register(Advance());
                return parseResult.Success(expr);
            }
            else
                return parseResult.Failure(new InvalidSyntaxError(currentToken.posStart, currentToken.posEnd, "Expected ')'"));
        }
        // ERROR
        return parseResult.Failure(new InvalidSyntaxError(tok.posStart, tok.posEnd, "Expected int, float, +, -, or ("));
    }

    ParseResult Factor() {
        ParseResult parseResult = new ParseResult(null, (NumberNode)null);
        Token tok = currentToken;

        if(tok.type == TokenType.PLUS || tok.type == TokenType.MINUS) {     // Unary Operators (-, +)
            parseResult.Register(Advance());
            dynamic factor = parseResult.Register(Factor());

            if (parseResult.error != null)
                return parseResult;

            return parseResult.Success(new UnaryOpNode(tok, factor));
        }
        return binOperator(Atom, TokenType.POWER, TokenType.POWER, Factor);
    }

    ParseResult Term() {
        return binOperator(Factor, TokenType.MUL, TokenType.DIV);
    }

    ParseResult Expr() {
        ParseResult res = null;

        if (currentToken.Matches(TokenType.KEYWORD, "VAR")) {
            res.Register(Advance());

            if (currentToken.type != TokenType.IDENTIFIER)
                return res.Failure(new InvalidSyntaxError(currentToken.posStart, currentToken.posEnd, "Expected identifier"));

            dynamic varName = currentToken;
            res.Register(Advance());

            if(currentToken.type != TokenType.EQ)
                return res.Failure(new InvalidSyntaxError(currentToken.posStart, currentToken.posEnd, "Expected '='"));

            res.Register(Advance());
            ParseResult expr = res.Register(Expr());
            if (res.error != null)
                return res;

            return res.Success(new VarAssignNode(varName, expr));
        }

        return binOperator(Term, TokenType.MINUS, TokenType.PLUS);
    }

    ParseResult binOperator(Func<ParseResult> func, TokenType operator1, TokenType operator2 = 0, Func<ParseResult> func1 = null) {
        if (func1 == null)
            func1 = func;

        ParseResult parseResult = new ParseResult(null, (BinOpNode)null);
        dynamic left = parseResult.Register(func());

        if (parseResult.error != null)
            return parseResult;

        while (currentToken.type == operator1 || currentToken.type == operator2) {
            Token opToken = currentToken;
            parseResult.Register(Advance());
            dynamic right = parseResult.Register(func1());

            if (parseResult.error != null)
                return parseResult;

            left = new BinOpNode(left, opToken, right);
        }
        return parseResult.Success(left);
    }
}

public class TokenError {
    public Token[] results;
    public Error error;
    public BinOpNode ast;

    public TokenError(Token[] results, Error error) {
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

    public void Advance(char currentChar = '\0') {
        index++;
        column++;
        if (currentChar == '\n') {
            line++;
            column = 0;
        }
    }

    public Position Copy() {
        return new Position(index, line, column, fileName, fileTxt); 
    }
}

public class Error : pseudo {

    protected string details;
    protected string errorName;
    protected Position posStart;
    protected Position posEnd;

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

class RTError : Error {

    Context context;

    public RTError(Position posStart, Position posEnd, string details = "", Context context = null) : base(posStart, posEnd, "Runtime Error", details) { this.context = context; }

    public override string Display() {
        string result = "";
        result += GenerateTraceback();
        result += $"{errorName}: {details}\n";
        return result;
    }

    public string GenerateTraceback() {
        string result = "";
        Position pos = posStart;
        Context context = this.context;

        while (context != null) {
            result += $"File: {pos.fileName}, line {pos.line + 1}, in {context.displayName}\n";
            pos = context.parentEntryPos;
            context = context.parent;
        }
        return "Traceback (most recent call last):\n" + result;
    }  
}

public class RTResult {
    public Number value;
    public Error error;

    public RTResult(Number value = null, Error error = null) {
        this.value = value;
        this.error = error;
    }

    public Number Register(RTResult res) {
        if (res.error != null)
            error = res.error;

        return res.value;
    }

    public RTResult Success(Number value) {
        this.value = value;
        return this;
    }

    public RTResult Failure(Error error) {
        this.error = error;
        return this; 
    }
}

public class Context {

    public string displayName;
    public Context parent;
    public Position parentEntryPos;

    public Context(string displayName, Context parent = null, Position parentEntryPos = null) {
        this.displayName = displayName;
        this.parent = parent;
        this.parentEntryPos = parentEntryPos;
    }
}

public class SymbolTable {

    Dictionary<string, dynamic> symbols = new Dictionary<string, dynamic>();
    Context parent;

    public SymbolTable() {
        symbols = null;
        parent = null;
    }

    void Get(string name) {
        if (symbols.ContainsKey(name)) {
            dynamic value;
            value = symbols[name];
        }
    }
}

public class Token : pseudo {

    public TokenType type;
    public dynamic value;

    public Position posStart, posEnd;

    public Token(TokenType type, dynamic value, Position posStart = null, Position posEnd = null) {
        this.type = type;
        this.value = value;

        if (posStart != null) {
            this.posStart = posStart.Copy();
            this.posEnd = posStart.Copy();
            this.posEnd.Advance();
        }

        if (posEnd != null)
            this.posEnd = posEnd.Copy();
    }

    public Token(TokenType type, Position posStart = null, Position posEnd = null) {
        this.type = type;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }

    public string DisplayOperator() {
        string displayText = $"{type}";
        return displayText;
    }

    public Token Copy() {
        return this;
    }

    public bool Matches(TokenType type, dynamic value) {
        return this.type == type && value == this.value;
    }

    public string Display() {
        string activeVar = value.ToString();
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
            else if(LETTERS.Contains(currectChar.ToString())) {
                tokens.Add(MakeIdentifier());
            }
            else if (currectChar == '+') {
                tokens.Add(new Token(TokenType.PLUS, pos));
                Advance();
            }
            else if (currectChar == '-') {
                tokens.Add(new Token(TokenType.MINUS, pos));
                Advance();
            }
            else if (currectChar == '*') {
                tokens.Add(new Token(TokenType.MUL, pos));
                Advance();
            }
            else if (currectChar == '/') {
                tokens.Add(new Token(TokenType.DIV, pos));
                Advance();
            }
            else if (currectChar == '^') {
                tokens.Add(new Token(TokenType.POWER, pos));
                Advance();
            }
            else if (currectChar == '=') {
                tokens.Add(new Token(TokenType.EQ, pos));
                Advance();
            }
            else if (currectChar == '(') {
                tokens.Add(new Token(TokenType.LPAREN, pos));
                Advance();
            }
            else if (currectChar == ')') {
                tokens.Add(new Token(TokenType.RPAREN, pos));
                Advance();
            }
            else {
                Position posStart = pos;
                char badChar = currectChar;
                Advance();
                return new TokenError(null, new IllegalCharError(posStart, pos, "'" + badChar + "'"));
            }
        }
        tokens.Add(new Token(TokenType.EOF, pos));
        return new TokenError(tokens.ToArray(), null);
    }

    Token MakeNumber() {
        string numStr = "";
        int dot_count = 0;
        Position posStart = pos.Copy();

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
            return new Token(TokenType.INT, int.Parse(numStr), posStart, pos);
        else
            return new Token(TokenType.FLOAT, float.Parse(numStr), posStart, pos);
    }

    Token MakeIdentifier() {
        string idStr = "";
        Position posStart = pos.Copy();
        while (currectChar != '\0' && LETTERS_DIGITS.Contains(currectChar.ToString())) {
            idStr += currectChar;
            Advance();
        }

        TokenType type = FindInArray(idStr) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
        return new Token(type, idStr, posStart, pos);   
    }

    bool FindInArray(string b) {
        string a = Array.Find(KEYWORDS, element => element == b);
        return a != null;
    }
}