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
    public class PSQTTests
    {
        [TestMethod()]
        public void initTest()
        {
            PSQT.init();
                        
            // check some BSFTable fields
            Assert.AreEqual(165218744, PSQT.psq[0, 5, 7]);
            Assert.AreEqual(164956612, PSQT.psq[0, 5, 8]);
            Assert.AreEqual(165612000, PSQT.psq[0, 5, 9]);
            Assert.AreEqual(-165153203, PSQT.psq[1, 5, 7]);
            Assert.AreEqual(-165087688, PSQT.psq[1, 5, 8]);
            Assert.AreEqual(-165677536, PSQT.psq[1, 5, 9]);
        }
    }
}

