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
            Bitbases.init();

            
            // check some BSFTable fields
            //Assert.AreEqual(Square.SQ_G7, Utils.BSFTable[27]);
            //Assert.AreEqual(Square.SQ_B2, Utils.BSFTable[28]);
            //Assert.AreEqual(Square.SQ_B8, Utils.BSFTable[29]);
        }
    }
}


