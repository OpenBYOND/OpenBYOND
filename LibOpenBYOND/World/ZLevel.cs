using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.World
{
    public class ZLevel
    {
        public Tile[,] Tiles;

        public ZLevel(uint size_x, uint size_y)
        {
            Tiles = new Tile[size_x, size_y];
        }

        public Tile GetTile(uint x, uint y)
        {
            return Tiles[x, y];
        }
    }
}
