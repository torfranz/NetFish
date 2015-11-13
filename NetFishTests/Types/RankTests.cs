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
    public class RankTests
    {
        [TestMethod()]
        public void OperatorTests()
        {
            var v1 = Rank.RANK_2;
            var v2 = Rank.RANK_3;
            var v3 = v1 + v2.Value;
            Assert.AreEqual(3, v3);

            var v4 = v3 + 1;
            Assert.AreEqual(4, v4);

            var v5 = 1 + v4;
            Assert.AreEqual(5, v5);

            var v6 = v5 - v1;
            Assert.AreEqual(4, v6);

            var v7 = v6 - 1;
            Assert.AreEqual(3, v7);

            Assert.AreEqual(-3, -v7);

            var v8 = v7 * 2;
            Assert.AreEqual(6, v8);

            var v9 = v8 / 2;
            Assert.AreEqual(3, v9);

            var v10 = 2 * v9;
            Assert.AreEqual(6, v10);

            var v11 = v10 / v2;
            Assert.AreEqual(3, v11);
        }
    }
}