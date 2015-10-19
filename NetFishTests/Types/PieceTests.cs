﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var v1 = new Piece(new Piece(1));
            var v2 = new Piece(2);
            var v3 = v1 + v2;
            Assert.AreEqual(3, v3.Value);

            var v4 = v3 + 1;
            Assert.AreEqual(4, v4.Value);

            var v5 = 1 + v4;
            Assert.AreEqual(5, v5.Value);

            var v6 = v5 - v1;
            Assert.AreEqual(4, v6.Value);

            var v7 = v6 - 1;
            Assert.AreEqual(3, v7.Value);
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
            var v1 = Piece.make_piece(new Color(Color.WHITE), new PieceType(PieceType.BISHOP));
            Assert.AreEqual(Piece.W_BISHOP, v1.Value);

            var v2 = Piece.make_piece(new Color(Color.BLACK), new PieceType(PieceType.KNIGHT));
            Assert.AreEqual(Piece.B_KNIGHT, v2.Value);
        }

        [TestMethod()]
        public void TypeOfTests()
        {
            var v1 = Piece.make_piece(new Color(Color.WHITE), new PieceType(PieceType.BISHOP));
            Assert.AreEqual(PieceType.BISHOP, v1.type_of().Value);
        }

        [TestMethod()]
        public void ColorOfTests()
        {
            var v1 = Piece.make_piece(new Color(Color.WHITE), new PieceType(PieceType.BISHOP));
            Assert.AreEqual(Color.WHITE, v1.color_of().Value);
        }
    }
}