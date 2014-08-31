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

    public class AtomProperties
    {
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

        private Atom owner;

        internal AtomProperties(Atom a)
        {
            this.owner = a;
        }

        public Atom this[string key]
        {
            get
            {
                if (changedProperties.ContainsKey(key))
                    return changedProperties[key];
                return properties[key];
            }
            set
            {
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
            if (!changedProperties.TryGetValue(key, out a))
            {
                if (!properties.TryGetValue(key, out a))
                {
                    throw new KeyNotFoundException(string.Format("Property {0} does not exist in {1}.", key, owner.GetName()));
                }
            }
            return (T)Utils.SimplifyProperty(typeof(T), a);
        }

        internal T GetProperty<T>(string key, T defaultValue)
        {
            Atom a = null;
            if (!changedProperties.TryGetValue(key, out a))
            {
                if (!properties.TryGetValue(key, out a))
                {
                    return defaultValue;
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

            changedProperties[key] = a;
        }

        internal string[] GetKeys(bool sorted = false)
        {
            List<string> keys = properties.Keys.ToList();
            if (sorted) keys.Sort();
            return keys.ToArray();
        }

        internal bool ContainsKey(string key)
        {
            return changedProperties.ContainsKey(key) || properties.ContainsKey(key);
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
        /// DO NOT FUCK WITH THIS.
        /// 
        /// Set by AtomPropertyAttribute.
        /// </summary>
        private Dictionary<string, NativeAtomProperty> NativeProperties = new Dictionary<string, NativeAtomProperty>();

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
