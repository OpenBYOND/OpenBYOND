using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND
{
    public static class StringExtensions
    {
        /// <summary>
        /// Python-style join.
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="E"></param>
        /// <returns></returns>
        public static string join(this string separator, IEnumerable<string> E)
        {
            return string.Join(separator, E);
        }
        //// str - the source string
        //// index- the start location to replace at (0-based)
        //// length - the number of characters to be removed before inserting
        //// replace - the string that is replacing characters
        public static string ReplaceAt(this string str, int index, int length, string replace)
        {
            return str.Remove(index, Math.Min(length, str.Length - index))
                    .Insert(index, replace);
        }
    }

    public static class ListExtensions
    {
        public static void Prepend<T>(this List<T> L, T i)
        {
            L.Insert(0, i);
        }
    }
}
