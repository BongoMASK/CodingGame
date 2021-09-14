using UnityEngine;
using System.Text;

public class pseudo
{
    public enum TokenType {
        TT_INT = 0,
        TT_FLOAT,
        TT_PLUS,
        TT_MINUS,
        TT_MUL,
        TT_DIV,
        TT_LPAREN,
        TT_RPAREN
    }

    protected string DIGITS = "0123456789";

    public TokenError Run(string text) {
        Lexer lexer = new Lexer(text);

        TokenError tokenError = lexer.MakeTokens();
        return tokenError;
    }
}

public class TokenError {
    public string result;
    public string error;

    public TokenError(string result, string error) {
        this.result = result;
        this.error = error;
    }
}

class Error : pseudo {
    
    string details;
    string errorName;

    public Error (string errorName, string details) {
        this.errorName = errorName;
        this.details = details;
    }

    public virtual string Display() {
        string res = $"{errorName}: {details}";
        return res;
    }
}

class IllegalCharError : Error {
    public IllegalCharError(string details) : base("Illegal Character", details) { }

    public override string Display() {
        return base.Display();
    }
}

class Token<T> : pseudo {
    
    int type;
    T value;

    public Token(TokenType type, T value) {
        this.type = (int)type;
        this.value = value;
    }

    public Token(TokenType type) {
        this.type = (int)type;
    }

    public string Display() {
        string displayText = $"{nameof(type)}";
        if(value != null)
            displayText = $"{nameof(type)}:{value}";
        return displayText;
    }
}

class Lexer : pseudo {

    string text;
    int pos;
    char currectChar;

    public Lexer(string text) {
        this.text = text;
        pos = -1;
        currectChar = '\0';
        Advance();
    }

    void Advance() {
        pos++;
        if (pos > text.Length)
            currectChar = text[pos];
        else
            currectChar = '\0';
    }

    public TokenError MakeTokens() {
        StringBuilder tokens = new StringBuilder("");

        while (currectChar != '\0') {
            if (" \t".Contains(currectChar.ToString()))
                Advance();
            else if (DIGITS.Contains(currectChar.ToString())) {
                tokens.Append(MakeNumber().Display());
                Advance();
            }
            else if (currectChar == '+') {
                tokens.Append(new Token<char>(TokenType.TT_PLUS).Display());
                Advance();
            }
            else if (currectChar == '-') {
                tokens.Append(new Token<char>(TokenType.TT_MINUS).Display());
                Advance();
            }
            else if (currectChar == '*') {
                tokens.Append(new Token<char>(TokenType.TT_MUL).Display());
                Advance();
            }
            else if (currectChar == '/') {
                tokens.Append(new Token<char>(TokenType.TT_DIV).Display());
                Advance();
            }
            else if (currectChar == '(') {
                tokens.Append(new Token<char>(TokenType.TT_LPAREN).Display());
                Advance();
            }
            else if (currectChar == ')') {
                tokens.Append(new Token<char>(TokenType.TT_RPAREN).Display());
                Advance();
            }
            else {
                char badChar = currectChar;
                Advance();
                return new TokenError(null, new IllegalCharError("'" + badChar + "'").Display());
            }
        }

        Debug.Log(tokens);
        return new TokenError(tokens.ToString(), null);
    }

    Token<float> MakeNumber() {
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
            return new Token<float>(TokenType.TT_INT, int.Parse(numStr));
        else
            return new Token<float>(TokenType.TT_FLOAT, float.Parse(numStr));
    }
}