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

            var pos1 = new Position("4rrk1/pp1n3p/3q2pQ/2p2b2/2PPp3/2P3N1/P2B2PP/4RRK1 w - - 0 20", false);
                                   
            MovePicker mp1 = new MovePicker(pos1, Move.MOVE_NONE, Depth.DEPTH_ZERO, new HistoryStats(), new CounterMovesHistoryStats(), Move.to_sq(new Move(2332)));
            
            var move1 = mp1.next_move(false);
            Assert.AreEqual(1445, move1);
            Assert.AreEqual(1445, mp1.moves[0].Move);
            Assert.AreEqual(36, mp1.moves[0].Value);
            Assert.AreEqual(3069, mp1.moves[7].Move);
            Assert.AreEqual(-130, mp1.moves[7].Value);

            var pos2 = new Position("4rrk1/pp1n3p/3q2pQ/2p2N2/2PPp3/2P5/P2B2PP/4RRK1 b - - 0 20", false);
            var mp2 = new MovePicker(pos2, Move.MOVE_NONE, new Depth(-1), new HistoryStats(), new CounterMovesHistoryStats(), Move.to_sq(new Move(1445)));
            var move2 = mp2.next_move(false);
            Assert.AreEqual(2981, move2);

            var pos3 = new Position("4rrk1/pp1n3p/3q3Q/2p2p2/2PPp3/2P5/P2B2PP/4RRK1 w - - 0 21", false);
            var mp3 = new MovePicker(pos3, Move.MOVE_NONE, new Depth(-2), new HistoryStats(), new CounterMovesHistoryStats(), Move.to_sq(new Move(2981)));
            var move3 = mp3.next_move(false);
            Assert.AreEqual(3051, move3);

            var pos4 = new Position("4rrk1/pp1n3p/3Q4/2p2p2/2PPp3/2P5/P2B2PP/4RRK1 b - - 0 21", false);
            var mp4 = new MovePicker(pos4, Move.MOVE_NONE, new Depth(-3), new HistoryStats(), new CounterMovesHistoryStats(), Move.to_sq(new Move(3051)));
            var move4 = mp4.next_move(false);
            Assert.AreEqual(2203, move4);

            var move5 = mp4.next_move(false);
            Assert.AreEqual(0, move5);
        }
    }
}


