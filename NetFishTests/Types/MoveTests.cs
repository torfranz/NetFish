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
            var v1 = Move.make_move(new Square(Square.SQ_A1), new Square(Square.SQ_H8));
            Assert.AreEqual(Square.SQ_A1, v1.from_sq());
            Assert.AreEqual(Square.SQ_H8, v1.to_sq());
            Assert.AreEqual(MoveType.NORMAL, v1.type_of());
        }

        [TestMethod()]
        public void MakeTests()
        {
            var v1 = Move.make(MoveType.CASTLING, new Square(Square.SQ_A1), new Square(Square.SQ_H8), new PieceType(PieceType.BISHOP));
            Assert.AreEqual(Square.SQ_A1, v1.from_sq());
            Assert.AreEqual(Square.SQ_H8, v1.to_sq());
            Assert.AreEqual(MoveType.CASTLING, v1.type_of());
            Assert.AreEqual(PieceType.BISHOP, v1.promotion_type());
        }

        [TestMethod()]
        public void IsOkTests()
        {
            var v1 = Move.make(MoveType.ENPASSANT, new Square(Square.SQ_A1), new Square(Square.SQ_H8), new PieceType(PieceType.BISHOP));
            Assert.IsTrue(v1.is_ok());

            var v2 = Move.make(MoveType.PROMOTION, new Square(Square.SQ_A1), new Square(Square.SQ_A1), new PieceType(PieceType.BISHOP));
            Assert.IsFalse(v2.is_ok());
        }
    }
}