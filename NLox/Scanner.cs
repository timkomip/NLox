using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class Scanner
    {
        private readonly static Dictionary<string, TokenType?> keywords = 
            new Dictionary<string, TokenType?>
            {
                { "and",    TokenType.And },
                { "class",  TokenType.Class },
                { "else",   TokenType.Else },
                { "false",  TokenType.False },
                { "for",    TokenType.For },
                { "fun",    TokenType.Fun },
                { "if",     TokenType.If },
                { "nil",    TokenType.Nil },
                { "or",     TokenType.Or },
                { "print",  TokenType.Print },
                { "return", TokenType.Return },
                { "super",  TokenType.Super },
                { "this",   TokenType.This },
                { "true",   TokenType.True },
                { "var",    TokenType.Var },
                { "while",  TokenType.While },
 
            };

        private readonly string source;
        private readonly IList<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 0;

        private bool IsAtEnd => current >= source.Length;

        public Scanner(string source)
        {
            this.source = source;
        }

        public IList<Token> ScanTokens()
        {
            while(!IsAtEnd)
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            var c = Advance();

            switch (c)
            {
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '-': AddToken(TokenType.Minus); break;
                case '+': AddToken(TokenType.Plus); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case '*': AddToken(TokenType.Star); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd) Advance();
                    } 
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                case '"': String(); break;
                default:
                    if (IsDigit(c))
                    {
                        Number();
                    } 
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    } 
                    else
                    {
                        Program.Error(line, "Unexpected character.");
                    }
                    break;

            }
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd)
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd)
            {
                Program.Error(line, "Unterminated string.");
                return;
            }

            Advance();

            var value = source.SubstringRange(start + 1, current - 1);
            AddToken(TokenType.String, value);
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.Number, double.Parse(source.SubstringRange(start, current)));
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            var text = source.SubstringRange(start, current);
            var type = keywords.GetValueOrDefault(text);
            AddToken(type ?? TokenType.Identifier);
        }

        private char Advance()
        {
            return source[current++];
        }

        private void AddToken(TokenType type, object literal = null)
        {
            var text = source.SubstringRange(start, current);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd) return '\0';
            return source[current];
        }
        
        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private bool IsAlpha(char c) =>
            (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            c == '_';

        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
    }
}
