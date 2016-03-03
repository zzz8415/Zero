using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI;
using System.IO;
using System.Web;

namespace Zero.Core.Web
{
    /// <summary>
    /// 
    /// </summary>
    public static class WebHelper
    {
        /// <summary>
        /// 获得技术部IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetRealIP()
        {
            string ip = HttpContext.Current.Request.UserHostAddress;//"0.0.0.0"; 
            try
            {
                if (ip != null && ip.StartsWith("10."))
                    ip = HttpContext.Current.Request.ServerVariables["HTTP_X_REAL_IP"].ToString().Split(',')[0].Trim();
            }
            catch
            {
                return ip;
            }
            return ip;
        }
        /// <summary>
        /// 获得技术部IP地址
        /// </summary>
        /// <returns></returns>
        public static long GetIP()
        {
            string ip = GetRealIP();
            if (string.IsNullOrEmpty(ip))
                return 0;
            return ConvertIPToLong(ip);
        }

        /// <summary>
        /// 返回UserHostAddress;HTTP_X_REAL_IP;HTTP_NDUSER_FORWARDED_FOR_HAPROXY
        /// </summary>
        /// <returns></returns>
        public static string GetIpList()
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return string.Empty;
            var request = HttpContext.Current.Request;
            string realip = request.ServerVariables == null
                    ? string.Empty
                    : request.ServerVariables["HTTP_X_REAL_IP"];
            string forwardip = request.ServerVariables == null
                    ? string.Empty
                    : request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string proxy = request.Headers == null
                                ? string.Empty
                                : request.Headers.Get("HTTP_NDUSER_FORWARDED_FOR_HAPROXY");
            return string.Format("{0};{1};{2};{3}", request.UserHostAddress, realip, forwardip, proxy);

        }

        /// <summary>
        /// 获取本机所有IPV4地址列表
        /// </summary>
        /// <returns>本机所有IPV4地址列表，以分号分隔</returns>
        public static string GetServerIpList()
        {
            try
            {
                StringBuilder ips = new StringBuilder();
                IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ipa in IpEntry.AddressList)
                {
                    if (ipa.AddressFamily == AddressFamily.InterNetwork)
                        ips.AppendFormat("{0};", ipa.ToString());
                }
                return ips.ToString();
            }
            catch (Exception)
            {
                //LogHelper.WriteCustom("获取本地ip错误" + ex, @"zIP\", false);
                return string.Empty;
            }
        }


        /// <summary>
        /// 获取url参数的整型值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int Int32ParseFromQueryString(string key)
        {
            try
            {
                return Convert.ToInt32(HttpContext.Current.Request[key]);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取url参数的长整型值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long Int64ParseFromQueryString(string key)
        {
            try
            {
                return Convert.ToInt64(HttpContext.Current.Request[key]);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取url参数的Decimal类型值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static decimal DecimalParseFromQueryString(string key)
        {
            try
            {
                return Convert.ToDecimal(HttpContext.Current.Request[key]);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取url参数的日期类型值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime DateParseFromQueryString(string key, string defaultValue)
        {
            try
            {
                return Convert.ToDateTime(HttpContext.Current.Request[key]);
            }
            catch
            {
                return DateTime.Parse(defaultValue);
            }
        }

        /// <summary>
        /// 获取url参数的日期类型值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static DateTime DateParseFromQueryString(string key)
        {
            try
            {
                return Convert.ToDateTime(HttpContext.Current.Request[key]);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 将IP地址转成长整型
        /// </summary>
        /// <param name="ip">待转换的IP地址</param>
        /// <returns></returns>
        public static long ConvertIPToLong(string ip)
        {
            try
            {
                string[] ipList = ip.Split(new char[] { '.' });
                string xIP = string.Empty;
                foreach (string ipStr in ipList)
                {
                    xIP += Convert.ToByte(ipStr).ToString("X").PadLeft(2, '0');
                }
                long ipResult = long.Parse(xIP, System.Globalization.NumberStyles.HexNumber);
                return ipResult;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 将长整型转成IP地址
        /// </summary>
        /// <param name="ipLong">长整型</param>
        /// <returns></returns>
        public static string ConvertLongToIP(long ipLong)
        {
            StringBuilder b = new StringBuilder();
            long tempLong, temp;

            tempLong = ipLong;
            temp = tempLong / (256 * 256 * 256);
            tempLong = tempLong - (temp * 256 * 256 * 256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong / (256 * 256);
            tempLong = tempLong - (temp * 256 * 256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong / 256;
            tempLong = tempLong - (temp * 256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong;
            tempLong = tempLong - temp;
            b.Append(Convert.ToString(temp));

            return b.ToString().ToLower();
        }

        /// <summary>
        /// 过滤sql语句
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string FilterSql(string source)
        {
            source = source.Replace("'", "''");
            source = source.Replace(";", "；");
            source = source.Replace("(", "（");
            source = source.Replace(")", "）");
            source = source.Replace("Exec", "");
            source = source.Replace("Execute", "");
            source = source.Replace("xp_", "x p_");
            source = source.Replace("sp_", "s p_");
            source = source.Replace("0x", "0 x");
            return source;
        }

        /// <summary>
        /// 过滤html中的不安全代码
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string WipeScript(string html)
        {
            System.Text.RegularExpressions.Regex regex1 = new System.Text.RegularExpressions.Regex(@"<script[\s\S]+[^>]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex2 = new System.Text.RegularExpressions.Regex(@" href *= *[\s\S]*script *:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex3 = new System.Text.RegularExpressions.Regex(@"\s*on\w+\s*=\s*[^ ]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex4 = new System.Text.RegularExpressions.Regex(@"<iframe[\s\S]+[^>]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex5 = new System.Text.RegularExpressions.Regex(@"<frameset[\s\S]+</frameset *>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            html = regex1.Replace(html, ""); //过滤<script></script>标记 
            html = regex2.Replace(html, ""); //过滤href=javascript: (<A>) 属性 
            html = regex3.Replace(html, " _disibledevent="); //过滤其它控件的on...事件 
            html = regex4.Replace(html, ""); //过滤iframe 
            html = regex5.Replace(html, ""); //过滤frameset 
            return html;
        }

        /// <summary>
        /// 去除HTML标记
        /// </summary>
        /// <param   name="Htmlstring">包括HTML的源码   </param>
        /// <returns>已经去除后的文字</returns>
        public static string NoHTML(string Htmlstring)
        {
            //删除脚本
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "",
              RegexOptions.IgnoreCase);
            //删除HTML
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "",
              RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            Htmlstring = HttpUtility.HtmlEncode(Htmlstring).Trim();

            return Htmlstring;
        }

        #region 判断IP地址是否在内网IP地址所在范围

        /// <summary>
        /// 判断IP地址是否为内网IP地址
        /// </summary>
        /// <param name="ipAddress">IP地址字符串</param>
        /// <returns></returns>
        public static bool IsInnerIP(String ipAddress)
        {
            bool isInnerIp = false;
            long ipNum = GetIpNum(ipAddress);
            /*
               私有IP：A类  10.0.0.0-10.255.255.255
                       B类  172.16.0.0-172.31.255.255
                       C类  192.168.0.0-192.168.255.255
                       当然，还有127这个网段是环回地址   
              */
            long aBegin = GetIpNum("10.0.0.0");
            long aEnd = GetIpNum("10.255.255.255");
            long bBegin = GetIpNum("172.16.0.0");
            long bEnd = GetIpNum("172.31.255.255");
            long cBegin = GetIpNum("192.168.0.0");
            long cEnd = GetIpNum("192.168.255.255");
            isInnerIp = IsInner(ipNum, aBegin, aEnd) || IsInner(ipNum, bBegin, bEnd) || IsInner(ipNum, cBegin, cEnd) || ipAddress.Equals("127.0.0.1");
            return isInnerIp;
        }

        /// <summary>
        /// 判断用户IP地址转换为Long型后是否在内网IP地址所在范围
        /// </summary>
        /// <param name="userIp"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static bool IsInner(long userIp, long begin, long end)
        {
            return (userIp >= begin) && (userIp <= end);
        }

        /// <summary>
        /// 把IP地址转换为Long型数字
        /// </summary>
        /// <param name="ipAddress">IP地址字符串</param>
        /// <returns></returns>
        public static long GetIpNum(String ipAddress)
        {
            String[] ip = ipAddress.Split('.');
            long a = int.Parse(ip[0]);
            long b = int.Parse(ip[1]);
            long c = int.Parse(ip[2]);
            long d = int.Parse(ip[3]);

            long ipNum = a * 256 * 256 * 256 + b * 256 * 256 + c * 256 + d;
            return ipNum;
        }
        #endregion

        /// <summary>
        /// 返回 Web服务器控件的HTML 输出
        /// </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        public static string GetHtml(Control ctl)
        {
            if (ctl == null)
                return string.Empty;

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                ctl.RenderControl(htw);
                return sw.ToString();
            }
        }

        /// <summary>
        /// 获取当前访问的页面的完整URL，如http://sj.91.com/dir/a.aspx
        /// </summary>
        /// <param name="getQueryString"></param>
        /// <returns></returns>
        public static string GetUrl(bool getQueryString)
        {
            string url = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];

            if (HttpContext.Current.Request.ServerVariables["SERVER_PORT"] != "80")
                url += ":" + HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
            //strTemp = strTemp & CheckStr(HttpContext.Current.Request.ServerVariables("URL")) 

            url += HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"];

            if (getQueryString)
            {
                if (HttpContext.Current.Request.QueryString.ToString() != "")
                {
                    url += "?" + HttpContext.Current.Request.QueryString;
                }
            }

            string https = HttpContext.Current.Request.ServerVariables["HTTPS"];
            if (string.IsNullOrEmpty(https) || https == "off")
            {
                url = "http://" + url;
            }
            else
            {
                url = "https://" + url;
            }
            return url;
        }

        
        #region WebRequest方法相关
        private static CookieContainer _cookie = new CookieContainer();

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="savePath"></param>
        /// <param name="refererUrl"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="proxy"></param>
        /// <param name="userAgent"></param>
        /// <param name="requestTimeOut">WebRequest的最大请求时间，单位：毫秒，0为不设置</param>
        /// <param name="allowRedirect"></param>
        /// <param name="downsize">分块下载每次下载字节数，小于等于0时表示不分块下载</param>
        public static void DownloadFile(string downloadUrl, string savePath,
            string refererUrl = null, string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, int downsize = 0)
        {
            DownloadFile(downloadUrl, savePath, ref _cookie, refererUrl, userName, password, proxy, userAgent,
                         requestTimeOut, allowRedirect, downsize);
        }


        /// <summary>
        /// 文件下载(注：如果savePath已经存在，会被先删除)
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="savePath"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="proxy"></param>
        /// <param name="userAgent"></param>
        /// <param name="requestTimeOut">WebRequest的最大请求时间，单位：毫秒，0为不设置</param>
        /// <param name="allowRedirect"></param>
        /// <param name="downsize">分块下载时，每次下载字节数，小于等于0时表示不分块下载</param>
        public static void DownloadFile(string downloadUrl, string savePath, ref CookieContainer cookieContainer,
            string refererUrl = null, string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, int downsize = 0)
        {
            // 先删除源文件，避免下载追加到源文件后面
            if (File.Exists(savePath))
                File.Delete(savePath);

            var begin = 0L;
            var total = -1L;
            var complete = false;
            while (!complete)
            {
                HttpWebRequest request = CreateRequest(downloadUrl, ref cookieContainer, refererUrl, userName, password, proxy,
                                        userAgent, requestTimeOut, allowRedirect);

                // 分块下载
                if (downsize > 0)
                    request.AddRange((int)begin, (int)begin + downsize - 1);

                FileStream stream;
                if (File.Exists(savePath))
                {
                    stream = File.OpenWrite(savePath);
                    stream.Seek(stream.Length, SeekOrigin.Current); //移动文件流中的当前指针 
                }
                else
                {
                    stream = new FileStream(savePath, FileMode.Create);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                {
                    if (responseStream == null)
                        return;
                    if (total == -1)
                    {
                        var range = response.Headers["Content-Range"] != null
                                        ? response.Headers["Content-Range"].Split('/')[1]
                                        : string.Empty;
                        if (string.IsNullOrEmpty(range) || !long.TryParse(range, out total))
                        {
                            total = response.ContentLength;
                        }
                    }

                    #region 保存到文件

                    //long s = stream.Length, l = response.ContentLength;
                    int read;
                    if (downsize <= 0)
                        downsize = (int)total;
                    byte[] buffer = new byte[downsize];
                    while ((read = responseStream.Read(buffer, 0, buffer.Length)) != 0) //&& s < l
                    {
                        stream.Write(buffer, 0, read);
                        stream.Flush();
                        //s += read;
                        //var percent = (s*100/(decimal) l).ToString("N");
                    }
                    //responseStream.Close();
                    stream.Close();

                    #endregion
                }
                begin += downsize;
                if (begin >= total)
                    complete = true;
            }
        }

        
        #region 文件分发相关方法
        /// <summary>
        /// 上传完成时的委托
        /// </summary>
        /// <param name="UploadFilePath">上传文件保存路径</param>
        public delegate void UploadSuccessHandle(string UploadFilePath);

        /// <summary>
        /// 客户端调用的文件上传方法，需配合UploadFileServer使用(如Aspx页面用UploadFileServer方法接收)
        /// </summary>
        /// <param name="filePath">需要上传的本地文件路径</param>
        /// <param name="uploadUrl">服务端接收上传的url路径</param>
        /// <param name="savePath">服务端的保存路径（含文件名），为空时用filePath</param>
        /// <param name="uploadsize">分块上传每次上传字节数，小于等于0时表示不分块上传</param>
        /// <param name="refererUrl"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="proxy"></param>
        /// <param name="userAgent"></param>
        /// <param name="requestTimeOut"></param>
        /// <param name="needZip">上传过程中，是否要进行zip</param>
        /// <returns></returns>
        public static string UploadFileClient(string filePath, string uploadUrl, string savePath = null,
            int uploadsize = 0, string refererUrl = null, string userName = null, string password = null,
            string proxy = null, string userAgent = null, int requestTimeOut = 0, bool needZip = false)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return "指定的文件不存在:" + filePath;

            // 传递给服务端保存路径文件和文件名
            if (string.IsNullOrEmpty(savePath))
                savePath = filePath;
            savePath = HttpUtility.UrlEncode(savePath);

            long ziplength = 0; // 记录压缩后的大小
            string oldfilename = string.Empty;      // 用于服务端解压后，得到解压的文件名
            const string zipSuffix = ".zipupload";
            if (needZip)
            {
                // zip压缩
                string zipfilepath = filePath + zipSuffix;
                Zero.Core.Util.ZipHelper.ZipFiles(zipfilepath, filePath);
                ziplength = new FileInfo(zipfilepath).Length;

                oldfilename = filePath;
                filePath = zipfilepath;
            }

            bool isfirstUpload = true;
            using (var filestream = File.OpenRead(filePath))
            {
                long filesize = filestream.Length;
                if (filesize <= 0)
                {
                    return "不能上传0字节的空文件";
                }
                // 是否分块上传
                bool isMultiUpload = uploadsize > 0;
                if (!isMultiUpload)
                    uploadsize = (int)filesize;

                byte[] data = new byte[uploadsize];
                int readcnt;
                while ((readcnt = filestream.Read(data, 0, uploadsize)) > 0)
                {
                    string realurl = uploadUrl;
                    if (isfirstUpload)
                    {
                        // 第一次上传，需要服务端删除已存在的文件，防止分块上传时，直接拼接到错误的文件上
                        if (realurl.IndexOf('?') < 0)
                            realurl += "?isfirstupload=1";
                        else
                            realurl += "&isfirstupload=1";
                        isfirstUpload = false;
                    }
                    realurl += "&" + savePath;
                    HttpWebRequest request = CreateRequest(realurl, ref _cookie, refererUrl,
                        userName, password, proxy, userAgent, requestTimeOut, false, false);

                    request.Method = "POST";

                    //request.ContentType = fileType;

                    request.Headers.Add("SaveFileName", savePath);
                    request.Headers.Add("MultiUpload", isMultiUpload ? "1" : "0");
                    if (needZip)
                    {
                        request.Headers.Add("ZipName", oldfilename);
                    }

                    // 必须先设置ContentLength，才能打开GetRequestStream
                    // ContentLength设置后，reqStream.Close前必须写入相同字节的数据，否则Request会被取消
                    request.ContentLength = readcnt;
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        // 上传数据
                        reqStream.Write(data, 0, readcnt);
                        reqStream.Close();
                    }
                    string html = string.Empty;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using(Stream stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                            {
                               html = reader.ReadToEnd();
                            }
                        }
                    }
                    if (html != "ok")
                    {
                        return html;
                    }
                }//end while

                // 不能删除，可能还要上传到别的地方
                //if (needZip && filePath.EndsWith(zipSuffix, StringComparison.Ordinal))
                //    File.Delete(filePath);

                #region 如果是分块上传，上传完成后，要再调用一次，通知服务端上传完成，需要把临时文件改名为正式文件);
                if(isMultiUpload)
                {
                    string lastUrl = uploadUrl;
                    if (lastUrl.IndexOf('?') < 0)
                        lastUrl += "?islastupload=1&" + savePath;
                    else
                        lastUrl += "&islastupload=1&" + savePath;
                    HttpWebRequest request = CreateRequest(lastUrl, ref _cookie, refererUrl,
                        userName, password, proxy, userAgent, requestTimeOut, false, false);
                    request.Headers.Add("SaveFileName", savePath);
                    if (needZip)
                    {
                        request.Headers.Add("ZipName", oldfilename);
                    }

                    string html = string.Empty;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                html = reader.ReadToEnd();
                            }
                        }
                    }
                    if (html != "ok")
                        return html;
                }
                #endregion
            }
            if (needZip)
                return "ok,上传大小:" + ziplength.ToString("N");
            return "ok";
        }

        /// <summary>
        /// 客户端调用的方法，不上传文件，仅为了让服务端执行委托方法，需配合UploadFileServer使用(如Aspx页面用UploadFileServer方法接收)
        /// </summary>
        /// <param name="filePath">需要上传的本地文件路径</param>
        /// <param name="uploadUrl">服务端接收上传的url路径</param>
        /// <param name="savePath">服务端的保存路径（含文件名），为空时用filePath</param>
        /// <param name="refererUrl"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="proxy"></param>
        /// <param name="userAgent"></param>
        /// <param name="requestTimeOut"></param>
        /// <returns></returns>
        public static string UploadOnlyRefreshClient(string filePath, string uploadUrl, string savePath = null,
            string refererUrl = null, string userName = null, string password = null,
            string proxy = null, string userAgent = null, int requestTimeOut = 0)
        {
            // 传递给服务端保存路径文件和文件名
            if (string.IsNullOrEmpty(savePath))
                savePath = filePath;
            savePath = HttpUtility.UrlEncode(savePath);

            string lastUrl = uploadUrl;
            if (lastUrl.IndexOf('?') < 0)
                lastUrl += "?islastupload=1&" + savePath;
            else
                lastUrl += "&islastupload=1&" + savePath;
            HttpWebRequest request = CreateRequest(lastUrl, ref _cookie, refererUrl,
                userName, password, proxy, userAgent, requestTimeOut, false, false);
            request.Headers.Add("SaveFileName", savePath);

            string html = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        html = reader.ReadToEnd();
                    }
                }
            }
            if (html != "ok")
                return html;
            return "ok";
        }

        /// <summary>
        /// 服务端调用的文件上传方法，需配合UploadFileClient使用(Aspx页面用UploadFileServer方法接收)
        /// </summary>
        /// <param name="method">上传完成时要调用的委托,会把保存文件路径作为参数传入</param>
        public static void UploadFileServer(UploadSuccessHandle method)
        {
            var Request = HttpContext.Current.Request;
            var Response = HttpContext.Current.Response;
            Response.Clear();
            FileStream stream = null;
            try
            {
                // zip压缩上传时，压缩包里的文件名，未压缩时，为空
                string zipName = Request.Headers["ZipName"];

                // 是否分块上传
                string tmp = Request.Headers["MultiUpload"];
                bool isMultiUpload = string.IsNullOrEmpty(tmp) || tmp == "1";

                string serverFileName = HttpUtility.UrlDecode(Request.Headers["SaveFileName"]);
                string fileTmp = serverFileName + ".tmp";

                // 第一次上传时，要删除临时文件
                string uploadfirst = Request.QueryString["isfirstupload"];
                if (!string.IsNullOrEmpty(uploadfirst) && uploadfirst == "1" && File.Exists(fileTmp))
                {
                    File.Delete(fileTmp);
                }

                // 最后一次上传时，要把临时文件改名为正式文件
                string uploadlast = Request.QueryString["islastupload"];
                if (!string.IsNullOrEmpty(uploadlast) && uploadlast == "1")
                {
                    if (File.Exists(fileTmp))
                    {
                        //Zero.Core.Util.LogHelper.WriteCustom(zipName + " " + fileTmp + " " + serverFileName, null, null);
                        // 解压
                        if (!string.IsNullOrEmpty(zipName))
                        {
                            Util.ZipHelper.UnZipFile(fileTmp);
                            if (!zipName.Equals(fileTmp, StringComparison.OrdinalIgnoreCase))
                                File.Delete(fileTmp);// 删除压缩文件
                            MoveFile(zipName, serverFileName);
                        }
                        else
                            MoveFile(fileTmp, serverFileName);
                    }
                    if (method != null)
                    {
                        method(serverFileName);
                    }
                    Response.Write("ok");
                    Response.End();
                    return;
                }

                byte[] data = Request.BinaryRead(Request.ContentLength);

                if (File.Exists(fileTmp))
                {
                    stream = File.OpenWrite(fileTmp);
                    stream.Seek(stream.Length, SeekOrigin.Current); //移动文件流中的当前指针 
                }
                else
                {
                    string dir = Path.GetDirectoryName(fileTmp);
                    if (string.IsNullOrEmpty(dir))
                    {
                        Response.Write("Directory is not sended!");
                        Response.End();
                        return;
                    }

                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    stream = new FileStream(fileTmp, FileMode.Create);
                }
                stream.Write(data, 0, data.Length);
                stream.Flush();
                stream.Close();

                if (!isMultiUpload)
                {
                    if (File.Exists(fileTmp))
                    {
                        // 解压
                        if (!string.IsNullOrEmpty(zipName))
                        {
                            Util.ZipHelper.UnZipFile(fileTmp);
                            if (!zipName.Equals(fileTmp, StringComparison.OrdinalIgnoreCase))
                                File.Delete(fileTmp);// 删除压缩文件
                            MoveFile(zipName, serverFileName);
                        }
                        else
                            MoveFile(fileTmp, serverFileName);
                    }
                    if (method != null)
                    {
                        method(serverFileName);
                    }
                }

                Response.Write("ok");
            }
            catch (ThreadAbortException)
            {
                if(stream != null)
                    stream.Close();
            }
            catch (Exception exp)
            {
                if (stream != null)
                    stream.Close();
                Response.Write(exp.ToString());
            }
            Response.End();
        }
        #endregion


        /// <summary>
        /// 抓取页面
        /// </summary>
        /// <param name="url">要抓取的网址</param>
        /// <param name="para">参数</param>
        /// <param name="httpMethod">GET POST</param>
        /// <param name="encoding">编码格式，默认UTF-8</param>
        /// <param name="proxy">WebRequest要用到的代理</param>
        /// <returns></returns>
        public static string GetPage(string url, string para, string httpMethod, string proxy, Encoding encoding)
        {
            return GetPage(url, proxy: proxy, param: para, HttpMethod: httpMethod, encoding: encoding);
        }

        /// <summary>
        /// 抓取页面
        /// </summary>
        /// <param name="url">要抓取的网址</param>
        /// <param name="param">参数</param>
        /// <param name="HttpMethod">GET POST</param>
        /// <param name="encoding">编码格式，默认UTF-8</param>
        /// <param name="refererUrl">要设置的头信息的引用页</param>
        /// <param name="showHeader">返回内容是否包括头信息</param>
        /// <param name="userName">网页登录名</param>
        /// <param name="password">登录密码</param>
        /// <param name="proxy">WebRequest要用到的代理</param>
        /// <param name="userAgent">要设置的头信息里的用户代理</param>
        /// <param name="requestTimeOut">WebRequest的最大请求时间，单位：毫秒，0为不设置</param>
        /// <param name="allowRedirect">出现301或302之类的转向时，是否要转向</param>
        /// <param name="headers">要设置的Header键值对</param>
        /// <returns></returns>
        public static string GetPage(string url,
            string param = null, string HttpMethod = null, Encoding encoding = null, string refererUrl = null,
            bool showHeader = false, string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, Dictionary<string, string> headers = null)
        {
            bool isok;
            long processtime;
            int responseStatus;
            return GetPage(url, out isok, ref _cookie, out processtime, out responseStatus, param, HttpMethod, encoding,
                refererUrl, showHeader, userName, password, proxy, userAgent, requestTimeOut, allowRedirect, headers);
        }

        /// <summary>
        /// 抓取页面
        /// </summary>
        /// <param name="url">要抓取的网址</param>
        /// <param name="isok">返回抓取结果，成功还是失败</param>
        /// <param name="processtime">返回抓取页面耗费的时间，毫秒</param>
        /// <param name="responseStatus">响应状态码，如200、302、503；600表示web异常；700表示其它异常</param>
        /// <param name="param">参数</param>
        /// <param name="HttpMethod">GET POST</param>
        /// <param name="encoding">编码格式，默认UTF-8</param>
        /// <param name="refererUrl">要设置的头信息的引用页</param>
        /// <param name="showHeader">返回内容是否包括头信息</param>
        /// <param name="userName">网页登录名</param>
        /// <param name="password">登录密码</param>
        /// <param name="proxy">WebRequest要用到的代理</param>
        /// <param name="userAgent">要设置的头信息里的用户代理</param>
        /// <param name="requestTimeOut">WebRequest的最大请求时间，单位：毫秒，0为不设置</param>
        /// <param name="allowRedirect">出现301或302之类的转向时，是否要转向</param>
        /// <param name="headers">要设置的Header键值对</param>
        /// <returns></returns>
        public static string GetPage(string url, out bool isok, out long processtime, out int responseStatus,
            string param = null, string HttpMethod = null, Encoding encoding = null, string refererUrl = null,
            bool showHeader = false, string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, Dictionary<string, string> headers = null)
        {
            return GetPage(url, out isok, ref _cookie, out processtime, out responseStatus, param, HttpMethod, encoding,
                refererUrl, showHeader, userName, password, proxy, userAgent, requestTimeOut, allowRedirect, headers);
        }

        // 主调方法
        /// <summary>
        /// 抓取页面
        /// </summary>
        /// <param name="url">要抓取的网址</param>
        /// <param name="isok">返回抓取结果，成功还是失败</param>
        /// <param name="cookieContainer">要使用的cookie</param>
        /// <param name="processtime">返回抓取页面耗费的时间，毫秒</param>
        /// <param name="responseStatus">响应状态码，如200、302、503；600表示web异常；700表示其它异常</param>
        /// <param name="param">参数</param>
        /// <param name="HttpMethod">GET POST</param>
        /// <param name="encoding">编码格式，默认UTF-8</param>
        /// <param name="refererUrl">要设置的头信息的引用页</param>
        /// <param name="showHeader">返回内容是否包括头信息</param>
        /// <param name="userName">网页登录名</param>
        /// <param name="password">登录密码</param>
        /// <param name="proxy">WebRequest要用到的代理</param>
        /// <param name="userAgent">要设置的头信息里的用户代理</param>
        /// <param name="requestTimeOut">WebRequest的最大请求时间，单位：毫秒，0为不设置</param>
        /// <param name="allowRedirect">出现301或302之类的转向时，是否要转向</param>
        /// <param name="headers">要设置的Header键值对</param>
        /// <returns></returns>
        public static string GetPage(string url, out bool isok, ref CookieContainer cookieContainer, out long processtime, out int responseStatus,
            string param = null, string HttpMethod = null, Encoding encoding = null, string refererUrl = null,
            bool showHeader = false, string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, Dictionary<string, string> headers = null)
        {
            isok = false;
            if (encoding == null)
                encoding = Encoding.UTF8;

            bool isGet = (string.IsNullOrEmpty(HttpMethod) ||
                          HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase));

            // 删除网址后面的#号
            var idx = url.IndexOf('#');
            if (idx >= 0)
                url = url.Substring(0, idx);

            // Get方式，且有参数时，把参数拼接到Url后面
            if (isGet && !string.IsNullOrEmpty(param))
                if (url.IndexOf('?') < 0)
                    url += "?" + param;
                else
                    url += "&" + param;


            // 必须在写入Post Stream之前设置Proxy
            HttpWebRequest request = CreateRequest(url, ref cookieContainer, refererUrl, userName, password, proxy,
                                                   userAgent, requestTimeOut, allowRedirect);

            if (encoding == Encoding.UTF8)
                request.Headers.Add("Accept-Charset", "utf-8");
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }

            if (isGet)
            {
                request.Method = "GET";
                //request.ContentType = "text/html";
            }
            else
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                // 设置提交的数据
                if (!string.IsNullOrEmpty(param))
                {
                    // 把数据转换为字节数组
                    byte[] l_data = encoding.GetBytes(param);
                    // 必须先设置ContentLength，才能打开GetRequestStream
                    // ContentLength设置后，reqStream.Close前必须写入相同字节的数据，否则Request会被取消
                    request.ContentLength = l_data.Length;
                    using (Stream newStream = request.GetRequestStream())
                    {
                        newStream.Write(l_data, 0, l_data.Length);
                        newStream.Close();
                    }
                }
                else
                    request.ContentLength = 0;// POST时，必须设置ContentLength属性
            }


            HttpWebResponse response;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                sw.Stop();
                processtime = sw.ElapsedMilliseconds;
            }
            #region WebRequest异常处理
            catch (WebException webExp)
            {
                sw.Stop();
                processtime = sw.ElapsedMilliseconds;
                if (webExp.Response != null)
                {
                    using (var responseErr = (HttpWebResponse)webExp.Response)
                    {
                        responseStatus = (int)responseErr.StatusCode;
                        Stream streamFail;
                        using (streamFail = responseErr.GetResponseStream())
                        {
                            var contentEncoding = responseErr.ContentEncoding.ToLower();
                            if (contentEncoding.Contains("gzip"))
                            {
                                streamFail = new GZipStream(streamFail, CompressionMode.Decompress);
                            }
                            else if (contentEncoding.Contains("deflate"))
                            {
                                streamFail = new DeflateStream(streamFail, CompressionMode.Decompress);
                            }
                            using (var sr = new StreamReader(streamFail, encoding))
                            {
                                var html = sr.ReadToEnd();
                                if (showHeader)
                                    html = "请求头信息：\r\n" + request.Headers + "\r\n\r\n响应头信息：\r\n" + responseErr.Headers +
                                           "\r\n\r\n响应内容:\r\n" +
                                           html;
                                return html;
                            }
                        }
                    }
                }
                else
                {
                    responseStatus = 600;
                }
                return "返回错误：" + webExp;
            }
            catch (Exception exp)
            {
                sw.Stop();
                processtime = sw.ElapsedMilliseconds;
                responseStatus = 700;
                return "返回错误：" + exp;
            }
            #endregion

            responseStatus = (int)response.StatusCode;
            try
            {
                Stream stream;
                using (stream = response.GetResponseStream())
                {
                    if (stream == null)
                        return "GetResponseStream为null";

                    var contentEncoding = response.ContentEncoding.ToLower();
                    if (contentEncoding.Contains("gzip"))
                    {
                        stream = new GZipStream(stream, CompressionMode.Decompress);
                    }
                    else if (contentEncoding.Contains("deflate"))
                    {
                        stream = new DeflateStream(stream, CompressionMode.Decompress);
                    }
                    using (var sr = new StreamReader(stream, encoding))
                    {
                        var html = sr.ReadToEnd();
                        if (showHeader)
                            html = "请求头信息：\r\n" + request.Headers + "\r\n\r\n响应头信息：\r\n" + response.Headers +
                                   "\r\n\r\n响应内容:\r\n" +
                                   html;
                        isok = true;
                        return html;
                    }
                }
            }
            catch (Exception exp)
            {
                responseStatus = 700;
                return "返回错误：" + exp;
            }
        }

        // 主调方法
        /// <summary>
        /// 抓取页面
        /// </summary>
        /// <param name="url">要抓取的网址</param>
        /// <param name="isok">返回抓取结果，成功还是失败</param>
        /// <param name="cookieContainer">要使用的cookie</param>
        /// <param name="processtime">返回抓取页面耗费的时间，毫秒</param>
        /// <param name="responseStatus">响应状态码，如200、302、503；600表示web异常；700表示其它异常</param>
        /// <param name="param">参数</param>
        /// <param name="HttpMethod">GET POST</param>
        /// <param name="encoding">编码格式，默认UTF-8</param>
        /// <param name="refererUrl">要设置的头信息的引用页</param>
        /// <param name="userName">网页登录名</param>
        /// <param name="password">登录密码</param>
        /// <param name="proxy">WebRequest要用到的代理</param>
        /// <param name="userAgent">要设置的头信息里的用户代理</param>
        /// <param name="requestTimeOut">WebRequest的最大请求时间，单位：毫秒，0为不设置</param>
        /// <param name="allowRedirect">出现301或302之类的转向时，是否要转向</param>
        /// <param name="headers">要设置的Header键值对</param>
        /// <returns></returns>
        public static byte[] GetPageBinary(string url, out bool isok, ref CookieContainer cookieContainer, out long processtime, out int responseStatus,
            string param = null, string HttpMethod = null, Encoding encoding = null, string refererUrl = null,
            string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, Dictionary<string, string> headers = null)
        {
            isok = false;
            if (encoding == null)
                encoding = Encoding.UTF8;

            bool isGet = (string.IsNullOrEmpty(HttpMethod) ||
                          HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase));

            // 删除网址后面的#号
            var idx = url.IndexOf('#');
            if (idx >= 0)
                url = url.Substring(0, idx);

            // Get方式，且有参数时，把参数拼接到Url后面
            if (isGet && !string.IsNullOrEmpty(param))
                if (url.IndexOf('?') < 0)
                    url += "?" + param;
                else
                    url += "&" + param;


            HttpWebRequest request = CreateRequest(url, ref cookieContainer, refererUrl, userName, password, proxy,
                                                   userAgent, requestTimeOut, allowRedirect);

            if (encoding == Encoding.UTF8)
                request.Headers.Add("Accept-Charset", "utf-8");
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }

            if (isGet)
            {
                request.Method = "GET";
                //request.ContentType = "text/html";
            }
            else
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                // 设置提交的数据
                if (!string.IsNullOrEmpty(param))
                {
                    // 把数据转换为字节数组
                    byte[] l_data = encoding.GetBytes(param);
                    request.ContentLength = l_data.Length;
                    // 必须先设置ContentLength，才能打开GetRequestStream
                    // ContentLength设置后，reqStream.Close前必须写入相同字节的数据，否则Request会被取消
                    using (Stream newStream = request.GetRequestStream())
                    {
                        newStream.Write(l_data, 0, l_data.Length);
                        newStream.Close();
                    }
                }
                else
                    request.ContentLength = 0;// POST时，必须设置ContentLength属性
            }


            HttpWebResponse response;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                sw.Stop();
                processtime = sw.ElapsedMilliseconds;
            }
            #region WebRequest异常处理
            catch (WebException webExp)
            {
                sw.Stop();
                processtime = sw.ElapsedMilliseconds;
                if (webExp.Response != null)
                {
                    using (var responseErr = (HttpWebResponse)webExp.Response)
                    {
                        responseStatus = (int)responseErr.StatusCode;
                        Stream streamFail;
                        using (streamFail = responseErr.GetResponseStream())
                        {
                            var contentEncoding = responseErr.ContentEncoding.ToLower();
                            if (contentEncoding.Contains("gzip"))
                            {
                                streamFail = new GZipStream(streamFail, CompressionMode.Decompress);
                            }
                            else if (contentEncoding.Contains("deflate"))
                            {
                                streamFail = new DeflateStream(streamFail, CompressionMode.Decompress);
                            }
                            using (var sr = new StreamReader(streamFail, encoding))
                            {
                                var html = sr.ReadToEnd();
                                return encoding.GetBytes(html);
                            }
                        }
                    }
                }
                else
                {
                    responseStatus = 600;
                }
                return encoding.GetBytes("返回错误：" + webExp);
            }
            catch (Exception exp)
            {
                sw.Stop();
                processtime = sw.ElapsedMilliseconds;
                responseStatus = 700;
                return encoding.GetBytes("返回错误：" + exp);
            }
            #endregion

            responseStatus = (int)response.StatusCode;
            try
            {
                Stream stream;
                using (stream = response.GetResponseStream())
                {
                    if (stream == null)
                        return encoding.GetBytes("GetResponseStream为null");

                    var contentEncoding = response.ContentEncoding.ToLower();
                    if (contentEncoding.Contains("gzip"))
                    {
                        stream = new GZipStream(stream, CompressionMode.Decompress);
                    }
                    else if (contentEncoding.Contains("deflate"))
                    {
                        stream = new DeflateStream(stream, CompressionMode.Decompress);
                    }

                    List<byte> ret = new List<byte>(10000);
                    byte[] arr = new byte[10000];
                    int readcnt;
                    while ((readcnt = stream.Read(arr, 0, arr.Length)) > 0)
                    {
                        ret.AddRange(arr.Take(readcnt));
                    }
                    arr = ret.ToArray();

                    // 下面的方法ContentLength可能为-1，导致new byte时报算术溢出
                    // 即使ContentLength正确，Read时也可能读取2遍以上，所以使用上面的循环来读取返回值
                    //int len = (int)response.ContentLength; //stream.Length;
                    //byte[] arr = new byte[len];
                    //stream.Read(arr, 0, len);
                    isok = true;
                    return arr;
                }
            }
            catch (Exception exp)
            {
                responseStatus = 700;
                return encoding.GetBytes("返回错误：" + exp);
            }
        }

        /// <summary>
        /// 抓取页面
        /// </summary>
        /// <param name="url">要抓取的网址</param>
        /// <param name="isok">返回抓取结果，成功还是失败</param>
        /// <param name="processtime">返回抓取页面耗费的时间，毫秒</param>
        /// <param name="responseStatus">响应状态码，如200、302、503；600表示web异常；700表示其它异常</param>
        /// <param name="param">参数</param>
        /// <param name="HttpMethod">GET POST</param>
        /// <param name="encoding">编码格式，默认UTF-8</param>
        /// <param name="refererUrl">要设置的头信息的引用页</param>
        /// <param name="userName">网页登录名</param>
        /// <param name="password">登录密码</param>
        /// <param name="proxy">WebRequest要用到的代理</param>
        /// <param name="userAgent">要设置的头信息里的用户代理</param>
        /// <param name="requestTimeOut">WebRequest的最大请求时间，单位：毫秒，0为不设置</param>
        /// <param name="allowRedirect">出现301或302之类的转向时，是否要转向</param>
        /// <param name="headers">要设置的Header键值对</param>
        /// <returns></returns>
        public static byte[] GetPageBinary(string url, out bool isok, out long processtime, out int responseStatus,
            string param = null, string HttpMethod = null, Encoding encoding = null, string refererUrl = null,
            string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, Dictionary<string, string> headers = null)
        {
            return GetPageBinary(url, out isok, ref _cookie, out processtime, out responseStatus, param, HttpMethod, encoding,
                refererUrl, userName, password, proxy, userAgent, requestTimeOut, allowRedirect, headers);
        }

        static HttpWebRequest CreateRequest(string url, ref CookieContainer cookieContainer, 
            string refererUrl = null, string userName = null, string password = null, string proxy = null,
            string userAgent = null, int requestTimeOut = 0, bool allowRedirect = false, bool enableGzip = true)
        {
            // 访问Https网站时，加上特殊处理，用于处理证书有问题的网站
            bool isHttps = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            if (isHttps)
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");

            if (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(password))
                request.Credentials = new NetworkCredential(userName, password);

            #region 加Cookie
            if (cookieContainer == null)
            {
                cookieContainer = _cookie;
            }
            request.CookieContainer = cookieContainer;
            //            request.CookieContainer.SetCookies(new Uri(url), "aaa=bbb&ccc=ddd");// 必须一次全部加入Cookie
            #endregion

            if (string.IsNullOrEmpty(userAgent))
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0;)";
            //"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1;)";
            else
                request.UserAgent = userAgent;

            if (string.IsNullOrEmpty(refererUrl))
                request.Referer = url;
            else
                request.Referer = refererUrl;

            if (requestTimeOut > 0)
                request.Timeout = requestTimeOut; // 设置超时时间，默认值为 100,000 毫秒（100 秒）

            request.AllowAutoRedirect = allowRedirect; //出现301或302之类的转向时，是否要转向

            //request.KeepAlive = true;
            //request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
            request.Accept = "*/*";
            if (enableGzip)
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
            //request.Headers.Add("Accept-Language", "zh-cn,en-us");


            if (!string.IsNullOrEmpty(proxy))
            {
                #region 设置代理
                string[] tmp = proxy.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                int port = 80;
                if (tmp.Length >= 2)
                {
                    if (!int.TryParse(tmp[1], out port))
                    {
                        port = 80;
                    }
                }
                request.Proxy = new WebProxy(tmp[0], port);
                #endregion
            }
            // 如果配置为null，Fiddler2将捕获不到请求
            //else
            //{
            //    request.Proxy = null;
            //}
            return request;
        }

        /// <summary>
        /// 用于访问Https站点时，证书有问题，始终返回true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // Always accept
            //Console.WriteLine("accept" + certificate.GetName());
            return true; //总是接受
        }
        #endregion

        /// <summary>
        /// 把源文件，移动到目标文件，会尝试移动两次
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="targetFile">目标文件</param>
        /// <param name="keepOld">源文件是否保留</param>
        /// <param name="secondMoveWait">毫秒数,第一次移动失败，进行第二次移动前要等待的时间</param>
        /// <returns></returns>
        public static bool MoveFile(string sourceFile, string targetFile, bool keepOld = false, int secondMoveWait = 100)
        {
            try
            {
                if (keepOld)
                {
                    File.Copy(sourceFile, targetFile, true);
                }
                else
                {
                    if (File.Exists(targetFile))
                        File.Delete(targetFile);
                    File.Move(sourceFile, targetFile);
                }
                return true;
            }
            catch (Exception)
            {
                try
                {
                    Thread.Sleep(secondMoveWait);
                    if (keepOld)
                    {
                        File.Copy(sourceFile, targetFile, true);
                    }
                    else
                    {
                        if (File.Exists(targetFile))
                            File.Delete(targetFile);
                        File.Move(sourceFile, targetFile);
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
