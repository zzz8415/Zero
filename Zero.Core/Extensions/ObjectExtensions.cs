using System;
using System.Collections.Generic;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 对象扩展方法
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 将对象转成Int32类型，如果转换失败，则返回-1
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int ToInt32(this object source)
        {
            return source.ToInt32(-1);
        }

        /// <summary>
        /// 将对象转成Int32类型
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue">如果转换失败，返回的数值</param>
        /// <returns></returns>
        public static int ToInt32(this object source, Int32 defaultValue)
        {
            if (source != null)
            {
                if (source is Int32)
                    return (Int32)source;

                Int32 result;
                if (Int32.TryParse(source.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 将对象转成Int64类型，如果转换失败，则返回-1
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static long ToInt64(this object source)
        {
            return source.ToInt64(-1);
        }
        /// <summary>
        /// 将对象转成Int64类型
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue">如果转换失败，返回的数值</param>
        /// <returns></returns>
        public static long ToInt64(this object source, Int64 defaultValue)
        {
            if (null != source)
            {
                if (source is Int64)
                    return (Int64)source;


                Int64 result;
                if (Int64.TryParse(source.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 将对象转成double类型，如果转换失败，则返回-1
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double ToDouble(this object source)
        {
            return source.ToDouble(-1d);
        }

        /// <summary>
        /// 将对象转成float类型，如果转换失败，则返回-1
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float ToFloat(this object source)
        {
            return source.ToFloat(-1f);
        }

        /// <summary>
        /// 将对象转成float类型
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue">如果转换失败，返回的数值</param>
        /// <returns></returns>
        public static float ToFloat(this object source, float defaultValue)
        {
            if (null != source)
            {
                if (source is float)
                    return (float)source;

                float result;
                if (float.TryParse(source.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 将对象转成double类型
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue">如果转换失败，返回的数值</param>
        /// <returns></returns>
        public static double ToDouble(this object source, Double defaultValue)
        {
            if (null != source)
            {
                if (source is Double)
                    return (Double)source;

                Double result;
                if (Double.TryParse(source.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 将对象转成Byte类型
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue">如果转换失败，返回的数值</param>
        /// <returns></returns>
        public static byte ToByte(this object source, Byte defaultValue)
        {
            if (source != null)
            {
                if (source is Byte)
                    return (Byte)source;

                Byte result;
                if (Byte.TryParse(source.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }
        /// <summary>
        /// 将对象转换成DateTime类型
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object source, DateTime defaultValue)
        {
            if (source != null)
            {
                if (source is DateTime)
                    return (DateTime)source;

                DateTime result;
                if (DateTime.TryParse(source.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        ///  将对象转换成DateTime类型
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object source)
        {
            return source.ToDateTime(DateTime.Now);
        }

        /// <summary>
        /// 将对象转成Byte类型
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte ToByte(this object source)
        {
            return source.ToByte(default);
        }

        /// <summary>
        /// 将对象转成Decimal类型，如果转换失败，则返回-1
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this object source)
        {
            return source.ToDecimal(-1M);
        }

        /// <summary>
        /// 将对象转成Decimal类型
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue">如果转换失败，返回的数值</param>
        /// <returns></returns>
        public static decimal ToDecimal(this object source, Decimal defaultValue)
        {
            if (source != null)
            {
                if (source is Decimal v)
                {
                    return v;
                }

                if (Decimal.TryParse(source.ToString(), out Decimal result))
                {
                    return result;
                }
            }
            return defaultValue;
        }
    }
}
