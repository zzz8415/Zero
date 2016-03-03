using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Security.Cryptography;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 其它扩展方法
    /// </summary>
    public static class OtherExtensions
    {
        /// <summary>
        /// 根据指定委托从数组中查找位置,未找到返回-1
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="array">数组对象</param>
        /// <param name="method">定义一组条件并确定指定对象是否符合这些条件的方法</param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> array, Predicate<T> method)
        {
            return IndexOf(array, method, 0, array.Count);
        }

        /// <summary>
        /// 根据指定委托从数组中查找位置,未找到返回-1
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="array">数组对象</param>
        /// <param name="method">定义一组条件并确定指定对象是否符合这些条件的方法</param>
        /// <param name="startIndex">从零开始的搜索的起始索引</param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> array, Predicate<T> method, int startIndex)
        {
            return IndexOf(array, method, startIndex, array.Count);
        }

        /// <summary>
        /// 根据指定委托从数组中查找位置,未找到返回-1
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="array">数组对象</param>
        /// <param name="method">定义一组条件并确定指定对象是否符合这些条件的方法</param>
        /// <param name="startIndex">从零开始的搜索的起始索引</param>
        /// <param name="count">要搜索的部分中的元素数</param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> array, Predicate<T> method, int startIndex, int count)
        {
            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (method(array[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
