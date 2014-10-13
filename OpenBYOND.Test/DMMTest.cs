using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OpenBYOND.World;
using OpenBYOND.World.Format;

namespace OpenBYOND.Test
{
    public class DMMTest
    {
        /// <summary>
        ///A test for Load
        ///</summary>
        [Test]
        public void LoadMapTest()
        {
            DMMLoader target = new DMMLoader();
            string filename = Path.Combine("TestFiles","TestMap.dmm");

            OpenBYOND.World.World world = new OpenBYOND.World.World();
            target.Load(world, filename);

            Assert.AreEqual(6, world.Levels.Count);

            ZLevel zlev = world.Levels[0];
            Assert.AreEqual(255, zlev.Height);
            Assert.AreEqual(255, zlev.Width);
        }
    }
}
