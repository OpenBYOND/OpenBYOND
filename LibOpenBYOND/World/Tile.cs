using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace OpenBYOND.World
{
    public class Tile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));
        public List<Atom> Atoms = new List<Atom>();
    }
}
