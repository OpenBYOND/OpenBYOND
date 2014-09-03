using NUnit.Framework;

namespace OpenBYOND.Test
{
    public class DirUtilsTest
    {

        /// <summary>
        ///A test for GetDirIndex
        ///</summary>
        [Test]
        public void GetDirIndexTest()
        {
            Assert.AreEqual(0, DirUtils.GetDirIndex(Direction.SOUTH), "SOUTH did not produce 0");
            Assert.AreEqual(1, DirUtils.GetDirIndex(Direction.NORTH), "NORTH did not produce 1");
            Assert.AreEqual(2, DirUtils.GetDirIndex(Direction.EAST), "EAST did not produce 2");
            Assert.AreEqual(3, DirUtils.GetDirIndex(Direction.WEST), "WEST did not produce 3");
        }

        /// <summary>
        ///A test for GetDirFromString
        ///</summary>
        [Test]
        public void GetDirFromStringTest()
        {
            Assert.AreEqual(Direction.NORTH, DirUtils.GetDirFromString("NORTH"));
        }
        /*
        /// <summary>
        ///A test for GetNameFromDir
        ///</summary>
        [Test]
        public void GetNameFromDirTest()
        {
            Direction dir = new Direction(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = DirUtils.GetNameFromDir(dir);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
         * */
    }
}
