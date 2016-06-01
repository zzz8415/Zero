using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zero.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Extensions.Tests
{
    [TestClass()]
    public class DateTimeExtensionsTests
    {
        [TestMethod()]
        public void GetMondayDateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSundayDateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToStandardStringTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToStandardStringTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FromUnixTimeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FromDateTimeTest()
        {
            var ticks = 1464335280L;
            var time = ticks.ToUnixDateTime();
            var t = time.ToUnixTimeStamp();

            var x = t;
            Assert.Fail();
        }
    }
}