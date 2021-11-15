﻿namespace NLox
{
    public abstract record Expr()
    {
        public R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }
    };
    public record Assign(Token Name, Expr Value) : Expr;
    public record Binary(Expr Left, Token Op, Expr Right) : Expr;
    public record Grouping(Expr Expression) : Expr;
    public record Literal(object Value) : Expr;
    public record Unary(Token Op, Expr Right) : Expr;
    public record Variable(Token Name) : Expr;
}
