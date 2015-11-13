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