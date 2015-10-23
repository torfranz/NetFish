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
    public class ExtMoveArrayWrapperTests
    {
        [TestMethod()]
        public void AssignmentTests()
        {
            var v1 = new ExtMoveArrayWrapper(new ExtMove[10], 5);
            var v2 = v1;

            Assert.AreEqual(5, v1.current);
            Assert.AreEqual(10, v1.table.Length);
            Assert.AreEqual(5, v2.current);
            Assert.AreEqual(10, v2.table.Length);

            ++v2;
            Assert.AreEqual(5, v1.current);
            Assert.AreEqual(10, v1.table.Length);
            Assert.AreEqual(6, v2.current);
            Assert.AreEqual(10, v2.table.Length);

            v2.set(new ExtMove[5]);
            Assert.AreEqual(5, v1.current);
            Assert.AreEqual(10, v1.table.Length);
            Assert.AreEqual(0, v2.current);
            Assert.AreEqual(5, v2.table.Length);
        }

        [TestMethod()]
        public void PartitionAndSortTests()
        {
            var table = new ExtMove[]
            {
                new ExtMove(new Move(1), new Value(-1)),
                new ExtMove(new Move(2), new Value(1)),
                new ExtMove(new Move(3), new Value(-2)),
                new ExtMove(new Move(4), new Value(11)),
                new ExtMove(new Move(5), new Value(0)),
                new ExtMove(new Move(6), new Value(5)),
                new ExtMove(new Move(7), new Value(-11)),
                new ExtMove(new Move(8), new Value(0)),
                new ExtMove(new Move(9), new Value(2)),
                new ExtMove(new Move(10), new Value(0))
            };

            var splitted = ExtMoveArrayWrapper.Partition(new ExtMoveArrayWrapper(table, 1),
                new ExtMoveArrayWrapper(table, 9));

            Assert.AreEqual(5, splitted.current);
            Assert.AreEqual(1, splitted[0].Move);
            Assert.AreEqual(4, splitted[2].Move);

            ExtMoveArrayWrapper.insertion_sort(new ExtMoveArrayWrapper(table, 1), splitted);

            Assert.AreEqual(1, splitted[0].Move);
            Assert.AreEqual(6, splitted[2].Move);
            Assert.AreEqual(2, splitted[4].Move);
        }
    }
}