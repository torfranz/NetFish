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
    public class CheckinfoTests
    {
        [TestMethod()]
        public void ConstructorTests()
        {
            Bitboards.init();
            Position.init();

            var pos = new Position("4rrk1/pp1n3p/3q2pQ/2p2b2/2PPp3/2P3N1/P2B2PP/4RRK1 w - - 0 20", false);

            CheckInfo ci = new CheckInfo(pos);

            Assert.AreEqual(Square.SQ_G8, ci.ksq);
            Assert.AreEqual(0ul, ci.checkSquares[0]);
            Assert.AreEqual(45035996273704960ul, ci.checkSquares[1]);
            Assert.AreEqual(4679521487814656ul, ci.checkSquares[2]);
            Assert.AreEqual(45053622886596608ul, ci.checkSquares[3]);
            Assert.AreEqual(11547299813322129408ul, ci.checkSquares[4]);
            Assert.AreEqual(11592353436208726016ul, ci.checkSquares[5]);
            Assert.AreEqual(0ul, ci.checkSquares[6]);
            Assert.AreEqual(0ul, ci.checkSquares[7]);
        }
    }
}
