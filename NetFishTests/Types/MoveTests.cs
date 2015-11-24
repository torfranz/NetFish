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
    public class MoveTests
    {
        [TestMethod()]
        public void MakeMoveTests()
        {
            var v1 = Move.make(Square.SQ_A1, Square.SQ_H8);
            Assert.AreEqual(Square.SQ_A1, Move.from_sq(v1));
            Assert.AreEqual(Square.SQ_H8, Move.to_sq(v1));
            Assert.AreEqual(MoveType.NORMAL, Move.type_of(v1));
        }

        [TestMethod()]
        public void MakeTests()
        {
            var v1 = Move.make(MoveType.CASTLING, Square.SQ_A1, Square.SQ_H8, PieceType.BISHOP);
            Assert.AreEqual(Square.SQ_A1, Move.from_sq(v1));
            Assert.AreEqual(Square.SQ_H8, Move.to_sq(v1));
            Assert.AreEqual(MoveType.CASTLING, Move.type_of(v1));
            Assert.AreEqual(PieceType.BISHOP, Move.promotion_type(v1));
        }

        [TestMethod()]
        public void IsOkTests()
        {
            var v1 = Move.make(MoveType.ENPASSANT, Square.SQ_A1, Square.SQ_H8, PieceType.BISHOP);
            Assert.IsTrue(Move.is_ok(v1));

            var v2 = Move.make(MoveType.PROMOTION, Square.SQ_A1, Square.SQ_A1, PieceType.BISHOP);
            Assert.IsFalse(Move.is_ok(v2));
        }
    }
}