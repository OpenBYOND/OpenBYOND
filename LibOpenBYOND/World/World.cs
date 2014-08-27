using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBYOND.World;
using OpenBYOND.World.Serializers;

/**
 * N3X here, I'm going to initially construct this like BYONDTools' Map.
 * 
 */
namespace OpenBYOND.World
{
    class World
    {
        public List<ZLevel> Levels = new List<ZLevel>();
        public List<Atom> Atoms = new List<Atom>();

        public World()
        {

        }

        public void Load(string filename, IWorldSerializer wl)
        {
        }

        public void Save(string filename, IWorldSerializer wl)
        {
        }
    }
}
