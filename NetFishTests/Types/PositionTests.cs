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
    }
}


