using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Zero.Core.Util
{
    /// <summary>
    /// 压缩解压缩类
    /// </summary>
    public class ZipHelper
    {
        #region 压缩方法
        /// <summary>
        /// 压缩指定的文件列表
        /// </summary>
        /// <param name="zipFilePath">要压缩到的文件路径名</param>
        /// <param name="files">要被压缩的文件列表</param>
        public static void ZipFiles(string zipFilePath, params string[] files)
        {
            if (files == null || files.Length == 0)
                throw new ArgumentException("文件列表不能为空", "files");
            if (string.IsNullOrEmpty(zipFilePath))
                zipFilePath = Path.GetFileName(files[0]) + ".zip";

            using (ZipOutputStream zos = new ZipOutputStream(File.Create(zipFilePath)))
            {
                foreach (string file in files)
                {
                    // GetDirectoryName，是用于把文件名中不需要压缩的路径替换掉，避免压缩包里出现C:这样的目录结构
                    string filename = Path.GetFullPath(file);   // 避免出现c:\\//a.txt这样的多斜杠的路径，造成压缩路径显示错误
                    AddFileEntry(zos, filename, Path.GetDirectoryName(filename));
                }
                zos.Close();
            }
        }

        /// <summary>
        /// 压缩指定的目录列表
        /// </summary>
        /// <param name="zipFilePath">要压缩到的文件路径名</param>
        /// <param name="dirs">要被压缩的目录列表</param>
        public static void ZipDirs(string zipFilePath, params string[] dirs)
        {
            if (dirs == null || dirs.Length == 0)
                throw new ArgumentException("目录列表不能为空", "dirs");
            if (string.IsNullOrEmpty(zipFilePath))
                zipFilePath = Path.GetFileName(dirs[0]) + ".zip";

            using (ZipOutputStream zos = new ZipOutputStream(File.Create(zipFilePath)))
            {
                foreach (string dir in dirs)
                {
                    string dirname = Path.GetFullPath(dir);   // 避免出现c:\\//abc这样的多斜杠的路径，造成压缩路径显示错误
                    AddDirEntry(zos, dirname, Path.GetDirectoryName(dirname));
                }
            }
        }

        /// <summary>
        /// 往已存在的压缩文件里添加指定的文件列表，压缩文件不存在时，新建
        /// </summary>
        /// <param name="zipFilePath">要压缩到的文件路径名</param>
        /// <param name="files">要被压缩的文件列表</param>
        public static void AddFiles(string zipFilePath, params string[] files)
        {
            if (string.IsNullOrEmpty(zipFilePath))
                zipFilePath = Path.GetFileName(files[0]) + ".zip";

            if (!File.Exists(zipFilePath))
            {
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
                {
                    s.Finish();
                    s.Close();
                }
            }
            using (ZipFile zip = new ZipFile(zipFilePath))
            {
                zip.BeginUpdate();
                //zip.IsStreamOwner = false;
                foreach (string file in files)
                {
                    zip.Add(file);
                }
                zip.CommitUpdate();
                zip.Close();
            }
        }

        /// <summary>
        /// 把目录加入压缩包,返回压缩的目录和文件数
        /// </summary>
        /// <param name="zos"></param>
        /// <param name="dir"></param>
        /// <param name="rootDir">用于把文件名中不需要压缩的路径替换掉，避免压缩包里出现C:这样的目录结构</param>
        private static int AddDirEntry(ZipOutputStream zos, string dir, string rootDir)
        {
            string[] dirs = Directory.GetDirectories(dir);
            string[] files = Directory.GetFiles(dir);
            int ret = files.Length + dirs.Length;
            foreach (string subdir in dirs)
            {
                int tmp = AddDirEntry(zos, subdir, rootDir);
                if (tmp == 0)
                {
                    string strEntryName = subdir.Replace(rootDir, "");
                    ZipEntry entry = new ZipEntry(strEntryName + "\\_");
                    zos.PutNextEntry(entry);
                }
                ret += tmp;
            }
            foreach (string file in files)
            {
                AddFileEntry(zos, file, rootDir);
            }
            return ret;
        }

        /// <summary>
        /// 把文件加入压缩包
        /// </summary>
        /// <param name="zos"></param>
        /// <param name="file"></param>
        /// <param name="rootDir">用于把文件名中不需要压缩的路径替换掉，避免压缩包里出现C:这样的目录结构</param>
        private static void AddFileEntry(ZipOutputStream zos, string file, string rootDir)
        {
            //rootDir = Regex.Replace(rootDir, @"[/\\]+", @"\");// 把多个斜杠替换为一个
            //file = Regex.Replace(file, @"[/\\]+", @"\");// 把多个斜杠替换为一个
            if (!rootDir.EndsWith(@"\"))
                rootDir += @"\";
            using (FileStream fs = File.OpenRead(file))
            {
                string strEntryName = file.Replace(rootDir, "");
                ZipEntry entry = new ZipEntry(strEntryName);
                zos.PutNextEntry(entry);
                int size = 1024;
                byte[] array = new byte[size];
                while (fs.Position < fs.Length)
                {
                    int length = fs.Read(array, 0, size);
                    zos.Write(array, 0, length);
                }
                fs.Close();
            }
        }

/*
        /// <summary>
        /// 分段压缩方法，避免压缩超过150M的文件时，机器响应慢，暂时未用
        /// </summary>
        /// <param name="path"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private static string createzipfile(string path, int m)
        {
            try
            {
                Crc32 crc = new Crc32();
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (FileStream fszip = new FileStream(path + ".zip", FileMode.Create))
                {
                    ZipOutputStream zipout = new ZipOutputStream(fszip);
                    long pai = 1024 * 1024 * m; //每m兆写一次 
                    long forint = fs.Length / pai + 1;
                    byte[] buffer;
                    var entry = new ZipEntry(Path.GetFileName(path));
                    entry.Size = fs.Length;
                    entry.DateTime = DateTime.Now;
                    zipout.PutNextEntry(entry);
                    for (long i = 1; i <= forint; i++)
                    {
                        if (pai * i < fs.Length)
                        {
                            buffer = new byte[pai];
                            fs.Seek(pai * (i - 1), SeekOrigin.Begin);
                        }
                        else
                        {
                            if (fs.Length < pai)
                            {
                                buffer = new byte[fs.Length];
                            }
                            else
                            {
                                buffer = new byte[fs.Length - pai * (i - 1)];
                                fs.Seek(pai * (i - 1), SeekOrigin.Begin);
                            }
                        }
                        fs.Read(buffer, 0, buffer.Length);
                        crc.Reset();
                        crc.Update(buffer);
                        zipout.Write(buffer, 0, buffer.Length);
                        zipout.Flush();
                    }
                    fs.Close();
                    zipout.Finish();
                    zipout.Close();
                    //File.Delete(path);
                    return path + ".zip";
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                return str;
            }
        }
*/
        #endregion


        #region 解压方法
        /// <summary>
        /// 把指定压缩包解压到指定文件夹，并返回解压文件数，文件夹为空时，解压到压缩包所在目录
        /// </summary>
        /// <param name="zipfilename"></param>
        /// <param name="unzipDir"></param>
        public static int UnZipFile(string zipfilename, string unzipDir = null)
        {
            int filecount = 0;
            if (string.IsNullOrEmpty(unzipDir))
            {
                unzipDir = Path.GetDirectoryName(zipfilename);
                if (unzipDir == null)
                    throw new ArgumentException("目录信息不存在", "zipfilename");
            }
            else if (!Directory.Exists(unzipDir))
            {
                //生成解压目录
                Directory.CreateDirectory(unzipDir);
            }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipfilename)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string path = Path.Combine(unzipDir, theEntry.Name);
                    if (theEntry.IsDirectory)
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                    }
                    else if (theEntry.IsFile)
                    {
                        filecount++;
                        string dir = Path.GetDirectoryName(path);
                        if(string.IsNullOrEmpty(dir))
                            throw new Exception("压缩文件有问题，有个文件没有目录" + path);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        //解压文件到指定的目录)
                        using (FileStream fs = File.Create(path))
                        {
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                int size = s.Read(data, 0, data.Length);
                                if (size <= 0)
                                    break;
                                fs.Write(data, 0, size);
                            }
                            fs.Close();
                        }
                    }
                }
                s.Close();
            }
            return filecount;
        }
        #endregion
    }
}