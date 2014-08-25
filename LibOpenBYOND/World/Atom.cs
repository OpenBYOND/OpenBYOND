using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.World
{
    /// <summary>
    /// Your basic object in the world.  Can have a loc.  Turfs also count.
    /// </summary>
    public class Atom
    {
        /// <summary>
        /// Use the GetProperty function to get this as a simple type.
        /// </summary>
        public Dictionary<string, Atom> Properties = new Dictionary<string, Atom>();


    }
}
