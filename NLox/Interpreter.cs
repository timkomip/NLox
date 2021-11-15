using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class Interpreter : IVisitor<object>
    {
        private LoxEnvironment environment = new LoxEnvironment();

        public object Visit<T>(T expr)
        {
            return expr switch
            {
                ExpressionStmt stmt => Evaluate(stmt.Expr),
                PrintStmt stmt => VisitPrintStmt(stmt),
                VarStmt stmt => VisitVarStmt(stmt),
                Variable exp => environment.Get(exp.Name),
                Assign exp => VisitAssignExpr(exp),
                Literal exp => exp.Value,
                Grouping exp => Evaluate(exp.Expression),
                Unary exp => VisitUnary(exp),
                Binary exp => VisitBinary(exp),
                _ => throw new NotImplementedException()
            };
        }

        public void Interpret(IList<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeException error)
            {
                Program.RuntimeError(error);
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

        private void Execute(Stmt stmt) => stmt.Accept(this);
        private object Evaluate(Expr expr) => expr.Accept(this);

        private object VisitVarStmt(VarStmt stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            environment.Define(stmt.Name.Lexeme, value);
            return null;
        }

        private object VisitPrintStmt(PrintStmt stmt)
        {
            var value = Evaluate(stmt.Expr);
            Console.WriteLine(value);
            return null;
        }

        private object VisitAssignExpr(Assign exp)
        {
            var value = Evaluate(exp.Value);
            environment.Assign(exp.Name, value);
            return value;
        }

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
