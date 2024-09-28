using System.Collections.Generic;
using System.Linq;

namespace MDO2.Core.Util
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }
        public static string ConcatStringBy(this IEnumerable<string> strings, string seperatorPattern = "/", bool removeNullOrWhiteSpaceEntries = true)
        {
            if (seperatorPattern.IsNullOrWhiteSpace())
            {
                seperatorPattern = "/";
            }

            if (strings == null)
            {
                return string.Empty;
            }
            else
            {
                IEnumerable<string> sary = strings;
                if (removeNullOrWhiteSpaceEntries)
                {
                    sary = strings.Where(x => !string.IsNullOrWhiteSpace(x));
                }

                var concat = string.Join(seperatorPattern, sary.ToArray());
                return concat;
            }
        }
        public static string CheckNullOrWhiteSpace(this string text, string returnIfNullOrWhiteSpace)
        {
            if (string.IsNullOrWhiteSpace(text)) return returnIfNullOrWhiteSpace;
            else return text;
        }
        public static bool IsMatchWithIgnoreCase(this string s, string with)
        {
            if (s == with) return true;
            else if (s?.ToLower() == with?.ToLower()) return true;
            else return false;
        }
        public static bool IsContainWithIgnoreCase(this string s, string with)
        {
            if (s == with) return true;
            else if (s?.ToLower().Contains(with?.ToLower()) ?? false) return true;
            else return false;
        }
        public static bool IsBeingWithIgnoreCase(this string s, string with)
        {
            if (s == with) return true;
            else if (s?.ToLower().StartsWith(with?.ToLower()) ?? false) return true;
            else return false;
        }
        public static bool IsEndWithIgnoreCase(this string s, string with)
        {
            if (s == with) return true;
            else if (s?.ToLower().EndsWith(with?.ToLower()) ?? false) return true;
            else return false;
        }
    }
}
