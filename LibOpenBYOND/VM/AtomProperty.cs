using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OpenBYOND.VM
{
    /// <summary>
    /// Specifies which atom property to bind with.
    /// 
    /// This basically tells DM to send all property updates to the specified C# property, rather than to the standard user-defined collections in AtomProperties.
    /// </summary>
    public class AtomPropertyAttribute : Attribute
    {
        /// <summary>
        /// The name of the DM-side property to bind with (atom.Name).
        /// </summary>
        public string Name;

        public AtomPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
