using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zero.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Core.Extensions.Tests
{
    [TestClass]
    public class Base62ExtensionsTests
    {
        [TestMethod()]
        public void ToBase62Test()
        {
            var i = 778198016;
            var s = i.ToBase62();
            var ss = s.FromBase62<long>();
            Assert.Fail();
        }
    }
}