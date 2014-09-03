using NUnit.Framework;

namespace OpenBYOND.Test
{
    /// <summary>
    ///This is a test class for DirUtilsTest and is intended
    ///to contain all DirUtilsTest Unit Tests
    ///</summary>
    [TestFixture]
    public class DirUtilsTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


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
            Assert.AreEqual(Direction.NORTH,DirUtils.GetDirFromString("NORTH"));
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
