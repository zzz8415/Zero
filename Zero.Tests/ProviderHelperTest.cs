using Zero.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zero.Tests
{
    
    
    /// <summary>
    ///这是 ProviderHelperTest 的测试类，旨在
    ///包含所有 ProviderHelperTest 单元测试
    ///</summary>
    [TestClass()]
    public class ProviderHelperTest
    {

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        //public TestContext TestContext
        //{
            //get
            //{
            //    return testContextInstance;
            //}
            //set
            //{
            //    testContextInstance = value;
            //}
        //}

        #region 附加测试特性
        // 
        //编写测试时，还可使用以下特性:
        //
        //使用 ClassInitialize 在运行类中的第一个测试前先运行代码
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //使用 ClassCleanup 在运行完类中的所有测试后再运行代码
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //使用 TestInitialize 在运行每个测试前先运行代码
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //使用 TestCleanup 在运行完每个测试后运行代码
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Providers 的测试
        ///</summary>
        [TestMethod()]
        public void ProvidersTest()
        {
            //try
            //{
            //    var c = 1;
            //    var count = DBConfigHelper.Instance.Providers.Count;
            //    Assert.AreEqual(count, c);
            //}
            //catch (Exception)
            //{
                
            //    throw;
            //}
            
            //Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///Providers 的测试
        ///</summary>
        //[TestMethod()]
        //public void ProvidersTest1()
        //{
        //    try
        //    {
        //        var c = 1;
        //        var count = DBConfigHelper.Providers.Count;
        //        Assert.AreEqual(count, c);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //    //Assert.Inconclusive("验证此测试方法的正确性。");
        //}

        /// <summary>
        ///Providers 的测试
        ///</summary>
        [TestMethod()]
        public void ProvidersTest2()
        {
            Queue<string> q = new Queue<string>();
            q.Enqueue("12");
            q.Enqueue("12");
            q.Enqueue("12");
            q.Enqueue("12");
            q.Enqueue("12");
            q.Enqueue("12");
            q.Enqueue("12");
            q.Take(5);
            Assert.AreEqual(2, q.Count);
            //try
            //{
            //    var w = "987654";
            //    var write = DBConfigHelper.Providers["Oracle"].WriteConnectionString;
            //    Assert.AreEqual(write, w);
            //    Assert.Inconclusive(write);
            //}
            //catch (Exception)
            //{

            //    throw;
            //}

            //Assert.Inconclusive("验证此测试方法的正确性。");
        }


    }
}
