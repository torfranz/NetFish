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
    public class BitboardsTests
    {
        [TestMethod()]
        public void initTest()
        {
            Bitboards.init();
        }
    }
}