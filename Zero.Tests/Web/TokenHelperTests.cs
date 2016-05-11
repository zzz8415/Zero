using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zero.Core.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Web.Tests
{
    [TestClass()]
    public class TokenHelperTests
    {
        class Test
        {
            public string ID { get; set; }

            public string IP { get; set; }

            public string Ticks { get; set; }

        }
        [TestMethod()]
        public void SerializeTest()
        {
            var test = new Test {
                ID = "123",
                IP = "2343",
                Ticks = "333"
            };

            var token = TokenHelper.Serialize(test);
            var test1 = TokenHelper.Deserialize<Test>(token);
            Assert.Fail();
        }
    }
}