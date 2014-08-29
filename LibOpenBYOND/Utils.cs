using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using OpenBYOND.VM;

namespace OpenBYOND
{
    public class Utils
    {
        private static Dictionary<string, Type> simplifiableTypes = new Dictionary<string, Type>();
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));

        static Utils()
        {
            simplifiableTypes.Add("number", typeof(int));
            simplifiableTypes.Add("float", typeof(float));
            simplifiableTypes.Add("string", typeof(string));
        }

        public static object SimplifyProperty(Type type, VM.Atom a)
        {
            if (type == typeof(int))
                return AtomToInt(a);
            if (type == typeof(float))
                return AtomToFloat(a);
            if (type == typeof(string))
                return AtomToString(a);
            if (type == typeof(VM.Atom))
                return a;

            log.WarnFormat("Unable to simplify type {0}.", type.FullName);
            return a;
        }

        private static object AtomToString(VM.Atom a)
        {
            return ((BYONDString)a).Value;
        }

        private static object AtomToFloat(VM.Atom a)
        {
            return ((BYONDValue<float>)a).Value;
        }

        private static object AtomToInt(VM.Atom a)
        {
            return ((BYONDValue<int>)a).Value;
        }

        /// <summary>
        /// WHAR MY OPERATOR EXTENSION C#
        /// WHAR
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<T> ConcatLists<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            var newlist = new List<T>(a);
            newlist.AddRange(b);
            return newlist;
        }
    }
}
