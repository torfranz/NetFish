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
    public class PositionArrayTests
    {
        [TestMethod()]
        public void AssignmentTests()
        {
            var v1 = new PositionArray(new ExtMove[10], 5);
            var v2 = v1;

            Assert.AreEqual(5, v1.last);
            Assert.AreEqual(10, v1.table.Length);
            Assert.AreEqual(5, v2.last);
            Assert.AreEqual(10, v2.table.Length);

            ++v2;
            Assert.AreEqual(5, v1.last);
            Assert.AreEqual(10, v1.table.Length);
            Assert.AreEqual(6, v2.last);
            Assert.AreEqual(10, v2.table.Length);

            v2.set(new ExtMove[5]);
            Assert.AreEqual(5, v1.last);
            Assert.AreEqual(10, v1.table.Length);
            Assert.AreEqual(0, v2.last);
            Assert.AreEqual(5, v2.table.Length);
        }
    }
}