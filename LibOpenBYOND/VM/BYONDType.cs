using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.VM
{
    public class BYONDType
    {
        /// <summary>
        /// Path string.
        /// </summary>
        string[] type = new string[0];

        /// <summary>
        /// Can be fucked with by scripts.
        /// </summary>
        bool mutable = true;

        public BYONDType(string path, bool mutable = true)
        {
            this.type = path.Split(new string[] { "/" }, StringSplitOptions.None);
            this.mutable = mutable;
        }
    }
}
