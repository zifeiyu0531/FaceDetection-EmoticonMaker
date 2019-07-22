using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceDetection_Form_01
{
    public class AdjustFunction
    {
        // ImageSource --> Bitmap
        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            Bitmap bmp = new Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // 坑点：选Format32bppRgb将不带透明度

            BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }

        // BitmapImage --> Bitmap
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        // Bitmap --> BitmapImage
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        //颜色转化
        public static System.Windows.Media.Color GetMediaColorFromDrawingColor(System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        //获取原图背景色
        public static System.Drawing.Color GetBackColor(IList<DetectedFace> faceList)
        {
            System.Drawing.Color backColor;
            double x = faceList[0].FaceRectangle.Left * resizeFactor + 10;
            double y = faceList[0].FaceRectangle.Top * resizeFactor + 30;
            Bitmap bmp = BitmapImageToBitmap(MainForm.ExtraImage);
            backColor = bmp.GetPixel((int)x, (int)y);
            return backColor;
        }

        //边缘模糊
        public static RenderTargetBitmap SetEdgeBlur(Bitmap bitmap,
             int blurRange, IList<DetectedFace> faceList)
        {
            System.Drawing.Color backColor = GetBackColor(faceList);
            Bitmap b = bitmap;
            Graphics g = Graphics.FromImage(b);
            Rectangle rect = new System.Drawing.Rectangle(b.Width - blurRange, 0, blurRange, b.Height);

            rect = new System.Drawing.Rectangle(0, 0, b.Width, blurRange);
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, backColor,
                System.Drawing.Color.FromArgb(0, backColor), LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);

            }

            rect = new Rectangle(0, 0, blurRange, b.Height);
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, backColor,
                System.Drawing.Color.FromArgb(0, backColor), LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, rect);

            }

            rect = new System.Drawing.Rectangle(b.Width - blurRange, 0, blurRange, b.Height);
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                 System.Drawing.Color.FromArgb(0, backColor), backColor, LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, rect);

            }

            rect = new System.Drawing.Rectangle(0, b.Height - blurRange, b.Width, blurRange);
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
       System.Drawing.Color.FromArgb(0, backColor), backColor, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            
            return Redraw(faceList,BitmapToBitmapImage(b));
        }

        //设置透明度  
        //定义图像透明度调整函数
        public static RenderTargetBitmap SetTrans(Bitmap src, int num, IList<DetectedFace> faceList)
        {
            try
            {
                int w = src.Width;
                int h = src.Height;
                Bitmap dstBitmap = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData srcData = src.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData dstData = dstBitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* pIn = (byte*)srcData.Scan0.ToPointer();
                    byte* pOut = (byte*)dstData.Scan0.ToPointer();
                    byte* p;
                    int stride = srcData.Stride;
                    int r, g, b;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            p = pIn;
                            b = pIn[0];
                            g = pIn[1];
                            r = pIn[2];
                            pOut[1] = (byte)g;
                            pOut[2] = (byte)r;
                            pOut[3] = (byte)num;
                            pOut[0] = (byte)b;
                            pIn += 4;
                            pOut += 4;
                        }
                        pIn += srcData.Stride - w * 4;
                        pOut += srcData.Stride - w * 4;
                    }
                    src.UnlockBits(srcData);
                    dstBitmap.UnlockBits(dstData);
                    return Redraw(faceList, BitmapToBitmapImage(dstBitmap));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                return null;
            }
        }
        public static BitmapImage SetTrans(Bitmap src, int num)
        {
            try
            {
                int w = src.Width;
                int h = src.Height;
                Bitmap dstBitmap = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData srcData = src.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData dstData = dstBitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* pIn = (byte*)srcData.Scan0.ToPointer();
                    byte* pOut = (byte*)dstData.Scan0.ToPointer();
                    byte* p;
                    int stride = srcData.Stride;
                    int r, g, b;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            p = pIn;
                            b = pIn[0];
                            g = pIn[1];
                            r = pIn[2];
                            pOut[1] = (byte)g;
                            pOut[2] = (byte)r;
                            pOut[3] = (byte)num;
                            pOut[0] = (byte)b;
                            pIn += 4;
                            pOut += 4;
                        }
                        pIn += srcData.Stride - w * 4;
                        pOut += srcData.Stride - w * 4;
                    }
                    src.UnlockBits(srcData);
                    dstBitmap.UnlockBits(dstData);
                    return BitmapToBitmapImage(dstBitmap);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                return null;
            }
        }

        //设置亮度     
        public static RenderTargetBitmap BrightnessP(Bitmap a, int v, IList<DetectedFace> faceList)
        {
            System.Drawing.Imaging.BitmapData bmpData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int bytes = a.Width * a.Height * 3;
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;
            unsafe
            {
                byte* p = (byte*)ptr;
                int temp;
                for (int j = 0; j < a.Height; j++)
                {
                    for (int i = 0; i < a.Width * 3; i++, p++)
                    {
                        temp = (int)(p[0] + v);
                        temp = (temp > 255) ? 255 : temp < 0 ? 0 : temp;
                        p[0] = (byte)temp;
                    }
                    p += stride - a.Width * 3;
                }
            }
            a.UnlockBits(bmpData);
            return Redraw(faceList, BitmapToBitmapImage(a));
        }
        public static BitmapImage BrightnessP(Bitmap a, int v)
        {
            System.Drawing.Imaging.BitmapData bmpData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int bytes = a.Width * a.Height * 3;
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;
            unsafe
            {
                byte* p = (byte*)ptr;
                int temp;
                for (int j = 0; j < a.Height; j++)
                {
                    for (int i = 0; i < a.Width * 3; i++, p++)
                    {
                        temp = (int)(p[0] + v);
                        temp = (temp > 255) ? 255 : temp < 0 ? 0 : temp;
                        p[0] = (byte)temp;
                    }
                    p += stride - a.Width * 3;
                }
            }
            a.UnlockBits(bmpData);
            return BitmapToBitmapImage(a);
        }


        //变灰
        public static Bitmap ToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    System.Drawing.Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        //黑白处理
        public static Bitmap BlackAndwhite(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Bitmap bm = new Bitmap(width, height);//初始化一个Bitmap对象，用来记录处理后的图片
            int x, y, result;//x,y是循环次数，result是记录处理后的像素值
            System.Drawing.Color pixel;

            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    pixel = bmp.GetPixel(x, y);//获取当前坐标的像素值
                    result = (pixel.R + pixel.G + pixel.B) / 3;//取红绿蓝三色的平均值
                    //绘图，把处理后的值赋值到刚才定义的bm对象里面
                    bm.SetPixel(x, y, System.Drawing.Color.FromArgb(result, result, result));
                }
            }
            return bm;//返回黑白图片            
        }

        //对比度
        public static RenderTargetBitmap KiContrast(Bitmap b, int degree, IList<DetectedFace> faceList)
        {
            if (b == null)
            {
                return null;
            }

            if (degree < -100) degree = -100;
            if (degree > 100) degree = 100;

            try
            {

                double pixel = 0;
                double contrast = (100.0 + degree) / 100.0;
                contrast *= contrast;
                int width = b.Width;
                int height = b.Height;
                BitmapData data = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* p = (byte*)data.Scan0;
                    int offset = data.Stride - width * 3;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // 处理指定位置像素的对比度
                            for (int i = 0; i < 3; i++)
                            {
                                pixel = ((p[i] / 255.0 - 0.5) * contrast + 0.5) * 255;
                                if (pixel < 0) pixel = 0;
                                if (pixel > 255) pixel = 255;
                                p[i] = (byte)pixel;
                            } // i
                            p += 3;
                        } // x
                        p += offset;
                    } // y
                }
                b.UnlockBits(data);
                return Redraw(faceList, BitmapToBitmapImage(b));
            }
            catch
            {
                return null;
            }
        } // end of Contrast
        public static BitmapImage KiContrast(Bitmap b, int degree)
        {
            if (b == null)
            {
                return null;
            }

            if (degree < -100) degree = -100;
            if (degree > 100) degree = 100;

            try
            {

                double pixel = 0;
                double contrast = (100.0 + degree) / 100.0;
                contrast *= contrast;
                int width = b.Width;
                int height = b.Height;
                BitmapData data = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* p = (byte*)data.Scan0;
                    int offset = data.Stride - width * 3;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // 处理指定位置像素的对比度
                            for (int i = 0; i < 3; i++)
                            {
                                pixel = ((p[i] / 255.0 - 0.5) * contrast + 0.5) * 255;
                                if (pixel < 0) pixel = 0;
                                if (pixel > 255) pixel = 255;
                                p[i] = (byte)pixel;
                            } // i
                            p += 3;
                        } // x
                        p += offset;
                    } // y
                }
                b.UnlockBits(data);
                return BitmapToBitmapImage(b);
            }
            catch
            {
                return null;
            }
        } // end of Contrast

        //添加文字
        public static FormattedText formattedText;
        
        static double resizeFactor;
        //重绘
        public static RenderTargetBitmap Redraw(IList<DetectedFace> faceList, BitmapImage bitmapImage)
        {
            System.Drawing.Color backColor = GetBackColor(faceList);
            
            string[] faceDescriptions;
            // Prepare to draw rectangles around the faces.
            DrawingVisual visual = new DrawingVisual();
            DrawingContext drawingContext = visual.RenderOpen();
            drawingContext.DrawImage(MainForm.ExtraImage,
                new Rect(0, 0, MainForm.ExtraImage.Width, MainForm.ExtraImage.Height));
            double dpi = MainForm.ExtraImage.DpiX;
            // Some images don't contain dpi info.
            resizeFactor = (dpi == 0) ? 1 : 96 / dpi;
            faceDescriptions = new String[faceList.Count];

            for (int i = 0; i < faceList.Count; ++i)
            {
                DetectedFace face = faceList[i];              
                // Draw a rectangle on the face.
               

                drawingContext.DrawImage(bitmapImage,
                    new Rect(
                        face.FaceRectangle.Left * resizeFactor + 15,
                        face.FaceRectangle.Top * resizeFactor + 5,
                        face.FaceRectangle.Width * resizeFactor-20,
                        face.FaceRectangle.Height * resizeFactor
                        )
                );
                drawingContext.DrawText(formattedText, 
                    new System.Windows.Point(
                        (face.FaceRectangle.Left-30) * resizeFactor,
                        (face.FaceRectangle.Top + face.FaceRectangle.Height+30) * resizeFactor
                    ));
            }

            drawingContext.Close();

            // Display the image with the rectangle around the face.
            RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                (int)(MainForm.ExtraImage.PixelWidth * resizeFactor),
                (int)(MainForm.ExtraImage.PixelHeight * resizeFactor),
                96,
                96,
                PixelFormats.Pbgra32);

            faceWithRectBitmap.Render(visual);

            return faceWithRectBitmap;
        }
    }
}
