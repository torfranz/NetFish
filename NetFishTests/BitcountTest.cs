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
    public class BitcountTests
    {
        [TestMethod()]
        public void popcount_Max15Test()
        {
            var result = Bitcount.popcount_Max15(282578800148862);
            Assert.AreEqual(12, result);
        }
    }
}