using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBYOND.World;
using OpenBYOND.World.Format;
using LibOpenBYOND.World;

/**
 * N3X here, I'm going to initially construct this like BYONDTools' Map.
 * 
 */
namespace OpenBYOND.World
{
    public class World : Atom
    {
        public List<ZLevel> Levels = new List<ZLevel>();

        public List<Atom> Atoms = new List<Atom>();

        /// <summary>
        /// Default turf.  Applied to tile when all turfs in the tile are deleted.
        /// </summary>
        [AtomProperty("turf")]
        public Atom default_turf = null;

        public World() : base()
        {
        }

        public ZLevel CreateZLevel(uint height, uint width, int z = -1)
        {
            ZLevel zLevel = new ZLevel(width, height);
            if (z >= 0)
                Levels[z] = zLevel;
            else
                Levels.Add(zLevel);
            return zLevel;
        }
    
        /*
        public AtomIterator IterAtoms() { return new AtomIterator(this); }
        public TileIterator IterTiles() { return new TileIterator(this); }
        public LocIterator IterLocs() { return new LocIterator(this); }
        */

        public void Load(string filename, IWorldFormat wf)
        {
            wf.Load(this, filename);
        }

        public void Save(string filename, IWorldFormat wf)
        {
            wf.Save(this, filename);
        }
    }
}
