using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zero.Core.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Web.Tests
{
    [TestClass()]
    public class CacheHelperTests
    {
        [TestMethod()]
        public void GetTest()
        {
            var key = CacheHelper.RenderKey("prefix", "test");
            try
            {
                CacheHelper.Get(key, CacheTimeOption.HalfHour, () =>
                    {
                        string s = null;
                        return s;
                    });
            }
            catch (Exception ex)
            {

                throw;
            }
            Assert.Fail();
        }
    }
}