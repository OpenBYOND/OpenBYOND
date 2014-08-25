using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.World
{
    /// <summary>
    /// Your basic object in the world.  _Can_ have a loc.  Turfs also count.
    /// </summary>
    public class Atom
    {
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

        public Atom(bool makeInitial=true)
        {
            if (makeInitial)
                InitialProperties = new Dictionary<string,Atom>(Properties); // Make copy.
        }

        /// <summary>
        /// Yes, type in BYOND is mutable.  How fucking dumb.
        /// </summary>
        public string BYONDType { get; set; }

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
            if(val.GetType() == typeof(Atom))
            {
                object o = val;
                a = (Atom)o;
            } else {
                a = new SimpleAtom<T>(val);
            }

            Properties[key] = a;
        }

        public string GetName()
        {
            // Emulate BYOND behavior.  (ID instead of memory address)
            // TODO: Proper/improper crap.
            return GetProperty<string>("name", String.Format("[0x{0:X}]",ID.ToString("X"))); 
        }

        public override string ToString()
        {
            return GetName();
        }
    }

    /// <summary>
    /// For simple types (T = int,float,string)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleAtom<T> : Atom {

        public SimpleAtom(T val)
        {
            this.Value = val;
        }
        /// <summary>
        /// Value of the "simple" type.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Implicit casting.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static implicit operator T(SimpleAtom<T> a) {
            return a.Value;
        }
    }
}
