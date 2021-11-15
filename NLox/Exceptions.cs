using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class ParseException : Exception { }
    public class RuntimeException : Exception
    {
        private readonly Token token;

        public Token Token => token;

        public RuntimeException(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
