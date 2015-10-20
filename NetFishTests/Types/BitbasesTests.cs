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
            Assert.AreEqual(522133503u, Bitbases.KPKBitbase[44]);
            Assert.AreEqual(4294901759u, Bitbases.KPKBitbase[45]);
            Assert.AreEqual(1061109759u, Bitbases.KPKBitbase[46]);
            Assert.AreEqual(4294901759u, Bitbases.KPKBitbase[47]);
            Assert.AreEqual(4244439039u, Bitbases.KPKBitbase[48]);
        }
    }
}


