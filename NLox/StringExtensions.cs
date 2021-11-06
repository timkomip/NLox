using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLox
{
    static class StringExtensions
    {
        public static string SubstringRange(this string str, int start, int end)
        {
            return str.Substring(start, end - start);
        }
    }
}
