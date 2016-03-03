using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace Zero.Core.Util
{
    /// <summary>
    /// 实现对图像的简单处理功能
    /// </summary>
    public class ImageHelper
    {
        /// <summary>
        /// 返回图片格式
        /// </summary>
        /// <param name="img"></param>
        /// <returns>jpeg\gif\bmp\png\tif\icon\wmf</returns>
        public static string GetImageFormat(Image img)
        {
            string strImgFormat = "";
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
                strImgFormat = "jpeg";
            else if (img.RawFormat.Equals(ImageFormat.Gif))
                strImgFormat = "gif";
            else if (img.RawFormat.Equals(ImageFormat.Bmp))
                strImgFormat = "bmp";
            else if (img.RawFormat.Equals(ImageFormat.Png))
                strImgFormat = "png";
            else if (img.RawFormat.Equals(ImageFormat.Tiff))
                strImgFormat = "tiff";
            else if (img.RawFormat.Equals(ImageFormat.Icon))
                strImgFormat = "icon";
            else if (img.RawFormat.Equals(ImageFormat.Wmf))
                strImgFormat = "wmf";

            return strImgFormat;
        }

        /// <summary> 
        /// 放大缩小图片尺寸 
        /// </summary> 
        /// <param name="picPath"></param> 
        /// <param name="reSizePicPath"></param> 
        /// <param name="h"></param> 
        /// <param name="w"></param> 
        /// <param name="format"></param> 
        public static void PicReSize(string picPath, string reSizePicPath, int h, int w, ImageFormat format)
        {
            Bitmap originBmp = new Bitmap(picPath);
            Bitmap resizedBmp = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(resizedBmp);
            //设置高质量插值法   
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度   
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //消除锯齿 
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawImage(originBmp, new Rectangle(0, 0, w, h), new Rectangle(0, 0, originBmp.Width, originBmp.Height), GraphicsUnit.Pixel);
            resizedBmp.Save(reSizePicPath, format);
            g.Dispose();
            resizedBmp.Dispose();
            originBmp.Dispose();
        }

        /// <summary>
        /// 生成缩略图  
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="miniaturePath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param> miniature
        /// <param name="mode">生成缩略图的方式 HW:指定高宽缩放（可能变形） W:指定宽，高按比例 H:指定高，宽按比例 Cut:指定高宽裁减（不变形）</param>    
        public static void GenerateMiniature(string originalImagePath, string miniaturePath, int width, int height, string mode)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

            int toWidth = width;
            int toHeight = height;

            int x = 0;
            int y = 0;
            int oW = originalImage.Width;
            int oH = originalImage.Height;

            switch (mode)
            {
                case "HW"://指定高宽缩放（可能变形）                
                    break;
                case "W"://指定宽，高按比例                    
                    toHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H"://指定高，宽按比例
                    toWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut"://指定高宽裁减（不变形）                
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)toWidth / (double)toHeight)
                    {
                        oH = originalImage.Height;
                        oW = originalImage.Height * toWidth / toHeight;
                        y = 0;
                        x = (originalImage.Width - oW) / 2;
                    }
                    else
                    {
                        oW = originalImage.Width;
                        oH = originalImage.Width * height / toWidth;
                        x = 0;
                        y = (originalImage.Height - oH) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(toWidth, toHeight);

            //新建一个画板

            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充

            g.Clear(System.Drawing.Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, toWidth, toHeight),
                new System.Drawing.Rectangle(x, y, oW, oH),
                System.Drawing.GraphicsUnit.Pixel);

            /////////////////////////////

            try
            {
                //以jpg格式保存缩略图

                bitmap.Save(miniaturePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }



        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="stream">用于生成图片的数据流</param>
        /// <param name="miniaturePath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param> miniature
        /// <param name="mode">生成缩略图的方式 HW:指定高宽缩放（可能变形） W:指定宽，高按比例 H:指定高，宽按比例 Cut:指定高宽裁减（不变形）</param>     
        public static void GenerateMiniatureByStream(Stream stream, string miniaturePath, int width, int height, string mode)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(stream);
            GenerateMiniatureByImage(originalImage, miniaturePath, width, height, mode);

            //int toWidth = width;
            //int toHeight = height;

            //int x = 0;
            //int y = 0;
            //int oW = originalImage.Width;
            //int oH = originalImage.Height;

            //switch (mode)
            //{
            //    case "HW"://指定高宽缩放（可能变形）                
            //        break;
            //    case "W"://指定宽，高按比例                    
            //        toHeight = originalImage.Height * width / originalImage.Width;
            //        break;
            //    case "H"://指定高，宽按比例
            //        toWidth = originalImage.Width * height / originalImage.Height;
            //        break;
            //    case "Cut"://指定高宽裁减（不变形）                
            //        if ((double)originalImage.Width / (double)originalImage.Height > (double)toWidth / (double)toHeight)
            //        {
            //            oH = originalImage.Height;
            //            oW = originalImage.Height * toWidth / toHeight;
            //            y = 0;
            //            x = (originalImage.Width - oW) / 2;
            //        }
            //        else
            //        {
            //            oW = originalImage.Width;
            //            oH = originalImage.Width * height / toWidth;
            //            x = 0;
            //            y = (originalImage.Height - oH) / 2;
            //        }
            //        break;
            //    default:
            //        break;
            //}

            ////新建一个bmp图片
            //System.Drawing.Image bitmap = new System.Drawing.Bitmap(toWidth, toHeight);

            ////新建一个画板

            //System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            ////设置高质量插值法
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            ////设置高质量,低速度呈现平滑程度
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            ////清空画布并以透明背景色填充

            //g.Clear(System.Drawing.Color.Transparent);

            ////在指定位置并且按指定大小绘制原图片的指定部分
            //g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, toWidth, toHeight),
            //    new System.Drawing.Rectangle(x, y, oW, oH),
            //    System.Drawing.GraphicsUnit.Pixel);



            //try
            //{
            //    //以jpg格式保存缩略图

            //    bitmap.Save(miniaturePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            //}
            //catch (System.Exception e)
            //{
            //    throw e;
            //}
            //finally
            //{
            //    originalImage.Dispose();
            //    bitmap.Dispose();
            //    g.Dispose();
            //}
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="miniaturePath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param> miniature
        /// <param name="mode">生成缩略图的方式 HW:指定高宽缩放（可能变形） W:指定宽，高按比例 H:指定高，宽按比例 Cut:指定高宽裁减（不变形）</param>     
        public static void GenerateMiniatureByImage(Image originalImage, string miniaturePath, int width, int height, string mode)
        {
            int toWidth = width;
            int toHeight = height;

            int x = 0;
            int y = 0;
            int oW = originalImage.Width;
            int oH = originalImage.Height;

            switch (mode)
            {
                case "HW"://指定高宽缩放（可能变形）                
                    break;
                case "W"://指定宽，高按比例                    
                    toHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H"://指定高，宽按比例
                    toWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut"://指定高宽裁减（不变形）                
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)toWidth / (double)toHeight)
                    {
                        oH = originalImage.Height;
                        oW = originalImage.Height * toWidth / toHeight;
                        y = 0;
                        x = (originalImage.Width - oW) / 2;
                    }
                    else
                    {
                        oW = originalImage.Width;
                        oH = originalImage.Width * height / toWidth;
                        x = 0;
                        y = (originalImage.Height - oH) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(toWidth, toHeight);
            //新建一个画板
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.Transparent);
            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, toWidth, toHeight),
                new System.Drawing.Rectangle(x, y, oW, oH),
                System.Drawing.GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图
                bitmap.Save(miniaturePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        /// <summary>
        /// 修改成低质量图片(节省空间)
        /// </summary>
        /// <param name="serverFile">要修改的文件地址</param>
        public void Modity2LowQualityImage(string serverFile)
        {
            GenLowQualityImage(serverFile, serverFile, 0, 0, null);
        }
        /// <summary>
        /// 修改成特定大小格式的图片
        /// </summary>
        /// <param name="serverFile">要修改的文件地址</param>
        /// <param name="width">宽度，如果大于图片的宽度则取图片宽度，如果为0，取原始宽度</param>
        /// <param name="height">高度</param>
        /// <param name="imgFmt">目标图片格式</param>
        public void Modity2LowQualityImage(string serverFile, int width, int height, ImageFormat imgFmt)
        {
            GenLowQualityImage(serverFile, serverFile, width, height, imgFmt);
        }
        /// <summary>
        /// 生成低质量图片
        /// </summary>
        /// <param name="serverFile">原始地址</param>
        /// <param name="savePathFile">目标地址</param>
        public void GenLowQualityImage(string serverFile, string savePathFile)
        {
            GenLowQualityImage(serverFile, savePathFile, 0, 0, null);
        }
        /// <summary>
        /// 生成低质量的缩略图，文件格式默认与原始格式一样
        /// </summary>
        /// <param name="serverFile">原始文件位置</param>
        /// <param name="savePathFile">保存目标文件位置</param>
        /// <param name="iWidth">宽度，如果大于图片的宽度则取图片宽度，如果为0，取原始宽度</param>
        /// <param name="iHeight">高度，同宽度</param>
        public void GenLowQualityImage(string serverFile, string savePathFile, int iWidth, int iHeight)
        {
            GenLowQualityImage(serverFile, savePathFile, iWidth, iHeight, null);
        }
        /// <summary>
        /// 生成低质量的图标文件
        /// </summary>
        /// <param name="serverFile">原始文件位置</param>
        /// <param name="savePathFile">保存目标文件位置</param>
        /// <param name="iWidth">宽度，如果大于图片的宽度则取图片宽度，如果为0，取原始宽度</param>
        /// <param name="iHeight">高度，同宽度</param>
        /// <param name="imgFmt">图片格式，如果null，那么取原始图片的格式</param>
        public void GenLowQualityImage(string serverFile, string savePathFile, int iWidth, int iHeight, ImageFormat imgFmt)
        {
            System.Drawing.Image oImg = System.Drawing.Image.FromFile(serverFile);
            System.Drawing.Image imgThumb = Modify2LowQualityImage(oImg, iWidth, iHeight);
            ImageFormat format = oImg.RawFormat;
            oImg.Dispose();
            imgThumb.Save(savePathFile, imgFmt == null ? format : imgFmt);
            imgThumb.Dispose();
        }
        /// <summary>
        /// 生成高质量的所略图，文件格式默认与原始格式一样
        /// </summary>
        /// <param name="serverFile">原始文件位置</param>
        /// <param name="savePathFile">保存目标文件位置</param>
        /// <param name="iWidth">宽度，如果大于图片的宽度则取图片宽度，如果为0，取原始宽度</param>
        /// <param name="iHeight">高度，同宽度</param>
        /// <param name="bgColor">背景颜色</param>
        public void GenHighQualityImage(string serverFile, string savePathFile, int iWidth, int iHeight, Color bgColor)
        {
            GenHighQualityImage(serverFile, savePathFile, iWidth, iHeight, bgColor, null, null, -1, -1);
        }
        /// <summary>
        /// 生成高质量的所略图
        /// </summary>
        /// <param name="serverFile">原始文件位置</param>
        /// <param name="savePathFile">保存目标文件位置</param>
        /// <param name="iWidth">宽度，如果大于图片的宽度则取图片宽度，如果为0，取原始宽度</param>
        /// <param name="iHeight">高度，同宽度</param>
        /// <param name="bgColor">背景颜色</param>
        /// <param name="imgFmt">图片格式，默认与原始文件一样</param>
        /// <param name="writeText">要在图片上写的文本</param>
        /// <param name="textX">位置X</param>
        /// <param name="textY">位置Y</param>
        public void GenHighQualityImage(string serverFile, string savePathFile, int iWidth, int iHeight, Color bgColor, ImageFormat imgFmt, string writeText, int textX, int textY)
        {
            System.Drawing.Image oImg = System.Drawing.Image.FromFile(serverFile);
            Image b = Modity2HighQualityImage(oImg, iWidth, iHeight, bgColor);
            WriteTextOnImage(b, writeText, textX, textY);
            ImageFormat format = oImg.RawFormat;

            // 以下代码为保存图片时,设置压缩质量
            EncoderParameters encoderParams = new EncoderParameters();
            long[] quality = new long[1];
            quality[0] = 100;
            EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            //获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象.
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo jpegICI = null;
            for (int x = 0; x < arrayICI.Length; x++)
            {
                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[x];
                    //设置JPEG编码
                    break;
                }
            }
            if (jpegICI != null)
            {
                b.Save(savePathFile, jpegICI, encoderParams);
            }
            else
            {
                b.Save(savePathFile, imgFmt == null ? format : imgFmt);
            }

            oImg.Dispose();
            //b.Save(savePathFile, imgFmt == null ? format : imgFmt);
            b.Dispose();
        }
        /// <summary>
        /// 在图片上叠加Log
        /// </summary>
        /// <param name="serverFile">源文件</param>
        /// <param name="logoFile">Log文件</param>
        /// <param name="iWidth"></param>
        /// <param name="iHeight"></param>
        public void ModifyImageWithLogo(string serverFile, string logoFile, int iWidth, int iHeight)
        {
            System.Drawing.Image oImg = System.Drawing.Image.FromFile(serverFile);
            Image b = Modity2HighQualityImage(oImg, iWidth, iHeight, Color.White);
            DrawLogoOnImage(b, logoFile, 0, 0);
            ImageFormat format = oImg.RawFormat;
            oImg.Dispose();
            b.Save(serverFile, format);
            b.Dispose();
        }
        /// <summary>
        /// 在图片上叠加Log 默认全图(左上角)
        /// </summary>
        /// <param name="serverFile">源文件</param>
        /// <param name="logoFile">Log文件</param>
        public void ModifyImageWithLogo(string serverFile, string logoFile)
        {
            System.Drawing.Image oImg = System.Drawing.Image.FromFile(serverFile);
            Image b = Modity2HighQualityImage(oImg, 0, 0, Color.White);
            DrawLogoOnImage(b, logoFile, 0, 0);
            ImageFormat format = oImg.RawFormat;
            oImg.Dispose();
            b.Save(serverFile, format);
            b.Dispose();
        }
        /// <summary>
        /// 在图片上叠加Log 默认全图(右下角)
        /// </summary>
        /// <param name="serverFile">源文件</param>
        /// <param name="logoFile">Log文件</param>
        /// <param name="bRightDown">水印打在图片右下角<remarks>如果水印图片的长或者宽大于原图的长宽，则把水印打在左上角</remarks></param>
        public void ModifyImageWithLogo(string serverFile, string logoFile, bool bRightDown)
        {
            if(!bRightDown)
            {
                ModifyImageWithLogo(serverFile, logoFile);
                return;
            }

            System.Drawing.Image oImg = System.Drawing.Image.FromFile(serverFile);
            Image b = Modity2HighQualityImage(oImg, 0, 0, Color.White);
            DrawLogoOnImage(b, logoFile, 0, 0, true);
            ImageFormat format = oImg.RawFormat;
            oImg.Dispose();
            b.Save(serverFile, format);
            b.Dispose();
        }
        /// <summary>
        /// 生成高质量的缩略图
        /// </summary>
        /// <param name="serverFile">原始文件位置</param>
        /// <param name="savePathFile">保存目标文件位置</param>
        /// <param name="logoFile">图标文件地址</param>
        /// <param name="iWidth">宽度，如果大于图片的宽度则取图片宽度</param>
        /// <param name="iHeight">高度，同宽度</param>
        /// <param name="bgColor">背景颜色</param>
        /// <param name="imgFmt">图片格式，默认与原始文件一样</param>
        /// <param name="logoX">位置X</param>
        /// <param name="logoY">位置Y</param>
        public void GenImageWithLogo(string serverFile, string savePathFile, string logoFile, int iWidth, int iHeight, Color bgColor, ImageFormat imgFmt, int logoX, int logoY)
        {
            System.Drawing.Image oImg = System.Drawing.Image.FromFile(serverFile);
            Image b = Modity2HighQualityImage(oImg, iWidth, iHeight, bgColor);
            DrawLogoOnImage(b, logoFile, logoX, logoY);
            ImageFormat format = oImg.RawFormat;
            oImg.Dispose();
            b.Save(savePathFile, imgFmt == null ? format : imgFmt);
            b.Dispose();
        }
        /// <summary>
        /// 在图片上写文字
        /// </summary>
        /// <param name="sourceFile">原始文件位置</param>
        /// <param name="destFile">保存目标文件位置</param>
        /// <param name="text">要在图片上写的文本</param>
        /// <param name="font">字体</param>
        /// <param name="textX">位置X</param>
        /// <param name="textY">位置Y</param>
        public void WriteTextOnImage(string sourceFile, string destFile, string text, Font font, int textX, int textY)
        {
            System.Drawing.Image oImg = System.Drawing.Image.FromFile(sourceFile);
            Bitmap bitMapImage = new System.Drawing.Bitmap(oImg, oImg.Width, oImg.Height);
            WriteTextOnImage(oImg, text, textX, textY);
            oImg.Dispose();
            bitMapImage.Save(destFile);
            bitMapImage.Dispose();
        }
        private void GetWidthHeight(Image sourceImage, int iWidth, int iHeight, out int width, out int height)
        {
            Image oImg = sourceImage;
            int intwidth = 0, intheight = 0;
            if (iWidth <= 0)
            {
                intwidth = oImg.Width;
            }
            else if (oImg.Width > iWidth)
            {
                intwidth = iWidth;
                intheight = (oImg.Height * iWidth) / oImg.Width;
            }
            else
            {
                intwidth = oImg.Width;
                intheight = oImg.Height;
            }

            if (iHeight <= 0)
            {
                intheight = oImg.Height;
            }
            else if (oImg.Height > iHeight)
            {
                intwidth = (oImg.Width * iHeight) / oImg.Height;
                intheight = iHeight;
            }
            else
            {
                intwidth = oImg.Width;
                intheight = oImg.Height;
            }
            width = intwidth;
            height = intheight;
        }
        /// <summary>
        /// 修改成低质量图片，返回Image对象
        /// </summary>
        /// <param name="sourceImage">原始图像</param>
        /// <param name="iWidth">宽度</param>
        /// <param name="iHeight">高度</param>
        /// <returns>修改后的Image对象</returns>
        private Image Modify2LowQualityImage(Image sourceImage, int iWidth, int iHeight)
        {
            Image oImg = sourceImage;
            int intwidth = 0, intheight = 0;
            GetWidthHeight(sourceImage, iWidth, iHeight, out intwidth, out intheight);
            System.Drawing.Image imgThumb = oImg.GetThumbnailImage(intwidth, intheight, null, System.IntPtr.Zero);
            return imgThumb;
        }

        private static Image CreateThumbnail(Image source, int thumbWi, int thumbHi, bool maintainAspect)
        {
            // return the source image if it's smaller than the designated thumbnail
            if (source.Width < thumbWi && source.Height < thumbHi) return source;

            System.Drawing.Bitmap ret = null;
            try
            {
                int wi, hi;

                wi = thumbWi;
                hi = thumbHi;

                if (maintainAspect)
                {
                    // maintain the aspect ratio despite the thumbnail size parameters
                    if (source.Width > source.Height)
                    {
                        wi = thumbWi;
                        hi = (int)(source.Height * ((decimal)thumbWi / source.Width));
                    }
                    else
                    {
                        hi = thumbHi;
                        wi = (int)(source.Width * ((decimal)thumbHi / source.Height));
                    }
                }

                // original code that creates lousy thumbnails
                // System.Drawing.Image ret = source.GetThumbnailImage(wi,hi,null,IntPtr.Zero);
                ret = new Bitmap(wi, hi);
                using (Graphics g = Graphics.FromImage(ret))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.FillRectangle(Brushes.White, 0, 0, wi, hi);
                    g.DrawImage(source, 0, 0, wi, hi);
                }
            }
            catch
            {
                ret = null;
            }

            return ret;
        }

        /// <summary>
        /// 生成图片缩略图（自定义图片质量）
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="qualityValue">;0to100最高质量为100</param>
        /// <param name="iWidth"></param>
        /// <param name="iHeight"></param>
        public static void ProduceHighQualityThumb(string filePath, int qualityValue, int iWidth, int iHeight)
        {
            Image image = Image.FromFile(filePath);
            System.Drawing.Image myThumbnail = CreateThumbnail(image, iWidth, iHeight, false);
            image.Dispose();

            //Configure JPEG Compression Engine
            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            long[] quality = new long[1];
            quality[0] = qualityValue;
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;

            System.Drawing.Imaging.ImageCodecInfo[] arrayICI = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            System.Drawing.Imaging.ImageCodecInfo jpegICI = null;
            for (int x = 0; x < arrayICI.Length; x++)
            {
                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[x];
                    break;
                }
            }

            myThumbnail.Save(filePath, jpegICI, encoderParams);
            myThumbnail.Dispose();
        }

        private Image Modity2HighQualityImage(Image sourceImage, int iWidth, int iHeight, Color bgColor)
        {
            Image oImg = sourceImage;
            int intwidth = 0, intheight = 0;
            GetWidthHeight(sourceImage, iWidth, iHeight, out intwidth, out intheight);
            Bitmap b = new Bitmap(intwidth, intheight);
            Graphics g = Graphics.FromImage(b);
            Color myColor = bgColor;
            g.Clear(myColor);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(oImg, new Rectangle(0, 0, intwidth, intheight), new Rectangle(0, 0, oImg.Width, oImg.Height), GraphicsUnit.Pixel);
            return b;
        }
        private void DrawLogoOnImage(Image sourceImage, string logoFile, int logoX, int logoY)
        {
            Image imgBG = Image.FromFile(logoFile);
            float[][] ptsArray ={ 
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 0.618f, 0}, 
                new float[] {0, 0, 0, 0, 1}};
            ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
            ImageAttributes imgAttributes = new ImageAttributes();
            imgAttributes.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            Graphics.FromImage(sourceImage).DrawImage(imgBG, new Rectangle(logoX, logoY, imgBG.Width, imgBG.Height), logoX, logoY, imgBG.Width, imgBG.Height, GraphicsUnit.Pixel, imgAttributes);
        }
        private void DrawLogoOnImage(Image sourceImage, string logoFile, int logoX, int logoY, bool bRightDown)
        {
            Image imgBG = Image.FromFile(logoFile);
            int destLogoX = logoX;
            int destLogoY = logoY;
            if(bRightDown)
            {
                if (sourceImage.Width > imgBG.Width && sourceImage.Height > imgBG.Height)
                {
                    destLogoX = sourceImage.Width - imgBG.Width - 10;
                    destLogoY = sourceImage.Height - imgBG.Height - 10;
                }
            }

            float[][] ptsArray ={ 
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 0.618f, 0}, 
                new float[] {0, 0, 0, 0, 1}};
            ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
            ImageAttributes imgAttributes = new ImageAttributes();
            imgAttributes.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            Graphics.FromImage(sourceImage).DrawImage(imgBG, new Rectangle(destLogoX, destLogoY, imgBG.Width, imgBG.Height), logoX, logoY, imgBG.Width, imgBG.Height, GraphicsUnit.Pixel, imgAttributes);
            imgBG.Dispose();
        }
        private void WriteTextOnImage(Image sourceImage, string writeText, int textX, int textY)
        {
            int intwidth = sourceImage.Width;
            int intheight = sourceImage.Height;
            if (!String.IsNullOrEmpty(writeText))
            {
                textX = (textX >= 0 && textX <= intwidth) ? textX : 0;
                textY = (textY >= 0 && textY <= intheight) ? textY : 0;
                WriteTextOnImage(Graphics.FromImage(sourceImage), writeText, null, textX, textY);
            }
        }
        private void WriteTextOnImage(Graphics gra, string text, Font font, int textX, int textY)
        {
            if (font == null)
            {
                font = new Font("宋体", 12, FontStyle.Bold);
            }
            gra.SmoothingMode = SmoothingMode.AntiAlias;
            gra.DrawString(text, font, SystemBrushes.WindowText, new Point(textX, textY));
        }
        /// <summary>
        /// 图片
        /// </summary>
        public ImageHelper()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        /// <summary>
        /// 判断是否是JPG
        /// </summary>
        /// <param name="filePath">文件的完整路径 </param>
        /// <returns></returns>
        public static bool IsJPG(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);
                string fileClass;
                byte buffer;
                byte[] b = new byte[2];
                buffer = reader.ReadByte();
                b[0] = buffer;
                fileClass = buffer.ToString();
                buffer = reader.ReadByte();
                b[1] = buffer;
                fileClass += buffer.ToString();

                reader.Close();
                fs.Close();
                if (fileClass == "255216")//255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar 
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>  
        /// 圆角生成（但是只能是一个角）  
        /// </summary>  
        /// <param name="image">源图片 Image</param>  
        /// <param name="roundCorner">圆角位置</param>  
        /// <returns>处理好的Image</returns>  
        public static Image CreateRoundedCorner(Image image, RoundRectanglePosition roundCorner)
        {
            Graphics g = Graphics.FromImage(image);
            //保证图片质量  
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            //构建圆角外部路径  
            GraphicsPath rectPath = CreateRoundRectanglePath(rect, image.Width / 10, roundCorner);
            //圆角背景用白色填充  
            Brush b = new SolidBrush(Color.White);
            g.DrawPath(new Pen(b), rectPath);
            g.FillPath(b, rectPath);
            g.Dispose();
            return image;
        }

        /// <summary>  
        /// 圆角生成（但是只能是一个角）  
        /// </summary>  
        /// <param name="image">源图片 Image</param>  
        /// <param name="roundCorner">圆角位置</param>  
        /// <returns>处理好的Image</returns>  
        public static Image CreateTransparentRoundedCorner(Image image, RoundRectanglePosition roundCorner)
        {
            Graphics g = Graphics.FromImage(image);
            //保证图片质量  
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            //构建圆角外部路径  
            GraphicsPath rectPath = CreateRoundRectanglePath(rect, image.Width / 10, roundCorner);
            //圆角背景用白色填充  
            Brush b = new SolidBrush(Color.White);
            g.DrawPath(new Pen(b), rectPath);
            g.FillPath(b, rectPath);
            g.Dispose();
            return image;
        }

        /// <summary>  
        /// 目标图片的圆角位置  
        /// </summary>  
        public enum RoundRectanglePosition
        {
            /// <summary>  
            /// 左上角  
            /// </summary>  
            TopLeft,
            /// <summary>  
            /// 右上角  
            /// </summary>  
            TopRight,
            /// <summary>  
            /// 左下角  
            /// </summary>  
            BottomLeft,
            /// <summary>  
            /// 右下角  
            /// </summary>  
            BottomRight
        }
        /// <summary>  
        /// 构建GraphicsPath路径  
        /// </summary>  
        /// <param name="rect"></param>  
        /// <param name="radius"></param>  
        /// <param name="rrPosition">图片圆角位置</param>  
        /// <returns>返回GraphicPath</returns>  
        private static GraphicsPath CreateRoundRectanglePath(Rectangle rect, int radius, RoundRectanglePosition rrPosition)
        {
            GraphicsPath rectPath = new GraphicsPath();
            switch (rrPosition)
            {
                case RoundRectanglePosition.TopLeft:
                    {
                        rectPath.AddArc(rect.Left, rect.Top, radius * 2, radius * 2, 180, 90);
                        rectPath.AddLine(rect.Left, rect.Top, rect.Left, rect.Top + radius);
                        break;
                    }
                case RoundRectanglePosition.TopRight:
                    {
                        rectPath.AddArc(rect.Right - radius * 2, rect.Top, radius * 2, radius * 2, 270, 90);
                        rectPath.AddLine(rect.Right, rect.Top, rect.Right - radius, rect.Top);
                        break;
                    }
                case RoundRectanglePosition.BottomLeft:
                    {
                        rectPath.AddArc(rect.Left, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        rectPath.AddLine(rect.Left, rect.Bottom - radius, rect.Left, rect.Bottom);
                        break;
                    }
                case RoundRectanglePosition.BottomRight:
                    {
                        rectPath.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        rectPath.AddLine(rect.Right - radius, rect.Bottom, rect.Right, rect.Bottom);
                        break;
                    }
            }
            return rectPath;
        }

        #region 生成圆角图（底色透明）
        /// <summary>
        /// 生成圆角图（底色透明）
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Image DrawTransparentRoundCornerImage(Image image)
        {
            Bitmap bm = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage(bm);
            g.FillRectangle(Brushes.Transparent, new Rectangle(0, 0, image.Width, image.Height));

            using (GraphicsPath path = CreateRoundedRectanglePath(new Rectangle(0, 0, image.Width, image.Height), image.Width / 5))
            {
                g.SetClip(path);
            }

            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            g.Dispose();

            return bm;
        }

        /// <summary>
        /// 生成圆角图（底色透明）
        /// </summary>
        /// <param name="image"></param>
        /// <param name="cornerRadius"></param>
        /// <returns></returns>
        public static Image DrawTransparentRoundCornerImage(Image image, int cornerRadius)
        {
            Bitmap bm = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage(bm);
            g.FillRectangle(Brushes.Transparent, new Rectangle(0, 0, image.Width, image.Height));

            using (GraphicsPath path = CreateRoundedRectanglePath(new Rectangle(0, 0, image.Width, image.Height), cornerRadius))
            {
                g.SetClip(path);
            }

            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            g.Dispose();

            return bm;
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath roundedRect = new GraphicsPath();
            roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
            roundedRect.CloseFigure();
            return roundedRect;
        }
        #endregion

    }

}