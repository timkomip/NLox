using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public abstract record Stmt()
    {
        public R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }
    };
    public record ExpressionStmt(Expr Expr) : Stmt;
    public record PrintStmt(Expr Expr): Stmt; public record VarStmt(Token Name, Expr Initializer) : Stmt;
    public record BlockStmt(IList<Stmt> Statements) : Stmt;
    public record FunctionStmt(Token Name, IList<Token> Params, IList<Stmt> Body) : Stmt;
    public record IfStmt(Expr Condition, Stmt ThenBranch, Stmt ElseBranch) : Stmt;
    public record WhileStmt(Expr Condition, Stmt Body) : Stmt;
} 