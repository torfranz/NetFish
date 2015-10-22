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
    public class MovepickerTests
    {
        [TestMethod()]
        public void GenerateNextStageTest()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("4rrk1/pp1n3p/3q2pQ/2p2b2/2PPp3/2P3N1/P2B2PP/4RRK1 w - - 0 20", false);

            MovePicker mp = new MovePicker(pos, Move.MOVE_NONE, Depth.DEPTH_ZERO, new HistoryStats(), new CounterMovesHistoryStats(), Move.to_sq(new Move(2332)));
            
            var move = mp.next_move(false);
            Assert.AreEqual(1445, move);
        }
    }
}


