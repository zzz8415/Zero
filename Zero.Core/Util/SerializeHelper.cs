using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Zero.NETCore.Util
{
    /// <summary>
    /// 序列化辅助类
    /// </summary>
    public class SerializeHelper
    {
        /// <summary>
        /// 序列化对象为二进制文件
        /// </summary>
        /// <typeparam name="T">需要序列化的对象类型，必须声明[Serializable]特征，且必须是public类</typeparam>
        /// <param name="obj">需要序列化的对象</param>
        /// <param name="serializeFilePath">序列化后的物理文件路径</param>
        public static void BinarySerialize<T>(T obj, string serializeFilePath)
        {
            using (var fs = new FileStream(serializeFilePath, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
            }
        }
        /// <summary>
        /// 反序列化二进制文件为对象
        /// </summary>
        /// <typeparam name="T">需要反序列化的对象类型，必须声明[Serializable]特征</typeparam>
        /// <param name="serializeFilePath">反序列化对象的物理文件路径</param>
        public static T BinaryDeserialize<T>(string serializeFilePath) where T : class
        {
            using (var fs = new FileStream(serializeFilePath, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(fs) as T;
            }
        }
    }
}
