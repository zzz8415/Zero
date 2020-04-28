using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zero.NETCore;
using System;
using System.Collections.Generic;
using System.Text;
using Zero.NETCore.Inject;

namespace Zero.NETCore.Tests
{
    [TestClass()]
    public class LogClientTests
    {
        [TestMethod()]
        public void WriteInfoTest()
        {
            var client = new LogClient();
            try
            {
                client.WriteInfo("WriteInfo");
                client.WriteInfo("WriteInfo");
                client.WriteTrace("WriteTrace");
                client.WriteTrace("WriteTrace");
                client.WriteCustom("WriteCustom", "WriteCustom");
                client.WriteCustom("WriteCustom", "WriteCustom");
                client.WriteDebug("WriteDebug");
                client.WriteDebug("WriteDebug");
                client.WriteError("WriteError");
                client.WriteError("WriteError");
                client.WriteFatal("WriteFatal");
                client.WriteFatal("WriteFatal");
                client.WriteException(new Exception("111"));
                client.WriteException(new Exception("222"));
                var y = 0;
                var x = 22 / y;
            }
            catch (Exception ex)
            {
                client.WriteException(ex);
            }

            Assert.Fail();
        }
    }
}