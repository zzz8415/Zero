using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Zero.NETCore.Util
{
    /// <summary>
    /// 加密帮助类 
    /// </summary>
    public class CryptoHelper
    {

        /// <summary>
        /// 3DES加解密的默认密钥, 前8位作为向量
        /// </summary>
        private const string KEY_Complement = "Z!E@R#O$Z%H^E&N*G(L)I_N+G{J}U|N?";


        #region 使用Get传输替换关键字符为全角和半角转换
        /// <summary>
        /// 使用Get传输替换关键字符为全角
        /// </summary>
        /// <param name="UrlParam"></param>
        /// <returns></returns>
        public static string UrlParamUrlEncodeRun(string UrlParam)
        {
            UrlParam = UrlParam.Replace("+", "＋");
            UrlParam = UrlParam.Replace("=", "＝");
            UrlParam = UrlParam.Replace("&", "＆");
            UrlParam = UrlParam.Replace("?", "？");
            return UrlParam;
        }

        /// <summary>
        /// 使用Get传输替换关键字符为半角
        /// </summary>
        /// <param name="UrlParam"></param>
        /// <returns></returns>
        public static string UrlParamUrlDecodeRun(string UrlParam)
        {
            UrlParam = UrlParam.Replace("＋", "+");
            UrlParam = UrlParam.Replace("＝", "=");
            UrlParam = UrlParam.Replace("＆", "&");
            UrlParam = UrlParam.Replace("？", "?");
            return UrlParam;
        }
        #endregion

        #region  MD5加密

        /// <summary>
        /// 标准MD5加密
        /// </summary>
        /// <param name="source">待加密字符串</param>
        /// <param name="addKey">附加字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        public static string MD5_Encrypt(string source, string addKey, Encoding encoding)
        {
            if (addKey.Length > 0)
            {
                source += addKey;
            }
            return MD5_Encrypt(encoding.GetBytes(source));
        }


        /// <summary>
        /// 标准md5加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string MD5_Encrypt(byte[] source)
        {
            byte[] newSource = MD5.HashData(source);
            string byte2String = null;
            for (int i = 0; i < newSource.Length; i++)
            {
                string thisByte = newSource[i].ToString("x");
                if (thisByte.Length == 1)
                {
                    thisByte = "0" + thisByte;
                }

                byte2String += thisByte;
            }
            return byte2String;
        }

        /// <summary>
        /// 标准MD5加密
        /// </summary>
        /// <param name="source">待加密字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        public static string MD5_Encrypt(string source, Encoding encoding)
        {
            return MD5_Encrypt(source, string.Empty, encoding);
        }
        /// <summary>
        /// 标准MD5加密
        /// </summary>
        /// <param name="source">被加密的字符串</param>
        /// <returns></returns>
        public static string MD5_Encrypt(string source)
        {
            return MD5_Encrypt(source, string.Empty, Encoding.UTF8);
        }


        #endregion

        #region 密码加密
        /// <summary>
        /// 返回使用MD5加密后字符串
        /// </summary>
        /// <param name="strpwd">待加密字符串</param>
        /// <returns>加密后字符串</returns>
        public static string RegUser_MD5_Pwd(string strpwd)
        {
            #region

            string appkey = KEY_Complement; //，。加一特殊的字符后再加密，这样更安全些
                                            //strpwd += appkey;

            byte[] a = Encoding.Default.GetBytes(appkey);
            byte[] datSource = Encoding.Default.GetBytes(strpwd);
            byte[] b = new byte[a.Length + 4 + datSource.Length];

            int i;
            for (i = 0; i < datSource.Length; i++)
            {
                b[i] = datSource[i];
            }

            b[i++] = 163;
            b[i++] = 172;
            b[i++] = 161;
            b[i++] = 163;

            foreach (byte t in a)
            {
                b[i] = t;
                i++;
            }
            byte[] newSource = MD5.HashData(b);

            string byte2String = null;
            for (i = 0; i < newSource.Length; i++)
            {
                string thisByte = newSource[i].ToString("x");
                if (thisByte.Length == 1)
                {
                    thisByte = "0" + thisByte;
                }

                byte2String += thisByte;
            }
            return byte2String;

            #endregion
        }
        #endregion

        #region  DES 加解密
        /// <summary>
        /// Desc加密 Encoding.Default
        /// </summary>
        /// <param name="source">待加密字符</param>
        /// <param name="key">密钥</param>
        /// <returns>string</returns>
        public static string DES_Encrypt(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }




            //把字符串放到byte数组中  
            byte[] inputByteArray = Encoding.Default.GetBytes(source);

            // 密钥必须是8位，否则会报错 System.ArgumentException: 指定键的大小对于此算法无效。
            key = BuildKey(key, 8);

            //建立加密对象的密钥和偏移量  
            //原文使用ASCIIEncoding.ASCII方法的GetBytes方法  
            //使得输入密码必须输入英文文本  
            //            des.Key = UTF8Encoding.UTF8.GetBytes(key);
            //            des.IV  = UTF8Encoding.UTF8.GetBytes(key);
            StringBuilder ret = new();

            var des = DES.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = Encoding.UTF8.GetBytes(key);

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// 使用默认key 做 DES加密 Encoding.Default
        /// </summary>
        /// <param name="source">明文</param>
        /// <returns>密文</returns>
        public static string DES_Encrypt(string source)
        {

            return DES_Encrypt(source, KEY_Complement);
        }
        /// <summary>
        /// 使用默认key 做 DES解密 Encoding.Default
        /// </summary>
        /// <param name="source">密文</param>
        /// <returns>明文</returns>
        public static string DES_Decrypt(string source)
        {
            return DES_Decrypt(source, KEY_Complement);
        }

        /// <summary>
        /// DES解密 Encoding.Default
        /// </summary>
        /// <param name="source">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string DES_Decrypt(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            //将字符串转为字节数组  
            byte[] inputByteArray = new byte[source.Length / 2];
            for (int x = 0; x < source.Length / 2; x++)
            {
                int i = (Convert.ToInt32(source.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            // 密钥必须是8位，否则会报错 System.ArgumentException: 指定键的大小对于此算法无效。
            key = BuildKey(key, 8);
            //建立加密对象的密钥和偏移量，此值重要，不能修改  
            var des = DES.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = Encoding.UTF8.GetBytes(key);

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #region 配合JS用的C#版DES加解密方法及相关函数

        /// <summary>
        /// 与客户端通用的des加密
        /// </summary>
        /// <param name="source">明文</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string DES_Encrypt_Client(string source, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return StringToHex(Des(key, source, true, false, string.Empty));
        }

        /// <summary>
        /// 与客户端通用的des解密
        /// </summary>
        /// <param name="source">密文</param>
        /// <param name="key">密匙</param>
        /// <returns></returns>
        public static string DES_Decrypt_Client(string source, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string ret = Des(key, HexTostring(source), false, false, string.Empty);
            return ret;
        }

        /// <summary>
        /// 把字符串转换为16进制字符串
        /// 如：a变成61（即10进制的97）；abc变成616263
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string StringToHex(string s)
        {
            string r = "";
            string[] hexes = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
            for (int i = 0; i < (s.Length); i++)
            {
                r += hexes[RM(s[i], 4)] + hexes[s[i] & 0xf];
            }
            return r;
        }

        /// <summary>
        /// 16进制字符串转换为字符串
        /// 如：61（即10进制的97）变成a；616263变成abc
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string HexTostring(string s)
        {
            string ret = string.Empty;

            for (int i = 0; i < s.Length; i += 2)
            {
                int sxx = Convert.ToInt32(s.Substring(i, 2), 16);
                ret += (char)sxx;
            }
            return ret;
        }

        /// <summary>
        /// 带符号位右移（类似于js的>>>）
        /// </summary>
        /// <param name="a">用于右移的操作数</param>
        /// <param name="bit">右移位数</param>
        /// <returns></returns>
        private static int RM(int a, int bit)
        {
            unchecked
            {
                uint b = (uint)a;
                b >>= bit;
                return (int)b;
            }
        }

        /// <summary>
        /// 加解密主调方法
        /// </summary>
        /// <param name="beinetkey">密钥</param>
        /// <param name="message">加密时为string，解密时为byte[]</param>
        /// <param name="encrypt">true：加密；false：解密</param>
        /// <param name="mode">true：CBC mode；false：非CBC mode</param>
        /// <param name="iv">初始化向量</param>
        /// <returns></returns>
        private static string Des(string beinetkey, string message, bool encrypt, bool mode, string iv)
        {
            //declaring this locally speeds things up a bit
            long[] spfunction1 = { 0x1010400, 0, 0x10000, 0x1010404, 0x1010004, 0x10404, 0x4, 0x10000, 0x400, 0x1010400, 0x1010404, 0x400, 0x1000404, 0x1010004, 0x1000000, 0x4, 0x404, 0x1000400, 0x1000400, 0x10400, 0x10400, 0x1010000, 0x1010000, 0x1000404, 0x10004, 0x1000004, 0x1000004, 0x10004, 0, 0x404, 0x10404, 0x1000000, 0x10000, 0x1010404, 0x4, 0x1010000, 0x1010400, 0x1000000, 0x1000000, 0x400, 0x1010004, 0x10000, 0x10400, 0x1000004, 0x400, 0x4, 0x1000404, 0x10404, 0x1010404, 0x10004, 0x1010000, 0x1000404, 0x1000004, 0x404, 0x10404, 0x1010400, 0x404, 0x1000400, 0x1000400, 0, 0x10004, 0x10400, 0, 0x1010004 };
            long[] spfunction2 = { -0x7fef7fe0, -0x7fff8000, 0x8000, 0x108020, 0x100000, 0x20, -0x7fefffe0, -0x7fff7fe0, -0x7fffffe0, -0x7fef7fe0, -0x7fef8000, -0x80000000, -0x7fff8000, 0x100000, 0x20, -0x7fefffe0, 0x108000, 0x100020, -0x7fff7fe0, 0, -0x80000000, 0x8000, 0x108020, -0x7ff00000, 0x100020, -0x7fffffe0, 0, 0x108000, 0x8020, -0x7fef8000, -0x7ff00000, 0x8020, 0, 0x108020, -0x7fefffe0, 0x100000, -0x7fff7fe0, -0x7ff00000, -0x7fef8000, 0x8000, -0x7ff00000, -0x7fff8000, 0x20, -0x7fef7fe0, 0x108020, 0x20, 0x8000, -0x80000000, 0x8020, -0x7fef8000, 0x100000, -0x7fffffe0, 0x100020, -0x7fff7fe0, -0x7fffffe0, 0x100020, 0x108000, 0, -0x7fff8000, 0x8020, -0x80000000, -0x7fefffe0, -0x7fef7fe0, 0x108000 };
            long[] spfunction3 = { 0x208, 0x8020200, 0, 0x8020008, 0x8000200, 0, 0x20208, 0x8000200, 0x20008, 0x8000008, 0x8000008, 0x20000, 0x8020208, 0x20008, 0x8020000, 0x208, 0x8000000, 0x8, 0x8020200, 0x200, 0x20200, 0x8020000, 0x8020008, 0x20208, 0x8000208, 0x20200, 0x20000, 0x8000208, 0x8, 0x8020208, 0x200, 0x8000000, 0x8020200, 0x8000000, 0x20008, 0x208, 0x20000, 0x8020200, 0x8000200, 0, 0x200, 0x20008, 0x8020208, 0x8000200, 0x8000008, 0x200, 0, 0x8020008, 0x8000208, 0x20000, 0x8000000, 0x8020208, 0x8, 0x20208, 0x20200, 0x8000008, 0x8020000, 0x8000208, 0x208, 0x8020000, 0x20208, 0x8, 0x8020008, 0x20200 };
            long[] spfunction4 = { 0x802001, 0x2081, 0x2081, 0x80, 0x802080, 0x800081, 0x800001, 0x2001, 0, 0x802000, 0x802000, 0x802081, 0x81, 0, 0x800080, 0x800001, 0x1, 0x2000, 0x800000, 0x802001, 0x80, 0x800000, 0x2001, 0x2080, 0x800081, 0x1, 0x2080, 0x800080, 0x2000, 0x802080, 0x802081, 0x81, 0x800080, 0x800001, 0x802000, 0x802081, 0x81, 0, 0, 0x802000, 0x2080, 0x800080, 0x800081, 0x1, 0x802001, 0x2081, 0x2081, 0x80, 0x802081, 0x81, 0x1, 0x2000, 0x800001, 0x2001, 0x802080, 0x800081, 0x2001, 0x2080, 0x800000, 0x802001, 0x80, 0x800000, 0x2000, 0x802080 };
            long[] spfunction5 = { 0x100, 0x2080100, 0x2080000, 0x42000100, 0x80000, 0x100, 0x40000000, 0x2080000, 0x40080100, 0x80000, 0x2000100, 0x40080100, 0x42000100, 0x42080000, 0x80100, 0x40000000, 0x2000000, 0x40080000, 0x40080000, 0, 0x40000100, 0x42080100, 0x42080100, 0x2000100, 0x42080000, 0x40000100, 0, 0x42000000, 0x2080100, 0x2000000, 0x42000000, 0x80100, 0x80000, 0x42000100, 0x100, 0x2000000, 0x40000000, 0x2080000, 0x42000100, 0x40080100, 0x2000100, 0x40000000, 0x42080000, 0x2080100, 0x40080100, 0x100, 0x2000000, 0x42080000, 0x42080100, 0x80100, 0x42000000, 0x42080100, 0x2080000, 0, 0x40080000, 0x42000000, 0x80100, 0x2000100, 0x40000100, 0x80000, 0, 0x40080000, 0x2080100, 0x40000100 };
            long[] spfunction6 = { 0x20000010, 0x20400000, 0x4000, 0x20404010, 0x20400000, 0x10, 0x20404010, 0x400000, 0x20004000, 0x404010, 0x400000, 0x20000010, 0x400010, 0x20004000, 0x20000000, 0x4010, 0, 0x400010, 0x20004010, 0x4000, 0x404000, 0x20004010, 0x10, 0x20400010, 0x20400010, 0, 0x404010, 0x20404000, 0x4010, 0x404000, 0x20404000, 0x20000000, 0x20004000, 0x10, 0x20400010, 0x404000, 0x20404010, 0x400000, 0x4010, 0x20000010, 0x400000, 0x20004000, 0x20000000, 0x4010, 0x20000010, 0x20404010, 0x404000, 0x20400000, 0x404010, 0x20404000, 0, 0x20400010, 0x10, 0x4000, 0x20400000, 0x404010, 0x4000, 0x400010, 0x20004010, 0, 0x20404000, 0x20000000, 0x400010, 0x20004010 };
            long[] spfunction7 = { 0x200000, 0x4200002, 0x4000802, 0, 0x800, 0x4000802, 0x200802, 0x4200800, 0x4200802, 0x200000, 0, 0x4000002, 0x2, 0x4000000, 0x4200002, 0x802, 0x4000800, 0x200802, 0x200002, 0x4000800, 0x4000002, 0x4200000, 0x4200800, 0x200002, 0x4200000, 0x800, 0x802, 0x4200802, 0x200800, 0x2, 0x4000000, 0x200800, 0x4000000, 0x200800, 0x200000, 0x4000802, 0x4000802, 0x4200002, 0x4200002, 0x2, 0x200002, 0x4000000, 0x4000800, 0x200000, 0x4200800, 0x802, 0x200802, 0x4200800, 0x802, 0x4000002, 0x4200802, 0x4200000, 0x200800, 0, 0x2, 0x4200802, 0, 0x200802, 0x4200000, 0x800, 0x4000002, 0x4000800, 0x800, 0x200002 };
            long[] spfunction8 = { 0x10001040, 0x1000, 0x40000, 0x10041040, 0x10000000, 0x10001040, 0x40, 0x10000000, 0x40040, 0x10040000, 0x10041040, 0x41000, 0x10041000, 0x41040, 0x1000, 0x40, 0x10040000, 0x10000040, 0x10001000, 0x1040, 0x41000, 0x40040, 0x10040040, 0x10041000, 0x1040, 0, 0, 0x10040040, 0x10000040, 0x10001000, 0x41040, 0x40000, 0x41040, 0x40000, 0x10041000, 0x1000, 0x40, 0x10040040, 0x1000, 0x41040, 0x10001000, 0x40, 0x10000040, 0x10040000, 0x10040040, 0x10000000, 0x40000, 0x10001040, 0, 0x10041040, 0x40040, 0x10000040, 0x10040000, 0x10001000, 0x10001040, 0, 0x10041040, 0x41000, 0x41000, 0x1040, 0x1040, 0x40040, 0x10000000, 0x10041000 };


            //create the 16 or 48 subkeys we will need
            int[] keys = Des_CreateKeys(beinetkey);
            int m = 0;
            int i, j;
            int temp, right1, right2, left, right;
            int[] looping;
            int cbcleft = 0, cbcleft2 = 0, cbcright = 0, cbcright2 = 0;
            int endloop;
            int loopinc;
            int len = message.Length;
            int chunk = 0;
            //set up the loops for single and triple des
            int iterations = keys.Length == 32 ? 3 : 9;//single or triple des
            if (iterations == 3)
            {
                looping = encrypt ? new int[] { 0, 32, 2 } : new int[] { 30, -2, -2 };
            }
            else { looping = encrypt ? new int[] { 0, 32, 2, 62, 30, -2, 64, 96, 2 } : new int[] { 94, 62, -2, 32, 64, 2, 30, -2, -2 }; }

            if (encrypt)
            {
                message += "\0\0\0\0\0\0\0\0";//pad the message out with null bytes
            }
            //store the result here
            //List<byte> result = new List<byte>();
            //List<byte> tempresult = new List<byte>();
            string result = string.Empty;
            string tempresult = string.Empty;

            if (mode)
            {//CBC mode
                int[] tmp = { 0, 0, 0, 0, 0, 0, 0, 0 };
                int pos = 24;
                int iTmp = 0;
                while (m < iv.Length && iTmp < tmp.Length)
                {
                    if (pos < 0)
                    {
                        pos = 24;
                    }

                    tmp[iTmp++] = iv[m++] << pos;
                    pos -= 8;
                }
                cbcleft = tmp[0] | tmp[1] | tmp[2] | tmp[3];
                cbcright = tmp[4] | tmp[5] | tmp[6] | tmp[7];

                //cbcleft = (iv[m++] << 24) | (iv[m++] << 16) | (iv[m++] << 8) | iv[m++];
                //cbcright = (iv[m++] << 24) | (iv[m++] << 16) | (iv[m++] << 8) | iv[m++];
                m = 0;
            }

            //loop through each 64 bit chunk of the message
            while (m < len)
            {
                if (encrypt)
                {/*加密时按双字节操作*/
                    left = (message[m++] << 16) | message[m++];
                    right = (message[m++] << 16) | message[m++];
                }
                else
                {
                    left = (message[m++] << 24) | (message[m++] << 16) | (message[m++] << 8) | message[m++];
                    right = (message[m++] << 24) | (message[m++] << 16) | (message[m++] << 8) | message[m++];
                }
                //for Cipher Block Chaining mode,xor the message with the previous result
                if (mode)
                {
                    if (encrypt)
                    {
                        left ^= cbcleft; right ^= cbcright;
                    }
                    else
                    {
                        cbcleft2 = cbcleft; cbcright2 = cbcright; cbcleft = left; cbcright = right;
                    }
                }

                //first each 64 but chunk of the message must be permuted according to IP
                temp = (RM(left, 4) ^ right) & 0x0f0f0f0f; right ^= temp; left ^= (temp << 4);
                temp = (RM(left, 16) ^ right) & 0x0000ffff; right ^= temp; left ^= (temp << 16);
                temp = (RM(right, 2) ^ left) & 0x33333333; left ^= temp; right ^= (temp << 2);
                temp = (RM(right, 8) ^ left) & 0x00ff00ff; left ^= temp; right ^= (temp << 8);
                temp = (RM(left, 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);

                left = ((left << 1) | RM(left, 31));
                right = ((right << 1) | RM(right, 31));

                //do this either 1 or 3 times for each chunk of the message
                for (j = 0; j < iterations; j += 3)
                {
                    endloop = looping[j + 1];
                    loopinc = looping[j + 2];
                    //now go through and perform the encryption or decryption 
                    for (i = looping[j]; i != endloop; i += loopinc)
                    {//for efficiency
                        right1 = right ^ keys[i];
                        right2 = (RM(right, 4) | (right << 28)) ^ keys[i + 1];
                        //the result is attained by passing these bytes through the S selection functions
                        temp = left;
                        left = right;
                        right = (int)(temp ^ (spfunction2[RM(right1, 24) & 0x3f] | spfunction4[RM(right1, 16) & 0x3f] | spfunction6[RM(right1, 8) & 0x3f] | spfunction8[right1 & 0x3f] | spfunction1[RM(right2, 24) & 0x3f] | spfunction3[RM(right2, 16) & 0x3f] | spfunction5[RM(right2, 8) & 0x3f] | spfunction7[right2 & 0x3f]));
                    }
                    temp = left; left = right; right = temp;//unreverse left and right
                }//for either 1 or 3 iterations

                //move then each one bit to the right
                left = (RM(left, 1) | (left << 31));
                right = (RM(right, 1) | (right << 31));

                //now perform IP-1,which is IP in the opposite direction
                temp = (RM(left, 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);
                temp = (RM(right, 8) ^ left) & 0x00ff00ff; left ^= temp; right ^= (temp << 8);
                temp = (RM(right, 2) ^ left) & 0x33333333; left ^= temp; right ^= (temp << 2);
                temp = (RM(left, 16) ^ right) & 0x0000ffff; right ^= temp; left ^= (temp << 16);
                temp = (RM(left, 4) ^ right) & 0x0f0f0f0f; right ^= temp; left ^= (temp << 4);

                //for Cipher Block Chaining mode,xor the message with the previous result
                if (mode)
                {
                    if (encrypt)
                    {
                        cbcleft = left; cbcright = right;
                    }
                    else
                    {
                        left ^= cbcleft2; right ^= cbcright2;
                    }
                }
                //int[] arrInt;
                if (encrypt)
                {
                    //arrInt = new int[]{ RM(left, 24), (RM(left, 16) & 0xff), (RM(left, 8) & 0xff), (left & 0xff), RM(right, 24), (RM(right, 16) & 0xff), (RM(right, 8) & 0xff), (right & 0xff) };
                    tempresult += string.Concat((char)RM(left, 24),
                        (char)(RM(left, 16) & 0xff),
                        (char)(RM(left, 8) & 0xff),
                        (char)(left & 0xff),
                        (char)RM(right, 24),
                        (char)(RM(right, 16) & 0xff),
                        (char)(RM(right, 8) & 0xff),
                        (char)(right & 0xff));
                }
                else
                {
                    // 解密时，最后一个字符如果是\0，去除
                    //arrInt = new int[] { (RM(left, 16) & 0xffff), (left & 0xffff), (RM(right, 16) & 0xffff), (right & 0xffff) };
                    int tmpch = (RM(left, 16) & 0xffff);
                    if (tmpch != 0)
                    {
                        tempresult += (char)tmpch;
                    }

                    tmpch = (left & 0xffff);
                    if (tmpch != 0)
                    {
                        tempresult += (char)tmpch;
                    }

                    tmpch = (RM(right, 16) & 0xffff);
                    if (tmpch != 0)
                    {
                        tempresult += (char)tmpch;
                    }

                    tmpch = (right & 0xffff);
                    if (tmpch != 0)
                    {
                        tempresult += (char)tmpch;
                    }
                    //tempresult += String.Concat((char)(RM(left, 16) & 0xffff),
                    //    (char)(left & 0xffff),
                    //    (char)(RM(right, 16) & 0xffff),
                    //    (char)(right & 0xffff));
                }/*解密时输出双字节*/
                //byte[] arrByte = new byte[arrInt.Length];
                //for (int loop = 0; loop < arrInt.Length; loop++)
                //{
                //    tempresult.Add(byte.Parse(arrInt[loop].ToString()));
                //    //arrByte[loop] = byte.Parse(arrInt[loop].ToString());
                //}
                //tempresult.Add(arrByte;// System.Text.Encoding.Unicode.GetString(arrByte);

                chunk += encrypt ? 16 : 8;
                if (chunk == 512)
                {
                    //result.AddRange(tempresult);tempresult.Clear(); 
                    result += tempresult; tempresult = string.Empty;
                    chunk = 0;
                }
            }//for every 8 characters,or 64 bits in the message

            //return the result as an array

            //result.AddRange(tempresult);
            //return result.ToArray();
            return result + tempresult;
        }//end of des

        /// <summary>
        /// this takes as input a 64 bit beinetkey(even though only 56 bits are used)
        /// as an array of 2 integers,and returns 16 48 bit keys
        /// </summary>
        /// <param name="beinetkey"></param>
        /// <returns></returns>
        private static int[] Des_CreateKeys(string beinetkey)
        {
            //declaring this locally speeds things up a bit
            int[] pc2bytes0 = { 0, 0x4, 0x20000000, 0x20000004, 0x10000, 0x10004, 0x20010000, 0x20010004, 0x200, 0x204, 0x20000200, 0x20000204, 0x10200, 0x10204, 0x20010200, 0x20010204 };
            int[] pc2bytes1 = { 0, 0x1, 0x100000, 0x100001, 0x4000000, 0x4000001, 0x4100000, 0x4100001, 0x100, 0x101, 0x100100, 0x100101, 0x4000100, 0x4000101, 0x4100100, 0x4100101 };
            int[] pc2bytes2 = { 0, 0x8, 0x800, 0x808, 0x1000000, 0x1000008, 0x1000800, 0x1000808, 0, 0x8, 0x800, 0x808, 0x1000000, 0x1000008, 0x1000800, 0x1000808 };
            int[] pc2bytes3 = { 0, 0x200000, 0x8000000, 0x8200000, 0x2000, 0x202000, 0x8002000, 0x8202000, 0x20000, 0x220000, 0x8020000, 0x8220000, 0x22000, 0x222000, 0x8022000, 0x8222000 };
            int[] pc2bytes4 = { 0, 0x40000, 0x10, 0x40010, 0, 0x40000, 0x10, 0x40010, 0x1000, 0x41000, 0x1010, 0x41010, 0x1000, 0x41000, 0x1010, 0x41010 };
            int[] pc2bytes5 = { 0, 0x400, 0x20, 0x420, 0, 0x400, 0x20, 0x420, 0x2000000, 0x2000400, 0x2000020, 0x2000420, 0x2000000, 0x2000400, 0x2000020, 0x2000420 };
            int[] pc2bytes6 = { 0, 0x10000000, 0x80000, 0x10080000, 0x2, 0x10000002, 0x80002, 0x10080002, 0, 0x10000000, 0x80000, 0x10080000, 0x2, 0x10000002, 0x80002, 0x10080002 };
            int[] pc2bytes7 = { 0, 0x10000, 0x800, 0x10800, 0x20000000, 0x20010000, 0x20000800, 0x20010800, 0x20000, 0x30000, 0x20800, 0x30800, 0x20020000, 0x20030000, 0x20020800, 0x20030800 };
            int[] pc2bytes8 = { 0, 0x40000, 0, 0x40000, 0x2, 0x40002, 0x2, 0x40002, 0x2000000, 0x2040000, 0x2000000, 0x2040000, 0x2000002, 0x2040002, 0x2000002, 0x2040002 };
            int[] pc2bytes9 = { 0, 0x10000000, 0x8, 0x10000008, 0, 0x10000000, 0x8, 0x10000008, 0x400, 0x10000400, 0x408, 0x10000408, 0x400, 0x10000400, 0x408, 0x10000408 };
            int[] pc2bytes10 = { 0, 0x20, 0, 0x20, 0x100000, 0x100020, 0x100000, 0x100020, 0x2000, 0x2020, 0x2000, 0x2020, 0x102000, 0x102020, 0x102000, 0x102020 };
            int[] pc2bytes11 = { 0, 0x1000000, 0x200, 0x1000200, 0x200000, 0x1200000, 0x200200, 0x1200200, 0x4000000, 0x5000000, 0x4000200, 0x5000200, 0x4200000, 0x5200000, 0x4200200, 0x5200200 };
            int[] pc2bytes12 = { 0, 0x1000, 0x8000000, 0x8001000, 0x80000, 0x81000, 0x8080000, 0x8081000, 0x10, 0x1010, 0x8000010, 0x8001010, 0x80010, 0x81010, 0x8080010, 0x8081010 };
            int[] pc2bytes13 = { 0, 0x4, 0x100, 0x104, 0, 0x4, 0x100, 0x104, 0x1, 0x5, 0x101, 0x105, 0x1, 0x5, 0x101, 0x105 };

            //how many iterations(1 for des,3 for triple des)
            int iterations = beinetkey.Length >= 24 ? 3 : 1;
            //stores the return keys
            int[] keys = new int[32 * iterations];
            //now define the left shifts which need to be done
            int[] shifts = { 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0 };
            //other variables
            int left, right;
            int lefttemp;
            int righttemp;
            int m = 0, n = 0;
            int temp;

            for (int j = 0; j < iterations; j++)
            {//either 1 or 3 iterations
                int[] tmp = { 0, 0, 0, 0, 0, 0, 0, 0 };
                int pos = 24;
                int iTmp = 0;
                while (m < beinetkey.Length && iTmp < tmp.Length)
                {
                    if (pos < 0)
                    {
                        pos = 24;
                    }

                    tmp[iTmp++] = beinetkey[m++] << pos;
                    pos -= 8;
                }
                left = tmp[0] | tmp[1] | tmp[2] | tmp[3];
                right = tmp[4] | tmp[5] | tmp[6] | tmp[7];

                //left = (beinetkey[m++] << 24) | (beinetkey[m++] << 16) | (beinetkey[m++] << 8) | beinetkey[m++];
                //right = (beinetkey[m++] << 24) | (beinetkey[m++] << 16) | (beinetkey[m++] << 8) | beinetkey[m++];

                temp = (RM(left, 4) ^ right) & 0x0f0f0f0f; right ^= temp; left ^= (temp << 4);
                temp = (RM(right, -16) ^ left) & 0x0000ffff; left ^= temp; right ^= (temp << -16);
                temp = (RM(left, 2) ^ right) & 0x33333333; right ^= temp; left ^= (temp << 2);
                temp = (RM(right, -16) ^ left) & 0x0000ffff; left ^= temp; right ^= (temp << -16);
                temp = (RM(left, 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);
                temp = (RM(right, 8) ^ left) & 0x00ff00ff; left ^= temp; right ^= (temp << 8);
                temp = (RM(left, 1) ^ right) & 0x55555555; right ^= temp; left ^= (temp << 1);

                //the right side needs to be shifted and to get the last four bits of the left side
                temp = (left << 8) | (RM(right, 20) & 0x000000f0);
                //left needs to be put upside down
                left = (right << 24) | ((right << 8) & 0xff0000) | (RM(right, 8) & 0xff00) | (RM(right, 24) & 0xf0);
                right = temp;

                //now go through and perform these shifts on the left and right keys
                for (int i = 0; i < shifts.Length; i++)
                {
                    //shift the keys either one or two bits to the left
                    if (shifts[i] == 1)
                    {
                        left = (left << 2) | RM(left, 26); right = (right << 2) | RM(right, 26);
                    }
                    else
                    {
                        left = (left << 1) | RM(left, 27); right = (right << 1) | RM(right, 27);
                    }
                    left &= -0xf; right &= -0xf;

                    //now apply PC-2,in such a way that E is easier when encrypting or decrypting
                    //this conversion will look like PC-2 except only the last 6 bits of each byte are used
                    //rather than 48 consecutive bits and the order of lines will be according to 
                    //how the S selection functions will be applied:S2,S4,S6,S8,S1,S3,S5,S7
                    lefttemp = pc2bytes0[RM(left, 28)] | pc2bytes1[RM(left, 24) & 0xf]
                   | pc2bytes2[RM(left, 20) & 0xf] | pc2bytes3[RM(left, 16) & 0xf]
                   | pc2bytes4[RM(left, 12) & 0xf] | pc2bytes5[RM(left, 8) & 0xf]
                   | pc2bytes6[RM(left, 4) & 0xf];
                    righttemp = pc2bytes7[RM(right, 28)] | pc2bytes8[RM(right, 24) & 0xf]
                   | pc2bytes9[RM(right, 20) & 0xf] | pc2bytes10[RM(right, 16) & 0xf]
                   | pc2bytes11[RM(right, 12) & 0xf] | pc2bytes12[RM(right, 8) & 0xf]
                   | pc2bytes13[RM(right, 4) & 0xf];
                    temp = (RM(righttemp, 16) ^ lefttemp) & 0x0000ffff;
                    keys[n++] = lefttemp ^ temp; keys[n++] = righttemp ^ (temp << 16);
                }
            }//for each iterations
            //return the keys we"ve created
            return keys;
        }//end of des_createKeys

        #endregion
        #endregion


        #region 3DES加解密

        /// <summary>
        /// 使用指定的key和iv，加密input数据
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key">密钥，必须为24位长度</param>
        /// <param name="iv">微量，必须为8位长度</param>
        /// <returns></returns>
        public static string TripleDES_Encrypt(string input, string key = null, string iv = null)
        {
            key = BuildKey(key, 24);

            iv = BuildKey(iv, 8);

            byte[] arrKey = Encoding.UTF8.GetBytes(key);
            byte[] arrIV = Encoding.UTF8.GetBytes(iv);

            // 获取加密后的字节数据
            byte[] arrData = Encoding.UTF8.GetBytes(input);
            byte[] result = TripleDesEncrypt(arrKey, arrIV, arrData);

            // 转换为16进制字符串
            StringBuilder ret = new();
            foreach (byte b in result)
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// 使用指定的key和iv，解密input数据
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key">密钥，必须为24位长度</param>
        /// <param name="iv">微量，必须为8位长度</param>
        /// <returns></returns>
        public static string TripleDES_Decrypt(string input, string key = null, string iv = null)
        {
            key = BuildKey(key, 24);

            iv = BuildKey(iv, 8);

            byte[] arrKey = Encoding.UTF8.GetBytes(key);
            byte[] arrIV = Encoding.UTF8.GetBytes(iv);

            // 获取加密后的字节数据
            int len = input.Length / 2;
            byte[] arrData = new byte[len];
            for (int x = 0; x < len; x++)
            {
                int i = (Convert.ToInt32(input.Substring(x * 2, 2), 16));
                arrData[x] = (byte)i;
            }

            byte[] result = TripleDesDecrypt(arrKey, arrIV, arrData);
            return Encoding.UTF8.GetString(result);
        }


        #region TripleDesEncrypt加密(3DES加密)
        /// <summary>
        /// 3Des加密，密钥长度必需是24字节
        /// </summary>
        /// <param name="key">密钥字节数组</param>
        /// <param name="iv">向量字节数组</param>
        /// <param name="source">源字节数组</param>
        /// <returns>加密后的字节数组</returns>
        private static byte[] TripleDesEncrypt(byte[] key, byte[] iv, byte[] source)
        {
            using var dsp = TripleDES.Create();

            dsp.Mode = CipherMode.CBC; // 默认值
            dsp.Padding = PaddingMode.PKCS7;// 默认值

            using MemoryStream mStream = new();
            using CryptoStream cStream = new(mStream, dsp.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            cStream.Write(source, 0, source.Length);
            cStream.FlushFinalBlock();
            return mStream.ToArray();

        }
        #endregion

        #region TripleDesDecrypt解密(3DES解密)
        /// <summary>
        /// 3Des解密，密钥长度必需是24字节
        /// </summary>
        /// <param name="key">密钥字节数组</param>
        /// <param name="iv">向量字节数组</param>
        /// <param name="source">加密后的字节数组</param>
        /// <param name="dataLen">解密后的数据长度</param>
        /// <returns>解密后的字节数组</returns>
        private static byte[] TripleDesDecrypt(byte[] key, byte[] iv, byte[] source, out int dataLen)
        {
            using var dsp = TripleDES.Create();

            dsp.Mode = CipherMode.CBC; // 默认值
            dsp.Padding = PaddingMode.PKCS7;// 默认值

            using MemoryStream mStream = new(source);
            using CryptoStream cStream = new(mStream, dsp.CreateDecryptor(key, iv), CryptoStreamMode.Read);
            byte[] result = new byte[source.Length];
            dataLen = cStream.Read(result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// 3Des解密，密钥长度必需是24字节
        /// </summary>
        /// <param name="key">密钥字节数组</param>
        /// <param name="iv">向量字节数组</param>
        /// <param name="source">加密后的字节数组</param>
        /// <returns>解密后的字节数组</returns>
        private static byte[] TripleDesDecrypt(byte[] key, byte[] iv, byte[] source)
        {
            byte[] result = TripleDesDecrypt(key, iv, source, out int dataLen);

            if (result.Length != dataLen)
            {
                // 如果数组长度不是解密后的实际长度，需要截断多余的数据，用来解决Gzip的"Magic byte doesn't match"的问题
                byte[] resultToReturn = new byte[dataLen];
                Array.Copy(result, resultToReturn, dataLen);
                return resultToReturn;
            }
            else
            {
                return result;
            }
        }
        #endregion


        #endregion


        #region SHA1加密

        /// <summary>
        /// SHA1加密，等效于 PHP 的 SHA1() 代码
        /// </summary>
        /// <param name="source">被加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string SHA1_Encrypt(string source)
        {
            byte[] temp1 = Encoding.UTF8.GetBytes(source);
            byte[] temp2 = SHA1.HashData(temp1);

            //注意，不能用这个
            //string output = Convert.ToBase64String(temp2); 

            string output = BitConverter.ToString(temp2);
            output = output.Replace("-", "");
            output = output.ToLower();
            return output;
        }
        #endregion

        #region 通过HTTP传递的Base64编码
        /// <summary>
        /// 编码 通过HTTP传递的Base64编码
        /// </summary>
        /// <param name="source">编码前的</param>
        /// <returns>编码后的</returns>
        public static string HttpBase64Encode(string source)
        {
            //空串处理
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }

            //编码
            string encodeString = Convert.ToBase64String(Encoding.UTF8.GetBytes(source));

            //过滤
            encodeString = encodeString.Replace("+", "~");
            encodeString = encodeString.Replace("/", "@");
            encodeString = encodeString.Replace("=", "$");

            //返回
            return encodeString;
        }
        #endregion

        #region 通过HTTP传递的Base64解码
        /// <summary>
        /// 解码 通过HTTP传递的Base64解码
        /// </summary>
        /// <param name="source">解码前的</param>
        /// <returns>解码后的</returns>
        public static string HttpBase64Decode(string source)
        {
            //空串处理
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }

            //还原
            string deocdeString = source;
            deocdeString = deocdeString.Replace("~", "+");
            deocdeString = deocdeString.Replace("@", "/");
            deocdeString = deocdeString.Replace("$", "=");

            //Base64解码
            deocdeString = Encoding.UTF8.GetString(Convert.FromBase64String(deocdeString));

            //返回
            return deocdeString;
        }
        #endregion

        /// <summary>
        /// 计算文件的MD5值并返回
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            using FileStream file = new(fileName, FileMode.Open);
            using MD5 md5 = MD5.Create();
            byte[] retVal = md5.ComputeHash(file);
            return BitConverter.ToString(retVal).Replace("-", "");
        }

        /// <summary>  
        ///AES加密（加密步骤）  
        ///1，加密字符串得到2进制数组；    
        ///2，进行base64编码  
        /// </summary>  
        /// <param name="toEncrypt">要加密的字符串</param>  
        /// <param name="key">密钥</param>  
        public static string AES_Encrypt(string toEncrypt, string key)
        {
            byte[] _Key = Encoding.ASCII.GetBytes(BuildKey(key, 32));
            byte[] _Source = Encoding.UTF8.GetBytes(toEncrypt);
            using Aes aes = Aes.Create();

            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _Key;
            ICryptoTransform cTransform = aes.CreateEncryptor();

            byte[] cryptData = cTransform.TransformFinalBlock(_Source, 0, _Source.Length);
            //string HexCryptString = Hex_2To16(cryptData);
            //byte[] HexCryptData = Encoding.UTF8.GetBytes(HexCryptString);
            return Convert.ToBase64String(cryptData);
        }

        /// <summary>  
        /// AES解密（解密步骤）  
        /// 1，将BASE64字符串转为数组  
        /// 2，用AES解密数据  
        /// </summary>  
        /// <param name="encryptedSource">已加密的内容</param>  
        /// <param name="key">密钥</param>  
        public static string AES_Decrypt(string encryptedSource, string key)
        {
            byte[] _Key = Encoding.ASCII.GetBytes(BuildKey(key, 32));
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _Key;
            ICryptoTransform cTransform = aes.CreateDecryptor();


            byte[] encryptedData = Convert.FromBase64String(encryptedSource);
            //string encryptedString = Encoding.UTF8.GetString(encryptedData);
            //byte[] _Source = Hex_16To2(encryptedString);
            byte[] originalSrouceData = cTransform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(originalSrouceData);
        }

        private static string BuildKey(string key, int length = 8)
        {
            return ((key ?? string.Empty) + KEY_Complement)[..length];
        }

        //private static string Hex_2To16(byte[] bytes)
        //{
        //    string hexString = string.Empty;
        //    int iLength = 65535;
        //    if (bytes != null)
        //    {
        //        StringBuilder strB = new StringBuilder();

        //        if (bytes.Length < iLength)
        //        {
        //            iLength = bytes.Length;
        //        }

        //        for (int i = 0; i < iLength; i++)
        //        {
        //            strB.Append(bytes[i].ToString("X2"));
        //        }
        //        hexString = strB.ToString();
        //    }
        //    return hexString;
        //}

        //private static byte[] Hex_16To2(string hexString)
        //{
        //    if ((hexString.Length % 2) != 0)
        //    {
        //        hexString += " ";
        //    }
        //    byte[] returnBytes = new byte[hexString.Length / 2];
        //    for (int i = 0; i < returnBytes.Length; i++)
        //    {
        //        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        //    }
        //    return returnBytes;
        //}
    }
}
