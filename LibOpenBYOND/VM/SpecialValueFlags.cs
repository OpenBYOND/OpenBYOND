using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.VM
{
    [Flags]
    public enum SpecialValueFlags
    {
        NONE=0,
        GLOBAL=1,
        CONST=2
    }
}
