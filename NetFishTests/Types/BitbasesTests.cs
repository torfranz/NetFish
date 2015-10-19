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
    public class BitbasesTests
    {
        [TestMethod()]
        public void initTest()
        {
            Bitboards.init();
            Bitbases.init();
                        
            // check some BSFTable fields
            Assert.AreEqual(4294901759, Bitbases.KPKBitbase[27]);
            Assert.AreEqual(4280229663, Bitbases.KPKBitbase[28]);
            Assert.AreEqual(4294901759, Bitbases.KPKBitbase[29]);
        }
    }
}


