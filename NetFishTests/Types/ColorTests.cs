using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    using System.Diagnostics;

    [TestClass()]
    public class ColorTests
    {
        [TestMethod()]
        public void FlipTests()
        {
            var v1 = Color.BLACK;
            var v2 = Color.opposite(v1);
            Assert.AreEqual(Color.WHITE, v2);
            var v3 = Color.opposite(v2);
            Assert.AreEqual(Color.BLACK, v3);
        }
    }
}