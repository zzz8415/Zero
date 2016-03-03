using Zero.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Tests
{
    [TestClass()]
    public class CryptoHelperTest
    {
        [TestMethod()]
        public void HttpBase64DecodeTest()
        {
            try
            {
                var v = "ab2e9f1f01d6e02aabba9aa0c194cc14";
                string source = "zerozheng"; // TODO: 初始化为适当的值
                string key = "12312312";
                var v1 = CryptoHelper.AES_Encrypt("sa", "!M@U#L$I%T^C&H*A(T)");
                var value = CryptoHelper.MD5_Encrypt(source, key, Encoding.Default); // TODO: 初始化为适当的值

                Assert.AreEqual(v, value);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        ///DES_Encrypt 的测试
        ///</summary>
        [TestMethod()]
        public void DES_EncryptTest()
        {
            string source = "Data Source=ZERO-PC;Initial Catalog=OneFace;Persist Security Info=True;User ID=sa;Password=sa"; // TODO: 初始化为适当的值
            string key = "@neface!"; // TODO: 初始化为适当的值
            string expected = "abc4510dba6a006e"; // TODO: 初始化为适当的值
            string actual;
            actual = CryptoHelper.DES_Encrypt(source, key);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///DES_Decrypt_Client 的测试
        ///</summary>
        [TestMethod()]
        public void DES_Decrypt_ClientTest()
        {
            string source = "abc4510dba6a006e"; // TODO: 初始化为适当的值
            string key = "88888888"; // TODO: 初始化为适当的值
            string expected = "zero"; // TODO: 初始化为适当的值
            string actual;
            actual = CryptoHelper.DES_Decrypt_Client(source, key);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///DES_Encrypt_Client 的测试
        ///</summary>
        [TestMethod()]
        public void DES_Encrypt_ClientTest()
        {
            string source = "zero"; // TODO: 初始化为适当的值
            string key = "88888888"; // TODO: 初始化为适当的值
            string expected = "abc4510dba6a006e"; // TODO: 初始化为适当的值
            string actual;
            actual = CryptoHelper.DES_Encrypt_Client(source, key);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///DES_Encrypt 的测试
        ///</summary>
        [TestMethod()]
        public void DES_EncryptTest1()
        {
            string source = string.Empty; // TODO: 初始化为适当的值
            string key = string.Empty; // TODO: 初始化为适当的值
            string expected = string.Empty; // TODO: 初始化为适当的值
            string actual;
            actual = CryptoHelper.DES_Encrypt(source, key);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///DES_Decrypt 的测试
        ///</summary>
        [TestMethod()]
        public void DES_DecryptTest()
        {
            string source = "BA657D3A2BE32C0AEC989F5E8DB2E47205507E014DABE63D268E17321B2B186D49081BF73553F6B387688A27C0F52DF7C7636392713F6B49EBF26579D55C8552804EB67E2B3449260CDFBA9A13AD495FC621FCB11FC536B3ECE307697BF245E9A698E7812F8C6DE5DA6C30A117C2623FC8D0C895769ED809"; // TODO: 初始化为适当的值
            string key = string.Empty; // TODO: 初始化为适当的值
            string expected = string.Empty; // TODO: 初始化为适当的值
            string actual;
            actual = CryptoHelper.DES_Decrypt(source);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }
    }
}
