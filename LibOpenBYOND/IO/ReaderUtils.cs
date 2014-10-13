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

        public override string ToString()
        {
            return "(" + (string.Join<CharMatcher>(" OR ", Matchers)) + ")";
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

        public override string ToString()
        {
            return "(" + (string.Join<CharMatcher>(" AND ", Matchers)) + ")";
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

        public override string ToString()
        {
            return new string(chars);
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

        public override string ToString()
        {
            return Start + "-" + End;
        }
    }

    public class ReaderUtils
    {
        public static string ReadUntil(TextReader rdr, params char[] c)
        {
            string buf = "";
            while (true)
            {
                int ci = rdr.Read();
                if (ci == -1) throw new EndOfStreamException();
                char cc = (char)ci;
                if (c.Contains(cc))
                {
                    return buf;
                }
                buf += cc;
            }
        }
        public static string ReadScriptUntil(TextReader rdr, string stringChars, string escapeChars, string endChars, out char lc)
        {
            string buf = "";
            char? endStringWith = null;
            char? lastChar = null;
            while (true)
            {
                int ci = rdr.Read();
                if (ci == -1) throw new EndOfStreamException();
                char cc = (char)ci;
                lc = cc;
                if (lastChar != null && !escapeChars.Contains((char)lastChar))
                {
                    if (endStringWith == null)
                    {
                        if (stringChars.Contains(cc))
                        {
                            endStringWith = cc;
                        }
                        else if (endChars.Contains(cc))
                        {
                            return buf;
                        }

                    }
                    else
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
            int ci = rdr.Peek();
            if (ci > -1)
                return (char)ci;
            throw new EndOfStreamException();
        }

        private static char ReadAChar(TextReader rdr)
        {
            int ci = rdr.Read();
            if (ci > -1)
                return (char)ci;
            throw new EndOfStreamException();
        }

        internal static string ReadCharRange(TextReader rdr, CharMatcher matcher, bool debug=false)
        {
            string buf = "";
            //Console.WriteLine("Read Char Range in {0}", matcher);
            while (true)
            {
                char c = GetNextChar(rdr);
                if (!matcher.Matches(c))
                {
                    if(debug)
                        Console.WriteLine(" Char '{0}' not in range {2}. Returning {1}.", c, buf, matcher);
                    return buf;
                }
                rdr.Read();
                buf += c;
            }
        }
    }
}
