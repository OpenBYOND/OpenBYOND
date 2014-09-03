using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace OpenBYOND.Test
{
    public class DMETest
    {
        /// <summary>
        ///A test for LoadFile
        ///</summary>
        [Test]
        //[DeploymentItem("TestFiles","TestFiles")]
        public void LoadFileTest()
        {
            //testContextInstance.WriteLine("CD: {0}", Environment.CurrentDirectory);

            DME target = new DME();
            string filename = Path.Combine("TestFiles","Test.dme");

            List<string> expected = new List<string>();
            expected.Add(Path.GetFullPath(Path.Combine("TestFiles", "code", "global.dm")));
            expected.Add(Path.GetFullPath(Path.Combine("TestFiles", "code", "world.dm")));
            expected.Add(Path.GetFullPath(Path.Combine("TestFiles", "interface", "interface.dm")));
            expected.Add(Path.GetFullPath(Path.Combine("TestFiles", "interface", "skin.dmf")));
            expected.Add(Path.GetFullPath(Path.Combine("TestFiles", "maps", "tgstation.dmm")));

            target.LoadFile(filename);

            Assert.AreEqual(target.Files.Count, 5, "Got incorrect # of files.");
            CollectionAssert.AllItemsAreInstancesOfType(target.Files, typeof(string), "Wrong types.");
            CollectionAssert.AreEqual(expected, target.Files, "Wrong entries");
        }

        /// <summary>
        ///A test for parseInclude
        ///</summary>
        [Test]
        //[DeploymentItem("LibOpenBYOND.dll")]
        public void parseIncludeTest()
        {
            DME target = new DME(); // TODO: Initialize to an appropriate value
            string line = "#include \"balls.txt\""; // TODO: Initialize to an appropriate value
            string expected = Path.GetFullPath(Path.Combine("balls.txt"));
            target.parseInclude(line);
            CollectionAssert.Contains(target.Files, expected, "Balls.txt not in Files.");
        }
    }
}
