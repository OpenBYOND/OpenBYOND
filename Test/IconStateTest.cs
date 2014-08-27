using OpenBYOND;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{


    /// <summary>
    ///This is a test class for IconStateTest and is intended
    ///to contain all IconStateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IconStateTest
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
        ///A test for GetFrameIndex
        ///</summary>
        [TestMethod()]
        public void GetFrameIndexTest()
        {
            IconState target = new IconState(); // TODO: Initialize to an appropriate value
            target.NumDirections = 4;

            // Directionality.
            Assert.AreEqual<uint>(0, target.GetFrameIndex(0, Direction.SOUTH, NoLengthChecks: true), "SOUTH:0 does not produce frame 0.");
            Assert.AreEqual<uint>(1, target.GetFrameIndex(0, Direction.NORTH, NoLengthChecks: true), "NORTH:0 does not produce frame 1.");
            Assert.AreEqual<uint>(2, target.GetFrameIndex(0, Direction.EAST, NoLengthChecks: true), "EAST:0 does not produce frame 2.");
            Assert.AreEqual<uint>(3, target.GetFrameIndex(0, Direction.WEST, NoLengthChecks: true), "WEST:0 does not produce frame 3.");
        }
    }
}
