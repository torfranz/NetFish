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
        public void OperatorTests()
        {
            var v1 = Color.WHITE;
            var v2 = Color.BLACK;
            var v3 = v1 + v2;
            Assert.AreEqual(Color.BLACK, v3);

            var v4 = v3 + 1;
            Assert.AreEqual(Color.NO_COLOR, v4);
            Assert.AreEqual(Color.COLOR_NB, v4);

            var v5 = v4 - v2;
            Assert.AreEqual(Color.BLACK, v5);

            var v6 = 1 + v5;
            Assert.AreEqual(Color.NO_COLOR, v6);

            var v7 = v6 - 1;
            Assert.AreEqual(Color.BLACK, v7);

            /*
            Assert.AreEqual(new Square(-3), -v7);

            var v8 = v7 * 2;
            Assert.AreEqual(new Square(6), v8);

            var v9 = 2 * v8;
            Assert.AreEqual(new Square(12), v9);

            var v10 = v9 / 2;
            Assert.AreEqual(new Square(6), v10);

            var v11 = v10 / v2;
            Assert.AreEqual(3, v11);
            */
        }

        [TestMethod()]
        public void FlipTests()
        {
            var v1 = Color.BLACK;
            var v2 = ~v1;
            Assert.AreEqual(Color.WHITE, v2);
            var v3 = ~v2;
            Assert.AreEqual(Color.BLACK, v3);
        }
    }
}