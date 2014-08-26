using OpenBYOND;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Test
{


    /// <summary>
    ///This is a test class for DMETest and is intended
    ///to contain all DMETest Unit Tests
    ///</summary>
    [TestClass()]
    public class DMETest
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
        ///A test for LoadFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TestFiles","TestFiles")]
        public void LoadFileTest()
        {
            testContextInstance.WriteLine("CD: {0}", Environment.CurrentDirectory);

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
        [TestMethod()]
        [DeploymentItem("LibOpenBYOND.dll")]
        public void parseIncludeTest()
        {
            DME_Accessor target = new DME_Accessor(); // TODO: Initialize to an appropriate value
            string line = "#include \"balls.txt\""; // TODO: Initialize to an appropriate value
            string expected = Path.GetFullPath(Path.Combine("balls.txt"));
            target.parseInclude(line);
            CollectionAssert.Contains(target.Files, expected, "Balls.txt not in Files.");
        }
    }
}
