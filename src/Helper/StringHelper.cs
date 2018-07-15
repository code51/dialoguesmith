using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DialogueSmith.Helper
{
    public class StringHelper
    {





        // http://www.dosomethinghere.com/2013/02/17/net-regex-to-find-strings-inside-curly-braces/
        public static List<string> CurlyExtracts(string text)
        {
            List<string> texts = new List<string>();

            foreach (Match match in Regex.Matches(text, @"\{([^\}]+)\}")) {
                texts.Add(match.ToString());
            }

            return texts;
        }
    }
}
