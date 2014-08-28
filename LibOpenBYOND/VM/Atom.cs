using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using log4net;

namespace OpenBYOND.VM
{
    internal class NativeAtomProperty
    {
        public string DMProperty;
        public PropertyInfo CSProperty;
        private Atom atom;

        public NativeAtomProperty(Atom atom)
        {
            this.atom = atom;
        }

        public void Set(object value)
        {
            CSProperty.SetValue(atom, value, new object[0]);
        }

        public void Get()
        {
            CSProperty.GetValue(atom, new object[0]);
        }
    }

    /// <summary>
    /// Your basic object in the world.  _Can_ have a loc.  Turfs also count.
    /// </summary>
    public class Atom
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));

        /// <summary>
        /// ID of the instance.
        /// </summary>
        public int ID;

        /// <summary>
        /// Use the GetProperty function to get this as a simple type.
        /// </summary>
        public Dictionary<string, Atom> Properties = new Dictionary<string, Atom>();

        /// <summary>
        /// Used for initial().  Copied after compile.
        /// </summary>
        public Dictionary<string, Atom> InitialProperties = new Dictionary<string, Atom>();

        /// <summary>
        /// DO NOT FUCK WITH THIS.
        /// 
        /// Set by AtomPropertyAttribute.
        /// </summary>
        private Dictionary<string, NativeAtomProperty> NativeProperties = new Dictionary<string, NativeAtomProperty>();

        /// <summary>
        /// Yes, type in BYOND is mutable.  How fucking dumb.
        /// </summary>
        public BYONDType Type = null;

        /// <summary>
        /// File this was defined in.
        /// </summary>
        public string Filename = "";

        /// <summary>
        /// Line of file this was defined in.
        /// </summary>
        public int Line = 0;

        internal Atom()
            : this("", false)
        {
            // Used internally for simple types.  Don't use this or I will slap you.
        }

        /// <summary>
        /// Instantiate an atom.
        /// </summary>
        /// <param name="makeInitial">Create InitialProperties?</param>
        public Atom(string path, bool makeInitial = true)
        {
            this.Type = new BYONDType(path);

            if (makeInitial)
                InitialProperties = new Dictionary<string, Atom>(Properties); // Make copy.

            // Build NativeProperties mappings
            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                foreach (AtomPropertyAttribute apa in prop.GetCustomAttributes(typeof(AtomPropertyAttribute), true))
                {
                    NativeAtomProperty nap = new NativeAtomProperty(this);
                    nap.CSProperty = prop;
                    nap.DMProperty = apa.Name != null ? apa.Name : prop.Name;
                    NativeProperties[nap.DMProperty] = nap;

                    log.DebugFormat("Mapped C# property {0} to atom property {1}.", prop.Name, nap.DMProperty);
                }
            }
        }

        public Atom(string npath, string filename, int ln)
            : this(npath)
        {
            this.Filename = filename;
            this.Line = ln;
        }

        /// <summary>
        /// Get a property as a simpler type.  
        /// 
        /// Throws exception if you fuck up.
        /// </summary>
        /// <typeparam name="T">Simple type to get (int, float, string)</typeparam>
        /// <param name="key">Name of property.</param>
        /// <returns></returns>
        public T GetProperty<T>(string key)
        {
            Atom a = null;
            if (!Properties.TryGetValue(key, out a))
            {
                throw new KeyNotFoundException(string.Format("Property {0} does not exist in {1}.", key, GetName()));
            }
            return (T)Utils.SimplifyProperty(typeof(T), a);
        }

        // I don't know why I made this.
        public T GetProperty<T>(string key, T defaultValue)
        {
            Atom a = null;
            if (!Properties.TryGetValue(key, out a))
            {
                return defaultValue;
            }
            return (T)Utils.SimplifyProperty(typeof(T), a);
        }

        /// <summary>
        /// Set a property, given a simple type.
        /// </summary>
        /// <typeparam name="T">Simple type (int, float, string)</typeparam>
        /// <param name="key">Property to set.</param>
        /// <param name="val">Value</param>
        public void SetProperty<T>(string key, T val)
        {
            Atom a = null;
            if (val.GetType() == typeof(Atom))
            {
                object o = val;
                a = (Atom)o;
            }
            else
            {
                a = new BYONDValue<T>(val);
            }

            Properties[key] = a;
        }

        public string GetName()
        {
            // Emulate BYOND behavior.  (ID instead of memory address)
            // TODO: Proper/improper crap.
            return GetProperty<string>("name", String.Format("[0x{0:X}]", ID));
        }

        public override string ToString()
        {
            return GetName();
        }
    }


}
