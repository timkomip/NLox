using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NLox
{
    public interface IVisitor<R>
    {
        R Visit<T>(T expr) where T : Expr;
    }
}
