using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class Interpreter : IVisitor<object>
    {
        private LoxEnvironment globals;
        private LoxEnvironment environment;

        public LoxEnvironment Globals => globals;
        public LoxEnvironment Environment => environment;

        public Interpreter()
        {
            globals = new LoxEnvironment();
            environment = globals;

            globals.Define("clock", new CallableFunc((interpreter, arguments) => 
            {
                return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }, 0));
            
            globals.Define("uppercase", new CallableFunc((interpreter, arguments) => 
            {
                var arg = arguments.FirstOrDefault();
                return arg is string str ? str.ToUpper() : null;
            }, 1));
        }

        public object Visit<T>(T expr)
        {
            return expr switch
            {
                ExpressionStmt stmt => Evaluate(stmt.Expr),
                PrintStmt stmt => VisitPrintStmt(stmt),
                VarStmt stmt => VisitVarStmt(stmt),
                BlockStmt stmt => VisitBlockStmt(stmt),
                FunctionStmt stmt => VisitFunctionStmt(stmt),
                IfStmt stmt => VisitIfStmt(stmt),
                WhileStmt stmt => VisitWhileStmt(stmt),
                Variable exp => environment.Get(exp.Name),
                Assign exp => VisitAssignExpr(exp),
                Literal exp => exp.Value,
                Grouping exp => Evaluate(exp.Expression),
                Unary exp => VisitUnary(exp),
                Binary exp => VisitBinary(exp),
                Logical exp => VisitLogicalExpr(exp),
                Call exp => VisitCallExpr(exp),
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
        
        private object VisitBlockStmt(BlockStmt stmt)
        {
            ExecuteBlock(stmt.Statements, new LoxEnvironment(environment));
            return null;
        }

        private object VisitIfStmt(IfStmt stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return null;
        }

        private object VisitWhileStmt(WhileStmt stmt)
        {
            while(IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return null;
        }

        private object VisitPrintStmt(PrintStmt stmt)
        {
            var value = Evaluate(stmt.Expr);
            Console.WriteLine(value);
            return null;
        }

        private object VisitFunctionStmt(FunctionStmt stmt)
        {
            var func = new CallableFunc(stmt);
            environment.Define(stmt.Name.Lexeme, func);
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

        private object VisitLogicalExpr(Logical exp)
        {
            var left = Evaluate(exp.Left);

            if (exp.Op.Type == TokenType.Or)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(exp.Right);
        }

        private object VisitCallExpr(Call exp)
        {
            var callee = Evaluate(exp.Callee);

            var arguments = new List<object>();
            foreach (var arg in exp.Arguments)
            {
                arguments.Add(Evaluate(arg));
            }

            if (callee is ICallable function)
            {
                if (arguments.Count != function.Arity) throw new RuntimeException(
                    exp.Paren,
                    $"Expected {function.Arity} arguments but got {arguments.Count}");
                return function.Call(this, arguments);
            }

            throw new RuntimeException(
                exp.Paren,
                "Can only call functions and classes.");
        }

        private void Execute(Stmt stmt) => stmt.Accept(this);
        private object Evaluate(Expr expr) => expr.Accept(this);

        public void ExecuteBlock(IList<Stmt> statements, LoxEnvironment environment)
        {
            var previous = this.environment;

            try
            {
                this.environment = environment;

                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            } 
            finally
            {
                this.environment = previous;
            }
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
