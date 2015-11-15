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

            var v2 = Score.Create(2097248);
            var mg2 = Score.mg_value(v2);
            Assert.AreEqual(32, mg2);

            var v3 = Score.Create(-2097248);
            var mg3 = Score.mg_value(v3);
            Assert.AreEqual(-32, mg3);
        }
    }
}