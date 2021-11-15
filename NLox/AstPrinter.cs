using System;
using System.Text;

namespace NLox
{
    public class AstPrinter : IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string Visit<T>(T expr)
        {
            return expr switch
            {
                Binary exp => Parenthesize(exp.Op.Lexeme, exp.Left, exp.Right),
                Grouping exp => Parenthesize("group", exp),
                Literal exp => exp.Value == null ? "nil" : exp.Value.ToString(),
                Unary exp => Parenthesize(exp.Op.Lexeme, exp.Right),
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
