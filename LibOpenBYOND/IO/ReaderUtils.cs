using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenBYOND.IO
{
    public abstract class CharMatcher
    {
        public abstract bool Matches(char c);

        public static CharMatcherOrGroup operator |(CharMatcher a, CharMatcher b)
        {
            var cmg = new CharMatcherOrGroup();
            cmg.Matchers.Add(a);
            cmg.Matchers.Add(b);
            return cmg;
        }

        public static CharMatcherOrGroup operator |(CharMatcher a, string b)
        {
            var cmg = new CharMatcherOrGroup();
            cmg.Matchers.Add(a);
            cmg.Matchers.Add(new ConstCharMatcher(b));
            return cmg;
        }

        public static CharMatcherAndGroup operator &(CharMatcher a, CharMatcher b)
        {
            var cmg = new CharMatcherAndGroup();
            cmg.Matchers.Add(a);
            cmg.Matchers.Add(b);
            return cmg;
        }

        public static CharMatcherAndGroup operator &(CharMatcher a, string b)
        {
            var cmg = new CharMatcherAndGroup();
            cmg.Matchers.Add(a);
            cmg.Matchers.Add(new ConstCharMatcher(b));
            return cmg;
        }
    }

    public abstract class CharMatcherGroup : CharMatcher
    {
        public List<CharMatcher> Matchers = new List<CharMatcher>();

        public static CharMatcherGroup operator |(CharMatcherGroup a, CharMatcher b)
        {
            a.Matchers.Add(b);
            return a;
        }

        public static CharMatcherGroup operator |(CharMatcherGroup a, string b)
        {
            a.Matchers.Add(new ConstCharMatcher(b));
            return a;
        }

        public static CharMatcherGroup operator &(CharMatcherGroup a, CharMatcher b)
        {
            a.Matchers.Add(b);
            return a;
        }

        public static CharMatcherGroup operator &(CharMatcherGroup a, string b)
        {
            a.Matchers.Add(new ConstCharMatcher(b));
            return a;
        }
    }

    public class CharMatcherOrGroup : CharMatcherGroup
    {
        public override bool Matches(char c)
        {
            foreach (CharMatcher m in Matchers)
            {
                if (m.Matches(c)) return true;
            }
            return false;
        }
    }

    public class CharMatcherAndGroup : CharMatcherGroup
    {
        public override bool Matches(char c)
        {
            foreach (CharMatcher m in Matchers)
            {
                if (!m.Matches(c)) return false;
            }
            return true;
        }
    }

    public class ConstCharMatcher : CharMatcher
    {
        public char[] chars;
        public ConstCharMatcher(string str)
        {
            chars = str.ToCharArray();
        }
        public ConstCharMatcher(char c)
        {
            chars = new char[1] { c };
        }
        public override bool Matches(char c)
        {
            return chars.Contains(c);
        }
    }

    public class CharRange : CharMatcher
    {
        public char Start;
        public char End;

        public CharRange(char start, char end)
        {
            this.Start = start;
            this.End = end;
        }

        public static CharRange Build(char start, char end)
        {
            return new CharRange(start, end);
        }

        public override bool Matches(char c)
        {
            return c >= this.Start && c <= this.End;
        }
    }

    public class ReaderUtils
    {
        public static string ReadUntil(TextReader rdr, char c)
        {
            string buf = "";
            while (true)
            {
                char cc = (char)rdr.Read();
                if (cc == c)
                {
                    return buf;
                }
                buf += cc;
            }
        }
        public static string ReadScriptUntil(TextReader rdr, out char lc, string stringChars = "\"", char escapeChar = '/', params char[] cl)
        {
            string buf = "";
            char? endStringWith = null;
            char? lastChar = null;
            while (true)
            {
                char cc = (char)rdr.Read();
                lc = cc;
                if (endStringWith == null)
                {
                    if (lastChar != escapeChar)
                    {
                        if (stringChars.Contains(cc))
                        {
                            endStringWith = cc;
                        }
                        else if (cl.Contains(cc))
                        {
                            return buf;
                        }
                    }
                }
                else
                {
                    if (lastChar != escapeChar)
                    {
                        if (endStringWith == cc)
                        {
                            endStringWith = null;
                        }
                    }
                }
                buf += cc;
                lastChar = cc;
            }
        }

        internal static char GetNextChar(TextReader rdr)
        {
            return (char)rdr.Peek();
        }

        internal static string ReadCharRange(TextReader rdr, CharMatcher matcher)
        {
            string buf = "";
            while (true)
            {
                char c = (char)rdr.Peek();
                if (!matcher.Matches(c))
                    return buf;
                rdr.Read();
                buf += c;
            }
        }
    }
}
