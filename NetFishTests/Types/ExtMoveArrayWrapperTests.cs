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
            var v2 = new ExtMoveArrayWrapper(v1);

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
        public void PartitionAndSortTest2()
        {
            var table = new ExtMove[]
            {
                new ExtMove(new Move(528), new Value(0)),
                new ExtMove(new Move(593), new Value(0)),
                new ExtMove(new Move(658), new Value(-32)),
                new ExtMove(new Move(723), new Value(0)),
                new ExtMove(new Move(788), new Value(0)),
                new ExtMove(new Move(853), new Value(-32)),
                new ExtMove(new Move(918), new Value(-32)),
                new ExtMove(new Move(983), new Value(-32)),
                new ExtMove(new Move(796), new Value(32)),
                new ExtMove(new Move(731), new Value(0))
            };

            var splitted = ExtMoveArrayWrapper.Partition(new ExtMoveArrayWrapper(table, 0),
                new ExtMoveArrayWrapper(table, 9));

            Assert.AreEqual(1, splitted.current);
            Assert.AreEqual(796, splitted[0].Move);
            Assert.AreEqual(593, splitted[1].Move);
            Assert.AreEqual(528, splitted[8].Move);

        }
    }
}