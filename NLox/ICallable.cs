using System;
using System.Collections.Generic;
using System.Linq;

namespace NLox
{
    public interface ICallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, IList<object> arguments);
    }

    public class CallableFunc : ICallable
    {
        private readonly Func<Interpreter, object[], object> func;
        public int Arity { get; }

        public CallableFunc(Func<Interpreter, IList<object>, object> func, int arity)
        {
            this.func = func;
            this.Arity = arity;
        }

        public object Call(Interpreter interpreter, IList<object> arguments) =>
            func.Invoke(interpreter, arguments.ToArray());

        public override string ToString() => "<native fn>";
    }
}
