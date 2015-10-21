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
    public class PieceTests
    {
        [TestMethod()]
        public void OperatorTests()
        {
            var v1 = Piece.W_PAWN;
            var v2 = Piece.W_KNIGHT;
            var v3 = v1 + v2;
            Assert.AreEqual(3, v3);

            var v4 = v3 + 1;
            Assert.AreEqual(4, v4);

            var v5 = 1 + v4;
            Assert.AreEqual(5, v5);

            var v6 = v5 - v1;
            Assert.AreEqual(4, v6);

            var v7 = v6 - 1;
            Assert.AreEqual(3, v7);
            /*
            Assert.AreEqual(-3, -v7.Value);

            var v8 = v7 * 2;
            Assert.AreEqual(6, v8.Value);

            var v9 = 2 * v8;
            Assert.AreEqual(12, v9.Value);

            var v10 = v9 / 2;
            Assert.AreEqual(6, v10.Value);

            var v11 = v10 / v2;
            Assert.AreEqual(3, v11.Value);
            */
        }

        [TestMethod()]
        public void MakePieceTest()
        {
            var v1 = Piece.make_piece(Color.WHITE, PieceType.BISHOP);
            Assert.AreEqual(Piece.W_BISHOP, v1);

            var v2 = Piece.make_piece(Color.BLACK, PieceType.KNIGHT);
            Assert.AreEqual(Piece.B_KNIGHT, v2);
        }

        [TestMethod()]
        public void TypeOfTests()
        {
            var v1 = Piece.make_piece(Color.WHITE, PieceType.BISHOP);
            Assert.AreEqual(PieceType.BISHOP, Piece.type_of(v1));
        }

        [TestMethod()]
        public void ColorOfTests()
        {
            var v1 = Piece.make_piece(Color.WHITE, PieceType.BISHOP);
            Assert.AreEqual(Color.WHITE, Piece.color_of(v1));
        }
    }
}