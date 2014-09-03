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
