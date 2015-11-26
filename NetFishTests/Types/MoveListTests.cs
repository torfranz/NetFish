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
    public class MoveListTests
    {
        [TestMethod()]
        public void sizeTests()
        {
            PSQT.init();
            Bitboards.init();
            Position.init();

            var pos = new Position("rnbqkbnr/1ppppppp/p7/7Q/4P3/8/PPPP1PPP/RNB1KBNR b KQkq - 1 2", false, null);
            var ml = new MoveList(pos);
            Assert.AreEqual(16, ml.size());
        }
        
    }
}