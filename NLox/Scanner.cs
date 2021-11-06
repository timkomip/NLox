using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    public class Scanner
    {
        private readonly string source;
        private readonly IList<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 0;

        private bool IsAtEnd => current >= source.Length;

        public Scanner(string source)
        {
            this.source = source;
        }

        public IList<Token> ScanTokens()
        {
            while(!IsAtEnd)
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {

        }
    }
}
