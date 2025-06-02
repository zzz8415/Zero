using System;
using System.Globalization; // 引入 System.Globalization 以便在 TryParse 中使用，确保文化不变性

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 对象扩展方法
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 尝试将对象转换为指定类型，如果转换失败则返回默认值。
        /// 这是一个通用的辅助方法。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="source">要转换的源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的默认值。</param>
        /// <returns>转换后的值或默认值。</returns>
        private static T ConvertOrDefault<T>(this object source, T defaultValue)
        {
            // 如果源对象为 null，直接返回默认值
            if (source == null)
            {
                return defaultValue;
            }

            // 如果源对象已经是目标类型，直接返回
            if (source is T existingValue)
            {
                return existingValue;
            }

            // 尝试通过字符串解析进行转换
            // 使用 InvariantCulture 避免因区域设置不同导致的解析问题
            try
            {
                // 使用 Convert.ChangeType 来处理更广泛的类型转换
                // 如果目标类型有 TryParse 方法，也可以考虑使用反射来调用
                // 但对于基本类型，Convert.ChangeType 通常更简洁
                return (T)Convert.ChangeType(source, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                // 如果 Convert.ChangeType 失败，尝试使用具体的 TryParse（如果 T 是数值或日期类型）
                // 注意：Convert.ChangeType 已经涵盖了大部分情况，这里作为额外尝试
                if (typeof(T) == typeof(int) && int.TryParse(source.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int intResult))
                {
                    return (T)(object)intResult;
                }
                else if (typeof(T) == typeof(long) && long.TryParse(source.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out long longResult))
                {
                    return (T)(object)longResult;
                }
                else if (typeof(T) == typeof(float) && float.TryParse(source.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float floatResult))
                {
                    return (T)(object)floatResult;
                }
                else if (typeof(T) == typeof(double) && double.TryParse(source.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleResult))
                {
                    return (T)(object)doubleResult;
                }
                else if (typeof(T) == typeof(decimal) && decimal.TryParse(source.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalResult))
                {
                    return (T)(object)decimalResult;
                }
                else if (typeof(T) == typeof(byte) && byte.TryParse(source.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte byteResult))
                {
                    return (T)(object)byteResult;
                }
                else if (typeof(T) == typeof(DateTime) && DateTime.TryParse(source.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeResult))
                {
                    return (T)(object)dateTimeResult;
                }
                else if (typeof(T) == typeof(DateTimeOffset) && DateTimeOffset.TryParse(source.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffsetResult))
                {
                    return (T)(object)dateTimeOffsetResult;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 将对象转成 Int32 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static int ToInt32(this object source, int defaultValue = -1)
        {
            return source.ConvertOrDefault(defaultValue);
        }

        /// <summary>
        /// 将对象转成 Int64 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static long ToInt64(this object source, long defaultValue = -1L)
        {
            return source.ConvertOrDefault(defaultValue);
        }

        /// <summary>
        /// 将对象转成 float 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static float ToFloat(this object source, float defaultValue = -1F)
        {
            return source.ConvertOrDefault(defaultValue);
        }

        /// <summary>
        /// 将对象转成 double 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static double ToDouble(this object source, double defaultValue = -1D)
        {
            return source.ConvertOrDefault(defaultValue);
        }


        /// <summary>
        /// 将对象转成 decimal 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static decimal ToDecimal(this object source, decimal defaultValue = -1M)
        {
            return source.ConvertOrDefault(defaultValue);
        }


        /// <summary>
        /// 将对象转成 Byte 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static byte ToByte(this object source, byte defaultValue = default)
        {
            return source.ConvertOrDefault(defaultValue);
        }

        /// <summary>
        /// 将对象转换成 DateTime 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static DateTime ToDateTime(this object source, DateTime defaultValue = default)
        {
            return source.ConvertOrDefault(defaultValue);
        }

        /// <summary>
        /// 将对象转换成 DateTimeOffset 类型。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="defaultValue">如果转换失败，返回的数值。</param>
        public static DateTimeOffset ToDateTimeOffset(this object source, DateTimeOffset defaultValue = default)
        {
            return source.ConvertOrDefault(defaultValue);
        }
    }
}