using NUnit.Framework;

namespace OpenBYOND.Test
{
    public class IconStateTest
    {
        /// <summary>
        ///A test for GetFrameIndex
        ///</summary>
        [Test]
        public void GetFrameIndexTest()
        {
            IconState target = new IconState(); // TODO: Initialize to an appropriate value
            target.NumDirections = 4;

            // Directionality.
            Assert.AreEqual(0, target.GetFrameIndex(0, Direction.SOUTH, NoLengthChecks: true), "SOUTH:0 does not produce frame 0.");
            Assert.AreEqual(1, target.GetFrameIndex(0, Direction.NORTH, NoLengthChecks: true), "NORTH:0 does not produce frame 1.");
            Assert.AreEqual(2, target.GetFrameIndex(0, Direction.EAST, NoLengthChecks: true), "EAST:0 does not produce frame 2.");
            Assert.AreEqual(3, target.GetFrameIndex(0, Direction.WEST, NoLengthChecks: true), "WEST:0 does not produce frame 3.");
        }
    }
}
