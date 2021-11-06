using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public interface IVisitor<R>
    {
        R Visit<T>(T expr) where T : Expr;
    }

    public class AstPrinter : IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string Visit<T>(T expr) where T : Expr
        {
            return expr switch
            {
                Binary(Expr left, Token op, Expr right) => Parenthesize(nameof(Binary), left, right),
                Grouping(Expr expression) => Parenthesize(nameof(Grouping), expression),
                Literal(Expr expression) => Parenthesize(nameof(Literal), expression),
                Unary(Token token, Expr expression) => Parenthesize(nameof(Unary), expression),
                _ => throw new NotImplementedException(),
            };
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }

            builder.Append(")");

            return builder.ToString();
        }
    }
}
