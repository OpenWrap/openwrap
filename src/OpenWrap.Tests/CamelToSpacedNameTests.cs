using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace OpenWrap.Tests
{
    [TestFixture]
    public class CamelToSpacedNameTests
    {
        [Test]
        [Sequential]
        public void Test(
            [Values("HelloWorld",  "ABC", "CustomerID",  "SomeLongerText")] string input,
            [Values("Hello World", "ABC", "Customer ID", "Some Longer Text")] string output)
        {
            Assert.AreEqual(output, input.CamelToSpacedName());
        }
    }
}
