using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetFishTests
{
    [TestClass]
    public class PerftTests
    {
        [TestMethod]
        public void TestPos1()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", false, null);

            Assert.AreEqual(20, Search.perft(true, pos, new Depth(1)));
            Assert.AreEqual(400, Search.perft(true, pos, new Depth(2)));
            Assert.AreEqual(8902, Search.perft(true, pos, new Depth(3)));
            Assert.AreEqual(197281, Search.perft(true, pos, new Depth(4)));
            Assert.AreEqual(4865609, Search.perft(true, pos, new Depth(5)));
        }

        [TestMethod]
        public void TestPos2()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", false, null);

            Assert.AreEqual(48, Search.perft(true, pos, new Depth(1)));
            Assert.AreEqual(2039, Search.perft(true, pos, new Depth(2)));
            Assert.AreEqual(97862, Search.perft(true, pos, new Depth(3)));
            Assert.AreEqual(4085603, Search.perft(true, pos, new Depth(4)));
        }

        [TestMethod]
        public void TestPos3()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -", false, null);

            Assert.AreEqual(14, Search.perft(true, pos, new Depth(1)));
            Assert.AreEqual(191, Search.perft(true, pos, new Depth(2)));
            Assert.AreEqual(2812, Search.perft(true, pos, new Depth(3)));
            Assert.AreEqual(43238, Search.perft(true, pos, new Depth(4)));
            Assert.AreEqual(674624, Search.perft(true, pos, new Depth(5)));
        }

        [TestMethod]
        public void TestPos4()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", false, null);

            Assert.AreEqual(6, Search.perft(true, pos, new Depth(1)));
            Assert.AreEqual(264, Search.perft(true, pos, new Depth(2)));
            Assert.AreEqual(9467, Search.perft(true, pos, new Depth(3)));
            Assert.AreEqual(422333, Search.perft(true, pos, new Depth(4)));
            Assert.AreEqual(15833292, Search.perft(true, pos, new Depth(5)));
        }

        [TestMethod]
        public void TestPos5()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", false, null);

            Assert.AreEqual(44, Search.perft(true, pos, new Depth(1)));
            Assert.AreEqual(1486, Search.perft(true, pos, new Depth(2)));
            Assert.AreEqual(62379, Search.perft(true, pos, new Depth(3)));
            Assert.AreEqual(2103487, Search.perft(true, pos, new Depth(4)));
            Assert.AreEqual(89941194, Search.perft(true, pos, new Depth(5)));
        }

        [TestMethod]
        public void TestPos6()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", false, null);

            Assert.AreEqual(46, Search.perft(true, pos, new Depth(1)));
            Assert.AreEqual(2079, Search.perft(true, pos, new Depth(2)));
            Assert.AreEqual(89890, Search.perft(true, pos, new Depth(3)));
            Assert.AreEqual(3894594, Search.perft(true, pos, new Depth(4)));
        }
    }
}
