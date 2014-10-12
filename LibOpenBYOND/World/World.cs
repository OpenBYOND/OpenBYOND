using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBYOND.World;
using OpenBYOND.World.Format;
using OpenBYOND.VM;
using LibOpenBYOND.VM;

/**
 * N3X here, I'm going to initially construct this like BYONDTools' Map.
 * 
 */
namespace OpenBYOND.World
{
    [GlobalBinding("world")]
    public class World : Atom
    {
        public List<ZLevel> Levels = new List<ZLevel>();

        public List<Atom> Atoms = new List<Atom>();

        /// <summary>
        /// Default turf.  Applied to tile when all turfs in the tile are deleted.
        /// </summary>
        [AtomProperty("turf")]
        public Atom default_turf = null;

        public World()
            : base("/reserved/world") // lolidfk
        {
        }

        public ZLevel CreateZLevel(uint height, uint width, int z = -1)
        {
            ZLevel zLevel = new ZLevel(width, height);
            if (z >= 0)
            {
                Levels[z] = zLevel;
            }
            else
            {
                Levels.Add(zLevel);
                z = Levels.Count - 1;
            }
            zLevel.Initialize((uint)z);
            return zLevel;
        }

        /// <summary>
        /// Get the Tile at an x,y,z coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Tile GetTileAt(uint x, uint y, uint z)
        {
            if (z < Levels.Count)
            {
                return Levels[(int)z].GetTile(x, y);
            }
            return null;
        }

        /*
        public AtomIterator IterAtoms() { return new AtomIterator(this); }
        public TileIterator IterTiles() { return new TileIterator(this); }
        public LocIterator IterLocs() { return new LocIterator(this); }
        */

        public void Load(string filename, WorldFormat wf)
        {
            wf.Load(this, filename);
        }

        public void Save(string filename, WorldFormat wf)
        {
            wf.Save(this, filename);
        }
    }
}
