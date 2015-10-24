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
    public class PositionTests
    {
        [TestMethod()]
        public void initTest()
        {
            Bitboards.init();
            Position.init();

            // check Zobrist.psq values
            Assert.AreEqual(10536503665313592279ul, Zobrist.psq[0, 1, 7]);
            Assert.AreEqual(4539792784031873725ul, Zobrist.psq[0, 1, 8]);
            Assert.AreEqual(2841870292508388689ul, Zobrist.psq[0, 1, 9]);
            Assert.AreEqual(15413206348252250872ul, Zobrist.psq[0, 1, 10]);
            Assert.AreEqual(6880843334474042246ul, Zobrist.psq[1, 1, 7]);
            Assert.AreEqual(560415017990002212ul, Zobrist.psq[1, 1, 8]);
            Assert.AreEqual(6626394159937994533ul, Zobrist.psq[1, 1, 9]);
            Assert.AreEqual(2670333323665803600ul, Zobrist.psq[1, 1, 10]);

            // check Zobrist.enpassant values
            Assert.AreEqual(9031641776876329352ul, Zobrist.enpassant[0]);
            Assert.AreEqual(12228382040141709029ul, Zobrist.enpassant[1]);
            Assert.AreEqual(2494223668561036951ul, Zobrist.enpassant[2]);
            Assert.AreEqual(7849557628814744642ul, Zobrist.enpassant[3]);
            Assert.AreEqual(16000570245257669890ul, Zobrist.enpassant[4]);
            Assert.AreEqual(16614404541835922253ul, Zobrist.enpassant[5]);
            Assert.AreEqual(17787301719840479309ul, Zobrist.enpassant[6]);
            Assert.AreEqual(6371708097697762807ul, Zobrist.enpassant[7]);

            // check Zobrist.castling values
            Assert.AreEqual(0ul, Zobrist.castling[0]);
            Assert.AreEqual(7487338029351702425ul, Zobrist.castling[1]);
            Assert.AreEqual(10138645747811604478ul, Zobrist.castling[2]);
            Assert.AreEqual(16959407016388712551ul, Zobrist.castling[3]);
            Assert.AreEqual(16332212992845378228ul, Zobrist.castling[4]);
            Assert.AreEqual(9606164174486469933ul, Zobrist.castling[5]);
            Assert.AreEqual(7931993123235079498ul, Zobrist.castling[6]);
            Assert.AreEqual(719529192282958547ul, Zobrist.castling[7]);

            // check Zobrist.exclusion
            Assert.AreEqual(895963052000028445UL, Zobrist.exclusion);

            // check Zobrist.side
            Assert.AreEqual(4906379431808431525ul, Zobrist.side);
        }

        [TestMethod()]
        public void fenTest()
        {
            Bitboards.init();
            Position.init();
            
            var fen1 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 10";
            var pos1 = new Position(fen1, false, null);
            Assert.AreEqual(fen1, pos1.fen());
            Assert.AreEqual(0x81598B11829602DD, pos1.st.key);

            var fen2 = "2r3k1/1q1nbppp/r3p3/3pP3/pPpP4/P1Q2N2/2RN1PPP/2R4K b - b3 0 23";
            var pos2 = new Position(fen2, false, null);
            Assert.AreEqual(fen2, pos2.fen());
            Assert.AreEqual(0xD76B6F24873CAD66, pos2.st.key);

            var fen3 = "rnb1kbnr/pp1ppppp/2p5/q7/4P3/3P4/PPP2PPP/RNBQKBNR w KQkq - 1 3";
            var pos3 = new Position(fen3, false, null);
            Assert.AreEqual(fen3, pos3.fen());
            Assert.AreEqual(0x139B22CF5565D5CEul, pos3.st.key);
            var b = pos3.checkers();
            Assert.AreEqual("a5", UCI.square(Utils.pop_lsb(ref b)));
        }

        [TestMethod()]
        public void flipTest()
        {
            Bitboards.init();
            Position.init();

            var fen1 = "r1bbk1nr/pp3p1p/2n5/1N4p1/2Np1B2/8/PPP2PPP/2KR1B1R w kq - 0 13";
            var pos1 = new Position(fen1, false, null);
            pos1.flip();
            Assert.AreEqual("2kr1b1r/ppp2ppp/8/2nP1b2/1n4P1/2N5/PP3P1P/R1BBK1NR b KQ - 0 13", pos1.fen());

            pos1.flip();
            Assert.AreEqual(fen1, pos1.fen());

            var fen2 = "2r3k1/1q1nbppp/r3p3/3pP3/pPpP4/P1Q2N2/2RN1PPP/2R4K b - b3 0 23";
            var pos2 = new Position(fen2, false, null);
            pos2.flip();
            Assert.AreEqual("2r4k/2rn1ppp/p1q2n2/PpPp4/3Pp3/R3P3/1Q1NBPPP/2R3K1 w - b6 0 23", pos2.fen());

            pos2.flip();
            Assert.AreEqual(fen2, pos2.fen());
        }
    }
}


