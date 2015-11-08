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

        [TestMethod()]
        public void PartitionAndSortTest1()
        {
            var table = new ExtMove[]
            {
                new ExtMove(new Move(528), new Value(-1008)),
                new ExtMove(new Move(593), new Value(-1008)),
                new ExtMove(new Move(658), new Value(-1069)),
                new ExtMove(new Move(788), new Value(-413)),
                new ExtMove(new Move(853), new Value(-944)),
                new ExtMove(new Move(918), new Value(-944)),
                new ExtMove(new Move(983), new Value(-944)),
                new ExtMove(new Move(1243), new Value(-413)),
                new ExtMove(new Move(536), new Value(-444)),
                new ExtMove(new Move(601), new Value(-444)),
                new ExtMove(new Move(666), new Value(-444)),
                new ExtMove(new Move(796), new Value(475)),
                new ExtMove(new Move(861), new Value(-444)),
                new ExtMove(new Move(926), new Value(-444)),
                new ExtMove(new Move(991), new Value(-444)),
                new ExtMove(new Move(75), new Value(-32)),
                new ExtMove(new Move(88), new Value(-444)),
                new ExtMove(new Move(82), new Value(-444)),
                new ExtMove(new Move(405), new Value(-444)),
                new ExtMove(new Move(407), new Value(-444)),
                new ExtMove(new Move(139), new Value(32)),
                new ExtMove(new Move(148), new Value(0)),
                new ExtMove(new Move(157), new Value(0)),
                new ExtMove(new Move(166), new Value(0)),
                new ExtMove(new Move(175), new Value(0)),
                new ExtMove(new Move(203), new Value(0)),
                new ExtMove(new Move(267), new Value(0)),
            };

            var splitted = ExtMoveArrayWrapper.Partition(new ExtMoveArrayWrapper(table, 0),
                new ExtMoveArrayWrapper(table, 26));

            Assert.AreEqual(2, splitted.current);
            Assert.AreEqual(139, table[0].Move);
            Assert.AreEqual(796, table[1].Move);
            Assert.AreEqual(593, table[11].Move);

            ExtMoveArrayWrapper.insertion_sort(new ExtMoveArrayWrapper(table, 0),
                splitted);

            Assert.AreEqual(796, table[0].Move);
            Assert.AreEqual(139, table[1].Move);
            Assert.AreEqual(593, table[11].Move);
        }

    }
}