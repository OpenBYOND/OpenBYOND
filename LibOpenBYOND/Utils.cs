using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace OpenBYOND
{
    public class Utils
    {
        private static Dictionary<string,Type> simplifiableTypes = new Dictionary<string,Type>();
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));

        static Utils()
        {
            simplifiableTypes.Add("number", typeof(int));
            simplifiableTypes.Add("float", typeof(float));
            simplifiableTypes.Add("string", typeof(string));
        }

        public static object SimplifyProperty(Type type, World.Atom a)
        {
            if(type == typeof(int))
                return AtomToInt(a);
            if(type == typeof(float))
                return AtomToFloat(a);
            if(type == typeof(string))
                return AtomToString(a);
            if(type == typeof(World.Atom))
                return a;

            log.WarnFormat("Unable to simplify type {0}.", type.FullName);
            return a;
        }

        private static object AtomToString(World.Atom a)
        {
            throw new NotImplementedException();
        }

        private static object AtomToFloat(World.Atom a)
        {
            throw new NotImplementedException();
        }

        private static object AtomToInt(World.Atom a)
        {
            throw new NotImplementedException();
        }
    }
}
