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
                new ExtMove(Move.Create(528), Value.Create(0)),
                new ExtMove(Move.Create(593), Value.Create(0)),
                new ExtMove(Move.Create(658), Value.Create(-32)),
                new ExtMove(Move.Create(723), Value.Create(0)),
                new ExtMove(Move.Create(788), Value.Create(0)),
                new ExtMove(Move.Create(853), Value.Create(-32)),
                new ExtMove(Move.Create(918), Value.Create(-32)),
                new ExtMove(Move.Create(983), Value.Create(-32)),
                new ExtMove(Move.Create(796), Value.Create(32)),
                new ExtMove(Move.Create(731), Value.Create(0))
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
                new ExtMove(Move.Create(528), Value.Create(-1008)),
                new ExtMove(Move.Create(593), Value.Create(-1008)),
                new ExtMove(Move.Create(658), Value.Create(-1069)),
                new ExtMove(Move.Create(788), Value.Create(-413)),
                new ExtMove(Move.Create(853), Value.Create(-944)),
                new ExtMove(Move.Create(918), Value.Create(-944)),
                new ExtMove(Move.Create(983), Value.Create(-944)),
                new ExtMove(Move.Create(1243), Value.Create(-413)),
                new ExtMove(Move.Create(536), Value.Create(-444)),
                new ExtMove(Move.Create(601), Value.Create(-444)),
                new ExtMove(Move.Create(666), Value.Create(-444)),
                new ExtMove(Move.Create(796), Value.Create(475)),
                new ExtMove(Move.Create(861), Value.Create(-444)),
                new ExtMove(Move.Create(926), Value.Create(-444)),
                new ExtMove(Move.Create(991), Value.Create(-444)),
                new ExtMove(Move.Create(75), Value.Create(-32)),
                new ExtMove(Move.Create(88), Value.Create(-444)),
                new ExtMove(Move.Create(82), Value.Create(-444)),
                new ExtMove(Move.Create(405), Value.Create(-444)),
                new ExtMove(Move.Create(407), Value.Create(-444)),
                new ExtMove(Move.Create(139), Value.Create(32)),
                new ExtMove(Move.Create(148), Value.Create(0)),
                new ExtMove(Move.Create(157), Value.Create(0)),
                new ExtMove(Move.Create(166), Value.Create(0)),
                new ExtMove(Move.Create(175), Value.Create(0)),
                new ExtMove(Move.Create(203), Value.Create(0)),
                new ExtMove(Move.Create(267), Value.Create(0)),
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