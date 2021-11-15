using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class RuntimeException : Exception
    {
        private readonly Token token;

        public Token Token => token;

        public RuntimeException(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
    public class Interpreter : IVisitor<object>
    {
        public object Visit<T>(T expr) where T : Expr
        {
            return expr switch
            {
                Literal exp => exp.Value,
                Grouping exp => Evaluate(exp.Expression),
                Unary exp => VisitUnary(exp),
                Binary exp => VisitBinary(exp),
                _ => throw new NotImplementedException()
            };
        }

        public string Interpret(Expr expression)
        {
            try
            {
                var value = Evaluate(expression);
                return Stringify(value);
            }
            catch (RuntimeException error)
            {
                Program.RuntimeError(error);
                return null;
            }
        }

        private string Stringify(object obj)
        {
            if (obj == null)
            {
                return "nil";
            }
            else if (obj is double d)
            {
                var str = d.ToString();
                if (str.EndsWith(".0"))
                {
                    return str.Split('.').First();
                }
                return str;
            }
            return obj.ToString();
        }

        private object Evaluate(Expr expr) => expr.Accept(this);

        private object VisitUnary(Unary expr)
        {
            var right = Evaluate(expr);
            return expr.Op.Type switch
            {
                TokenType.Bang => !IsTruthy(right),
                TokenType.Minus => -(double)right,
                _ => null
            };
        }

        private object VisitBinary(Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);
            return expr.Op.Type switch
            {
                TokenType.Greater => (left, right) switch
                {
                    (double l, double r) => l > r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.GreaterEqual => (left, right) switch
                {
                    (double l, double r) => l >= r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.Less => (left, right) switch
                {
                    (double l, double r) => l < r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.LessEqual => (left, right) switch
                {
                    (double l, double r) => l !<= r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.BangEqual => (left, right) switch
                {
                    (double l, double r) => l != r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.EqualEqual => (left, right) switch
                {
                    (double l, double r) => l == r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.Minus => (left, right) switch
                {
                    (double l, double r) => l - r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.Plus => (left, right) switch
                {
                    (string l, string r) => l + r,
                    (double l, double r) => l + r,
                    _ => throw new RuntimeException(expr.Op, "Both operands must be a numbers or strings"),
                },
                TokenType.Slash => (left, right) switch
                {
                    (double l, double r) => l / r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                TokenType.Star => (left, right) switch
                {
                    (double l, double r) => l * r,
                    _ => throw new RuntimeException(expr.Op, "Operands must be a number"),
                },
                _ => null
            };
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool b) return b;
            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a == b;
        }
    }
}
