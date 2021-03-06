using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class Parser
    {
        private readonly IList<Token> tokens;
        private int current = 0;

        public Parser(IList<Token> tokens)
        {
            this.tokens = tokens;
        }

        public IList<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd)
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.Fun)) return Function("function");
                if (Match(TokenType.Var)) return VarDeclaration();

                return Statement();
            }
            catch (ParseException e)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect variable name.");

            Expr initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after variable declartion.");
            return new VarStmt(name, initializer);
        }

        private Stmt Function(string kind)
        {
            var name = Consume(TokenType.Identifier, $"Expect {kind} name.");
            Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");

            var parameters = new List<Token>();

            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters");
                    }

                    parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
                }
                while(Match(TokenType.Comma));
            }
            Consume(TokenType.RightParen, "Expect ')' after parameters.");

            Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
            var body = Block();
            return new FunctionStmt(name, parameters, body);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.If)) return IfStatement();
            if (Match(TokenType.Print)) return PrintStatement();
            if (Match(TokenType.While)) return WhileStatement();
            if (Match(TokenType.LeftBrace)) return new BlockStmt(Block());
            return ExpressionStatement();
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expect ')' after if condition.");

            var thenBranch = Statement();
            Stmt elseBranch = null;

            if (Match(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new IfStmt(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after value.");
            return new PrintStmt(value);
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
            var condition = Expression();
            Consume(TokenType.RightParen, "Expect ')' after if condition.");
            var body = Statement();

            return new WhileStmt(condition, body);
        }

        private Stmt ExpressionStatement()
        {
            var value = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after expression.");
            return new ExpressionStmt(value);
        }

        private IList<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RightBrace) && !IsAtEnd)
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RightBrace, "Expect '}' after block.");
            return statements;
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            var expr = Or();

            if (Match(TokenType.Equal))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr is Variable varExpr)
                {
                    var name = varExpr.Name;
                    return new Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.Or))
            {
                var op = Previous();
                var right = And();
                expr = new Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while (Match(TokenType.And))
            {
                var op = Previous();
                var right = Equality();
                expr = new Logical(expr, op, right);
            }

            return expr;
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

            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();

            while(true)
            {
                if (Match(TokenType.LeftParen))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");

            return new Call(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.False)) return new Literal(false);
            if (Match(TokenType.True)) return new Literal(true);
            if (Match(TokenType.Nil)) return new Literal(null);

            if (Match(TokenType.Number, TokenType.String)) return new Literal(Previous().Literal);

            if (Match(TokenType.Identifier)) return new Variable(Previous());

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
