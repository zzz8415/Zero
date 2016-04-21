using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zero.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var uri = new Uri("http://192.169.0.1:3333/index");
            Console.WriteLine(uri);
        }
    }
}
