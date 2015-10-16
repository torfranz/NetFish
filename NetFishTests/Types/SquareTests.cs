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
        public void OperatorTests()
        {
            var v1 = new Square(new Square(1));
            var v2 = new Square(2);
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

            Assert.AreEqual(-3, -v7.Value);

            var v8 = v7 * 2;
            Assert.AreEqual(6, v8.Value);

            var v9 = 2 * v8;
            Assert.AreEqual(12, v9.Value);

            var v10 = v9 / 2;
            Assert.AreEqual(6, v10.Value);

            var v11 = v10 / v2;
            Assert.AreEqual(3, v11);
        }

        [TestMethod()]
        public void FlipTests()
        {
            var v1 = new Square(new Square(Square.SQ_A1));
            var v2 = ~v1;
            Assert.AreEqual(Square.SQ_A8, v2.Value);
            var v3 = ~v2;
            Assert.AreEqual(Square.SQ_A1, v3.Value);

            var v5 = new Square(new Square(Square.SQ_H4));
            var v6 = ~v5;
            Assert.AreEqual(Square.SQ_H5, v6.Value);
            var v7 = ~v6;
            Assert.AreEqual(Square.SQ_H4, v7.Value);
        }

        [TestMethod()]
        public void IsOkTests()
        {
            var v1 = new Square(-5);
            Assert.IsFalse(v1.is_ok());

            var v2 = new Square(Square.SQ_A7);
            Assert.IsTrue(v2.is_ok());
        }

        [TestMethod()]
        public void OppositeColorTests()
        {
            var v1 = new Square(Square.SQ_A7);
            var v2 = new Square(Square.SQ_A6);
            Assert.IsTrue(v1.opposite_colors(v2));

            var v3 = new Square(Square.SQ_A5);
            Assert.IsFalse(v1.opposite_colors(v3));
        }

        [TestMethod()]
        public void RelativeSquareTests()
        {
            var v1 = new Square(new Square(Square.SQ_A7));
            Assert.AreEqual(Square.SQ_A2, v1.relative_square(new Color(Color.BLACK)).Value);
        }

        [TestMethod()]
        public void MakeSquareTests()
        {
            var v1 = Square.make_square(new File(File.FILE_C), new Rank(Rank.RANK_3));
            Assert.AreEqual(Square.SQ_C3, v1.Value);
        }

        [TestMethod()]
        public void FileAndRankOfTests()
        {
            var v1 = Square.make_square(new File(File.FILE_C), new Rank(Rank.RANK_3));
            Assert.AreEqual(File.FILE_C, v1.file_of().Value);
            Assert.AreEqual(Rank.RANK_3, v1.rank_of().Value);
        }
    }
}