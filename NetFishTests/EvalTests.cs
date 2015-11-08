using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    using System.Diagnostics;

    [TestClass()]
    public class EvalTests
    {
        [TestMethod()]
        public void evaluateTest1()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();
            Bitbases.init();
            Search.init();
            Eval.init();
            Pawns.init();
            TranspositionTable.resize(16);

            var pos = new Position("rnbqkb1r/ppp1pppp/3p1P2/8/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 3", false, new MainThread(null));
            var eval = Eval.evaluate(false, pos);
            Assert.AreEqual(-799, eval);
        }

        
    }
}