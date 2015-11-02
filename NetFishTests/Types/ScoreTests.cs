﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var v1 = new Score(1);
            var v2 = new Score(2);
            var v3 = v1 + v2;
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

            var v9 = 2 * v8;
            Assert.AreEqual(12, v9);
        }

        [TestMethod()]
        public void MgEgTest()
        {
            var v1 = Score.make_score(12, 56);
            var mg = Score.mg_value(v1);
            var eg = Score.eg_value(v1);
            Assert.AreEqual(12, mg);
            Assert.AreEqual(56, eg);

            v1 = Score.make_score(-12, -56);
            mg = Score.mg_value(v1);
            eg = Score.eg_value(v1);
            Assert.AreEqual(-12, mg);
            Assert.AreEqual(-56, eg);

            var v2 = new Score(2097248);
            var mg2 = Score.mg_value(v2);
            Assert.AreEqual(32, mg2);

            var v3 = new Score(-2097248);
            var mg3 = Score.mg_value(v3);
            Assert.AreEqual(-32, mg3);
        }
    }
}