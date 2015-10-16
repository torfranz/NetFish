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
    public class ScoreTests
    {
        [TestMethod()]
        public void BaseOperatorTests()
        {
            var v1 = new Score(new Score(1));
            var v2 = new Score(2);
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

            Assert.AreEqual(-3, -v7.Value);

            var v8 = v7 * 2;
            Assert.AreEqual(6, v8.Value);

            var v9 = 2 * v8;
            Assert.AreEqual(12, v9.Value);
        }

        [TestMethod()]
        public void MgEgTest()
        {
            var v1 = Score.make_score(1234, 5678);
            var mg = v1.mg_value();
            var eg = v1.eg_value();
            Assert.AreEqual(1234, mg.value);
            Assert.AreEqual(5678, eg.value);
        }
    }
}