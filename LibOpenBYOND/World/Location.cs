using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.World
{
    public struct Location
    {
        public uint X;
        public uint Y;
        public uint Z;

        public Location(uint x, uint y, uint z, Tile t)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
