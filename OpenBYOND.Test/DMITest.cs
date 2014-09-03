using System.IO;
using NUnit.Framework;

namespace OpenBYOND.Test
{
    
    
    /// <summary>
    ///This is a test class for DMITest and is intended
    ///to contain all DMITest Unit Tests
    ///</summary>
    public class DMITest
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
        ///A test for DMI Constructor
        ///</summary>
        [Test]
        //[DeploymentItem("TestFiles/human.dmi", "TestFiles")]
        public void DMIConstructorTest()
        {
            string icon = Path.Combine("TestFiles","human.dmi");
            string icon_state = string.Empty; // TODO: Initialize to an appropriate value
            DMI target = new DMI(icon);
            //testContextInstance.WriteLine("width = {0}, height={1}", target.Width, target.Height);
        }
    }
}
