using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.World.Format
{
    public abstract class WorldFormat
    {
        /// <summary>
        /// What file extensions, if any, should be tied to this IWorldFormat?
        /// </summary>
        internal abstract List<string> GetExtensions();

        /// <summary>
        /// Save a world to a serialized form.
        /// </summary>
        /// <param name="w">World to save</param>
        /// <param name="filename">Filename to output to</param>
        public abstract bool Save(World w, string filename);

        /// <summary>
        /// Load a world from a serialized form.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="filename"></param>
        public abstract bool Load(World w, string filename);
    }
}
