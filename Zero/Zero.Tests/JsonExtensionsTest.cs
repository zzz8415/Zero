using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Zero.Core.Extensions;

namespace Zero.Tests
{


    /// <summary>
    ///这是 JsonExtensionsTest 的测试类，旨在
    ///包含所有 JsonExtensionsTest 单元测试
    ///</summary>
    [TestClass()]
    public class JsonExtensionsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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
        ///ToJson 的测试
        ///</summary>
        [TestMethod()]
        public void ToJsonTest()
        {
            //object obj = new CMPParameters
            //{
            //    Gender = "男",
            //    MinBirthDate = "2010-10-10 12:30".ToDateTime().ToShortDateString(),
            //    MaxBirthDate = DateTime.Now.ToShortDateString(),
            //    PersonLogicTypes = 15,
            //    MinThreshold = 12,
            //    PersonID = "PersonID",
            //    ResultsLimit = 50,
            //    TaskPriority = 3
            //}; // TODO: 初始化为适当的值
            //JsonSerializerSettings settings = null; // TODO: 初始化为适当的值
            //string expected = string.Empty; // TODO: 初始化为适当的值
            //string actual;
            //actual = JsonExtensions.ToJson(obj, settings);
            //Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ToJson 的测试
        ///</summary>
        [TestMethod()]
        public void GTParameters()
        {
            //object obj = new GTParameters
            //{
            //    MinIod = 12,
            //    MaxIod = 35,
            //    TemplateSize = "1",
            //    MaxRollAngleDeviation = 1,
            //    MaxYawAngleDeviation = 1,
            //    MaxPitchAngleDeviation = 2,
            //    UseLivenessCheck = false,
            //    LivenessThreshold = 30.3f,
            //    MaxStreamDurationInFrames =2,
            //    FeaturePointsLevel = "2",
            //    FaceConfidenceThreshold = 1.3f,
            //    FaceQualityThreshold = 66

            //}; // TODO: 初始化为适当的值
            //JsonSerializerSettings settings = null; // TODO: 初始化为适当的值
            //string expected = string.Empty; // TODO: 初始化为适当的值
            //string actual;
            //actual = JsonExtensions.ToJson(obj, settings);
            //Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");

        }
    }
}
