using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Zero.Core.Util.Tests
{
    [TestClass()]
    public class CryptoHelperTest
    {
        [TestMethod()]
        public void DES_EncryptTest()
        {
            try
            {
                string source = "a123456"; // TODO: 初始化为适当的值
                string key = "!&*(HDBHEK#^!(!0"; // TODO: 初始化为适当的值
                string expected = string.Empty; // TODO: 初始化为适当的值
                string actual;
                //actual = AES_Encrypt(source, key);
                //string s = AES_Decrypt(actual, key);


                actual = DES_Encrypt(source, key);

                string s = DES_Decrypt(actual.ToLower(), key);

                //Assert.AreEqual(expected, actual);
                Assert.Inconclusive("验证此测试方法的正确性。");
            }
            catch (Exception)
            {

                throw;
            }
            Assert.Fail();
        }

        private string BuildKey(string key, int length = 8)
        {
            return (key ?? string.Empty).Substring(0, length);
        }

        public string DES_Decrypt(string source, string key)
        {
            //将字符串转为字节数组  
            byte[] inputByteArray = new byte[source.Length / 2];
            for (int x = 0; x < source.Length / 2; x++)
            {
                int i = (Convert.ToInt32(source.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            string sKey = BuildKey(key);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                des.Key = ASCIIEncoding.UTF8.GetBytes(sKey);
                des.IV = ASCIIEncoding.UTF8.GetBytes(sKey);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        public string DES_Encrypt(string source, string key)
        {
            string sKey = BuildKey(key);
            byte[] btKey = Encoding.UTF8.GetBytes(sKey);
            byte[] btIV = Encoding.UTF8.GetBytes(sKey);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] inData = Encoding.UTF8.GetBytes(source);
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }

                    StringBuilder ret = new StringBuilder();
                    foreach (byte b in ms.ToArray())
                    {
                        ret.AppendFormat("{0:X2}", b);
                    }

                    return ret.ToString();
                }
            }
        }

        /// <summary>  
        /// AES解密（解密步骤）  
        /// 1，将BASE64字符串转为16进制数组  
        /// 2，将16进制数组转为字符串  
        /// 3，将字符串转为2进制数据  
        /// 4，用AES解密数据  
        /// </summary>  
        /// <param name="encryptedSource">已加密的内容</param>  
        /// <param name="key">密钥</param>  
        public static string AES_Decrypt(string encryptedSource, string key)
        {
            RijndaelManaged aes = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = Encoding.UTF8.GetBytes(key),
                IV = Encoding.UTF8.GetBytes("0231345874954435")
            };
            ICryptoTransform decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(encryptedSource);
                    cs.Write(xXml, 0, xXml.Length);
                }
                xBuff = ms.ToArray();
            }
            string Output = Encoding.UTF8.GetString(xBuff);
            return Output;
        }

        /// <summary>  
        ///AES加密（加密步骤）  
        ///1，加密字符串得到2进制数组；  
        ///2，将2禁止数组转为16进制；  
        ///3，进行base64编码  
        /// </summary>  
        /// <param name="toEncrypt">要加密的字符串</param>  
        /// <param name="key">密钥</param>  
        public static string AES_Encrypt(string toEncrypt, string key)
        {
            RijndaelManaged aes = new RijndaelManaged
            {
                Padding = PaddingMode.PKCS7,
                Key = Encoding.UTF8.GetBytes(key),
                IV = Encoding.UTF8.GetBytes("0231345874954435")
            };
            ICryptoTransform encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(toEncrypt);
                    cs.Write(xXml, 0, xXml.Length);
                }
                xBuff = ms.ToArray();
            }

            string Output = Convert.ToBase64String(xBuff);

            return Output;
        }

    }
}