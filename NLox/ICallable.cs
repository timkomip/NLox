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
        private readonly FunctionStmt declaration;
        public int? arity;
        public int Arity => declaration?.Params.Count ?? (arity ?? 0);

        public CallableFunc(FunctionStmt declaration)
        {
            this.declaration = declaration;
        }

        public CallableFunc(Func<Interpreter, IList<object>, object> func, int arity)
        {
            this.func = func;
            this.arity = arity;
        }

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            if (func != null)
            {
                return func.Invoke(interpreter, arguments.ToArray());
            }
            else if (declaration != null)
            {
                var env = new LoxEnvironment(interpreter.Globals);
                foreach (var (param, index) in declaration.Params.Select((value, i) => (value, i)))
                {
                    env.Define(param.Lexeme, arguments[index]);
                }

                interpreter.ExecuteBlock(declaration.Body, env);
                return null;
            }

            throw new Exception("CallableFunc is in an invalid state");
        }

        public override string ToString() => $"<fun {declaration?.Name?.Lexeme ?? "[native]"}>";
    }
}
