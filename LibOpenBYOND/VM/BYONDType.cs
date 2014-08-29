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
            this.type = path.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries); // Dump empty entries. (Beginning slash, double slashes)
            this.mutable = mutable;
        }

        public string[] GetPathSegments()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "/" + ("/".join(type));
        }
    }
}
