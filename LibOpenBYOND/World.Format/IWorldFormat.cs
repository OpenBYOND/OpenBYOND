using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBYOND.World.Format
{
    public interface IWorldFormat
    {
        /// <summary>
        /// Save a world to a serialized form.
        /// </summary>
        /// <param name="w">World to save</param>
        /// <param name="filename">Filename to output to</param>
        void Save(World w, string filename);

        /// <summary>
        /// Load a world from a serialized form.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="filename"></param>
        void Load(World w, string filename);
    }
}
