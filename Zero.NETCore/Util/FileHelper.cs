using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore.Util
{
    public class FileHelper
    {
        /// <summary>
        /// 验证文件名是否是图片
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetExtName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "";
            }
            var extName = "";
            if (fileName.LastIndexOf(".") > 0)
            {
                extName = fileName.Substring(fileName.LastIndexOf(".")).ToLower();
            }
            return extName;
        }


        /// <summary>
        /// 验证文件名是否是图片
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsImage(string fileName)
        {
            var extName = GetExtName(fileName);
            if (extName != ".jpeg" && extName != ".jpg" && extName != ".png" && extName != ".gif" && extName != ".bmp")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
