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
    public class SquareTests
    {
        
        [TestMethod()]
        public void FlipTests()
        {
            var v1 = Square.SQ_A1;
            var v2 = ~v1;
            Assert.AreEqual(Square.SQ_A8, v2);
            var v3 = ~v2;
            Assert.AreEqual(Square.SQ_A1, v3);

            var v5 = Square.SQ_H4;
            var v6 = ~v5;
            Assert.AreEqual(Square.SQ_H5, v6);
            var v7 = ~v6;
            Assert.AreEqual(Square.SQ_H4, v7);
        }

        [TestMethod()]
        public void IsOkTests()
        {
            var v1 = new Square(-5);
            Assert.IsFalse(v1.is_ok());

            var v2 = Square.SQ_A7;
            Assert.IsTrue(v2.is_ok());
        }

        [TestMethod()]
        public void OppositeColorTests()
        {
            var v1 = Square.SQ_A7;
            var v2 = Square.SQ_A6;
            Assert.IsTrue(Square.opposite_colors(v1, v2));

            var v3 = Square.SQ_A5;
            Assert.IsFalse(Square.opposite_colors(v1, v3));
        }

        [TestMethod()]
        public void RelativeSquareTests()
        {
            var v1 = Square.SQ_A7;
            Assert.AreEqual(Square.SQ_A2, Square.relative_square(Color.BLACK, v1));
        }

        [TestMethod()]
        public void MakeSquareTests()
        {
            var v1 = Square.make_square(File.FILE_C, Rank.RANK_3);
            Assert.AreEqual(Square.SQ_C3, v1);
        }

        [TestMethod()]
        public void FileAndRankOfTests()
        {
            var v1 = Square.make_square(File.FILE_C, Rank.RANK_3);
            Assert.AreEqual(File.FILE_C, Square.file_of(v1));
            Assert.AreEqual(Rank.RANK_3, Square.rank_of(v1));
        }
    }
}