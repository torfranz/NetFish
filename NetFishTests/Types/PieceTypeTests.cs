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
    public class PieceTypeTests
    {
        [TestMethod()]
        public void OperatorTests()
        {
            var v1 = new PieceType(new PieceType(1));
            var v2 = new PieceType(2);
            var v3 = v1 + v2;
            Assert.AreEqual(3, v3.Value);

            var v4 = v3 + 1;
            Assert.AreEqual(4, v4.Value);

            var v5 = 1 + v4;
            Assert.AreEqual(5, v5.Value);

            var v6 = v5 - v1;
            Assert.AreEqual(4, v6.Value);

            var v7 = v6 - 1;
            Assert.AreEqual(3, v7.Value);

            /*
            Assert.AreEqual(-3, -v7.Value);

            var v8 = v7 * 2;
            Assert.AreEqual(6, v8.Value);

            var v9 = 2 * v8;
            Assert.AreEqual(12, v9.Value);

            var v10 = v9 / 2;
            Assert.AreEqual(6, v10.Value);

            var v11 = v10 / v2;
            Assert.AreEqual(3, v11);
            */
        }
    }
}