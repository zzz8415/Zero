using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zero.NETCore.Extensions;

namespace Zero.NETCore.Util
{
    /// <summary>
    /// 文件帮助类
    /// </summary>
    public class FileHelper
    {

        /// <summary>
        /// 验证文件名是否是图片
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsImage(string fileName)
        {
            var extName = Path.GetExtension(fileName);
            var imageExtNames = new List<string> {
                "jpeg",
                "jpg",
                "png",
                "gif",
                "bmp"
            };
            return imageExtNames.Contains(extName);
        }
    }
}
