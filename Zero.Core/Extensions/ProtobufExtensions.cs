using System;
using System.IO;
using ProtoBuf;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProtoBufExtensions
    {
        /// <summary>
        /// 使用protobuf序列化对象为二进制文件
        /// </summary>
        /// <typeparam name="T">需要序列化的对象类型，必须声明[ProtoContract]特征，且相应属性必须声明[ProtoMember(序号)]特征</typeparam>
        /// <param name="obj">需要序列化的对象</param>
        /// <param name="serializeFilePath">序列化后的物理文件路径</param>
        public static void ProtobufSerialize<T>(T obj, string serializeFilePath)
        {
            using (var fs = new FileStream(serializeFilePath, FileMode.Create))
            {
                Serializer.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// 使用protobuf反序列化二进制文件为对象
        /// </summary>
        /// <typeparam name="T">需要反序列化的对象类型，必须声明[ProtoContract]特征，且相应属性必须声明[ProtoMember(序号)]特征</typeparam>
        /// <param name="serializeFilePath">反序列化对象的物理文件路径</param>
        public static T ProtobufDeserialize<T>(string serializeFilePath) where T : class
        {
            using (var fs = new FileStream(serializeFilePath, FileMode.Open))
            {
                return Serializer.Deserialize<T>(fs);
            }
        }

        /// <summary>
        /// 使用protobuf把对象序列化为Byte数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Byte[] ToProtoBuf(this object obj)
        {
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, obj);
                return memory.ToArray();
            }
        }

        /// <summary>
        /// 使用protobuf反序列化二进制数组为对象
        /// </summary>
        /// <typeparam name="T">需要反序列化的对象类型，必须声明[ProtoContract]特征，且相应属性必须声明[ProtoMember(序号)]特征</typeparam>
        /// <param name="data"></param>
        public static T DeserializeProtoBuf<T>(this Byte[] data) where T : class
        {
            using (var memory = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(memory);
            }
        }

    }
}
