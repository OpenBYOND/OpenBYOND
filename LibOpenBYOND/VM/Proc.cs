using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.VM
{
    public class Proc : Atom
    {
        private string[] procArgs;
        public bool definition;
        public string origpath;

        public Proc(string path, string[] procArgs, string filename, int ln)
        {
            this.Type = new BYONDType(path);
            this.procArgs = procArgs; // TODO
            this.Filename = filename;
            this.Line = ln;
        }
    }
}
