﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class ParseException : Exception { }

    public class Parser
    {
        private readonly IList<Token> tokens;
        private int current = 0;

        public Parser(IList<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseException e)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Equality();
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while(Match(TokenType.BangEqual, TokenType.EqualEqual))
            {
                var op = Previous();
                var right = Comparison();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd) current++;
            return Previous();
        }

        private bool IsAtEnd => Peek().Type == TokenType.EOF;

        private Token Peek() => tokens[current];

        private Token Previous() => tokens[current - 1];

        private Expr Comparison()
        {
            var expr = Term();

            while(Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var op = Previous();
                var right = Term();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term()
        {
            var expr = Factor();

            while(Match(TokenType.Minus, TokenType.Plus))
            {
                var op = Previous();
                var right = Factor();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Factor()
        {
            var expr = Unary();

            while(Match(TokenType.Slash, TokenType.Star))
            {
                var op = Previous();
                var right = Unary();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if(Match(TokenType.Bang, TokenType.Minus))
            {
                var op = Previous();
                var right = Unary();
                return new Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.False)) return new Literal(false);
            if (Match(TokenType.True)) return new Literal(true);
            if (Match(TokenType.Nil)) return new Literal(null);

            if (Match(TokenType.Number, TokenType.String)) return new Literal(Previous().Literal);

            if (Match(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(TokenType.RightParen, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression");
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        private ParseException Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseException();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd)
            {
                if (Previous().Type == TokenType.Semicolon) return;

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }

                Advance();
            }
        }
    }
}
