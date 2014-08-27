using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibOpenBYOND.VM
{
    public class GlobalBindingAttribute : Attribute
    {
        public string Name;

        public GlobalBindingAttribute(string name)
        {
            Name = name;
        }
    }
}
