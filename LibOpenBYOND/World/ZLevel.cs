using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.World
{
    public class ZLevel
    {
        public Tile[,] Tiles; // This is a rectangular array.
        public uint Width = 0;
        public uint Height = 0;
        public uint Z=0;

        public delegate void TileIterationDelegate(uint x, uint y, ref Tile t);

        public ZLevel(uint size_x, uint size_y)
        {
            Tiles = new Tile[size_x, size_y];
            this.Width = size_x;
            this.Height = size_y;

        }

        public void Initialize(uint z) {
            this.Z = z;
            this.ForEach(
                (uint x, uint y,ref Tile t) =>
                {
                    t = new Tile(x,y,z);
                }
            );
        }

        public void ForEach(TileIterationDelegate tid)
        {
            for (uint y = 0; y < Height; y++)
                for (uint x = 0; x < Width; x++)
                    tid(x, y, ref Tiles[x, y]);
        }

        public Tile GetTile(uint x, uint y)
        {
            return Tiles[x, y];
        }
    }
}
