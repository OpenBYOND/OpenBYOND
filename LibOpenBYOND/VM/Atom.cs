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

        public Atom Get()
        {
            return (Atom)CSProperty.GetValue(atom, new object[0]);
        }
    }

    public class AtomProperties
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AtomProperties));

        /// <summary>
        /// Use the GetProperty function to get this as a simple type.
        /// 
        /// TODO: Just link to the type def sitting in the object tree.
        /// </summary>
        private Dictionary<string, Atom> properties = new Dictionary<string, Atom>();

        /// <summary>
        /// Properties that have been changed from the original.
        /// </summary>
        private Dictionary<string, Atom> changedProperties = new Dictionary<string, Atom>();

        /// <summary>
        /// DO NOT FUCK WITH THIS.
        /// 
        /// Set by AtomPropertyAttribute.
        /// </summary>
        private Dictionary<string, NativeAtomProperty> nativeProperties = new Dictionary<string, NativeAtomProperty>();

        private Atom owner;

        internal AtomProperties(Atom a)
        {
            this.owner = a;

            // Build NativeProperties mappings
            foreach (PropertyInfo prop in owner.GetType().GetProperties())
            {
                foreach (AtomPropertyAttribute apa in prop.GetCustomAttributes(typeof(AtomPropertyAttribute), true))
                {
                    NativeAtomProperty nap = new NativeAtomProperty(owner);
                    nap.CSProperty = prop;
                    nap.DMProperty = apa.Name != null ? apa.Name : prop.Name;
                    nativeProperties[nap.DMProperty] = nap;

                    log.DebugFormat("Mapped C# property {0} to atom property {1}.", prop.Name, nap.DMProperty);
                }
            }
        }

        public Atom this[string key]
        {
            get
            {
                if (nativeProperties.ContainsKey(key))
                    return nativeProperties[key].Get();
                if (changedProperties.ContainsKey(key))
                    return changedProperties[key];
                return properties[key];
            }
            set
            {
                if (nativeProperties.ContainsKey(key))
                    nativeProperties[key].Set(value);
                else
                    changedProperties[key] = value;
            }
        }

        internal void ClearDeltas()
        {
            properties = new Dictionary<string, Atom>(changedProperties);
            changedProperties.Clear();
        }

        internal T GetProperty<T>(string key)
        {
            Atom a = null;
            // If there's a binding, redirect Get to the property in question.
            if (nativeProperties.ContainsKey(key))
            {
                a = nativeProperties[key].Get();
            }
            else
            {
                if (!changedProperties.TryGetValue(key, out a))
                {
                    if (!properties.TryGetValue(key, out a))
                    {
                        throw new KeyNotFoundException(string.Format("Property {0} does not exist in {1}.", key, owner.GetName()));
                    }
                }
            }
            return (T)Utils.SimplifyProperty(typeof(T), a);
        }

        internal T GetProperty<T>(string key, T defaultValue)
        {
            Atom a = null;
            // If there's a binding, redirect Get to the property in question.
            if (nativeProperties.ContainsKey(key))
            {
                a = nativeProperties[key].Get();
            }
            else
            {
                if (!changedProperties.TryGetValue(key, out a))
                {
                    if (!properties.TryGetValue(key, out a))
                    {
                        return defaultValue;
                    }
                }
            }
            return (T)Utils.SimplifyProperty(typeof(T), a);
        }

        internal void SetProperty<T>(string key, T val)
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

            if (nativeProperties.ContainsKey(key))
                nativeProperties[key].Set(a);
            else
                changedProperties[key] = a;
        }

        internal string[] GetKeys(bool sorted = false)
        {
            List<string> keys = properties.Keys.ToList();
            keys.AddRange(nativeProperties.Keys.ToList());
            if (sorted) keys.Sort();
            return keys.ToArray();
        }

        internal bool ContainsKey(string key)
        {
            return nativeProperties.ContainsKey(key) || changedProperties.ContainsKey(key) || properties.ContainsKey(key);
        }
    }

    /// <summary>
    /// Your basic object in the world.  _Can_ have a loc.  Turfs also count.
    /// </summary>
    public class Atom
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Atom));

        /// <summary>
        /// ID of the instance.
        /// </summary>
        public int ID;

        /// <summary>
        /// Properties.
        /// </summary>
        public AtomProperties Properties;

        /// <summary>
        /// Used in the Object Tree.
        /// 
        /// TODO:  Make <string,string>
        /// </summary>
        internal Dictionary<string, Atom> Children = new Dictionary<string, Atom>();

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

        /// <summary>
        /// Object Tree stuff.
        /// </summary>
        public Atom Parent;

        private bool Inherited=false;
        private bool ob_inherited=false;

        internal Atom()
            : this("", false)
        {
            // Used internally for simple types and tree stuff.  Don't use this or I will slap you.
        }

        /// <summary>
        /// Instantiate an atom.
        /// </summary>
        /// <param name="makeInitial">Create InitialProperties?</param>
        public Atom(string path, bool makeInitial = true)
        {
            Properties = new AtomProperties(this);
            this.Type = new BYONDType(path);

            if (makeInitial)
                Properties.ClearDeltas();
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
            return Properties.GetProperty<T>(key);
        }

        // I don't know why I made this.
        public T GetProperty<T>(string key, T defaultValue)
        {
            return Properties.GetProperty<T>(key, defaultValue);
        }

        /// <summary>
        /// Set a property, given a simple type.
        /// </summary>
        /// <typeparam name="T">Simple type (int, float, string)</typeparam>
        /// <param name="key">Property to set.</param>
        /// <param name="val">Value</param>
        public void SetProperty<T>(string key, T val)
        {
            Properties.SetProperty<T>(key, val);
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

        internal void InheritProperties()
        {
            //if self.ob_inherited: return
            // debugInheritance=self.path in ('/area','/obj','/mob','/atom/movable','/atom')
            if (this.Parent != null)
            {
                if (this.Parent.ob_inherited) return;
                string[] keys = Parent.Properties.GetKeys(sorted: true);
                foreach (string key in keys)
                {
                    Atom value = Parent.Properties[key];//.copy()
                    if (!Properties.ContainsKey(key))
                    {
                        Properties[key] = value;
                        Properties[key].Inherited = true;
                        // if debugInheritance:print('  {0}[{2}] -> {1}'.format(self.parent.path,self.path,key))
                    }
                }
            }
            // assert 'name' in self.properties
            this.ob_inherited = true;
            foreach (string k in this.Children.Keys)
            {
                this.Children[k].InheritProperties();
            }
        }
    }
}
