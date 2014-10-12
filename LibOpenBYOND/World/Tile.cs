using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using OpenBYOND.VM;

namespace OpenBYOND.World
{
    public class Tile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Tile));

        public List<Atom> Atoms = new List<Atom>();

        public string origID;

        public Location loc;

        internal Tile(uint x, uint y, uint z)
        {
            loc = new Location(x, y, z, this);
        }

        public Tile()
        {
            // TODO: Complete member initialization
        }

        internal Tile CopyNew(uint x, uint y, uint z)
        {
            Tile t = new Tile(x,y,z);
            t.Atoms = this.Atoms.Select((Atom a) => {
                return a.Clone();
            }).ToList();
            return t;
        }
    }
}
