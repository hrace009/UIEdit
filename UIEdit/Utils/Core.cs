using FreeImageAPI;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace UIEdit.Utils
{
    public class Core
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);

        public static void ClearMemory()
        {
            GC.Collect();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }

        public static ImageSource GetImageSourceFromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return new BitmapImage();
            var pngFileName = fileName.Replace(".dds", ".png").
                Replace(".DDS", ".png").
                Replace(".tga", ".png").
                Replace(".jpg", ".png").
                Replace(".TGA", ".png");

            if (!File.Exists(pngFileName) && File.Exists(fileName))
            {
                var ms = new FileStream(fileName, FileMode.Open);
                var dds = FreeImage.LoadFromStream(ms);
                if (dds.IsNull)
                    return new BitmapImage();
                ms.Close();
                try
                {
                    FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, dds, pngFileName, FREE_IMAGE_SAVE_FLAGS.PNG_Z_NO_COMPRESSION);
                }
                catch (Exception) { }
                dds.SetNull();
                ClearMemory();
            }
            if (File.Exists(pngFileName))
            {
                var uri = new Uri(pngFileName);
                var bitmapImage = new BitmapImage(uri);

                int width = bitmapImage.PixelWidth;
                int height = bitmapImage.PixelHeight;

                int stride = width * 4; 
                var pixelData = new byte[stride * height];
                bitmapImage.CopyPixels(pixelData, stride, 0);

                //Convert image to 96 dpi
                return BitmapSource.Create(width, height, 96, 96,
                                            PixelFormats.Bgra32,
                                            bitmapImage.Palette,
                                            pixelData, stride);
            }
            using (var memory = new MemoryStream())
            {
                Properties.Resources.errorpic.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }


        public static ImageSource FrameImage(string fileName, double targetWidth, double targetHeight, int framesCount)
        {
            if (string.IsNullOrEmpty(fileName)) return new BitmapImage();
            if (targetWidth == 0 || targetHeight == 0) return new BitmapImage();
            GetImageSourceFromFileName(fileName);
            var pngFileName = fileName.Replace(".dds", ".png").
                Replace(".DDS", ".png").
                Replace(".tga", ".png").
                Replace(".jpg", ".png").
                Replace(".TGA", ".png");
            if (!File.Exists(pngFileName)) return new BitmapImage();
            var sourceImage = Image.FromFile(pngFileName);
            var targetImage = new BitmapImage();
            var img = new Bitmap((int)targetWidth, (int)targetHeight);

            using (var g = Graphics.FromImage(img))
            {
                var srcFrame = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height / framesCount);
                var destFrame = new Rectangle(0, 0, (int)targetWidth, (int)targetHeight);
                g.DrawImage(sourceImage, destFrame, srcFrame, GraphicsUnit.Pixel);
            }

            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                targetImage.BeginInit();
                targetImage.StreamSource = ms;
                targetImage.CacheOption = BitmapCacheOption.OnLoad;
                targetImage.EndInit();
                ms.Close();
            }
            return targetImage;
        }

        public static ImageSource TrueStretchImage(string fileName, double targetWidth, double targetHeight)
        {
            if (string.IsNullOrEmpty(fileName)) return new BitmapImage();
            if (targetWidth == 0 || targetHeight == 0) return new BitmapImage();
            GetImageSourceFromFileName(fileName);
            var pngFileName = fileName.Replace(".dds", ".png").
                Replace(".DDS", ".png").
                Replace(".tga", ".png").
                Replace(".jpg", ".png").
                Replace(".TGA", ".png");
            if (!File.Exists(pngFileName)) return new BitmapImage();
            var sourceImage = Image.FromFile(pngFileName);
            var targetImage = new BitmapImage();

            var img = new Bitmap((int)targetWidth, (int)targetHeight);
            using (var g = Graphics.FromImage(img))
            {
                var backgroundImage = new Bitmap(1, 1);
                using (var tg = Graphics.FromImage(backgroundImage))
                {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, 1, 1),
                        new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, 1, 1),
                        GraphicsUnit.Pixel
                        );
                }


                var topLeftAngle = new Rectangle(0, 0, sourceImage.Width / 2, sourceImage.Height / 2 + 1);
                var topRightAngle = new Rectangle(sourceImage.Width / 2, 0, sourceImage.Width / 2, sourceImage.Height / 2);
                var bottomLeftAngle = new Rectangle(0, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);
                var bottomRighttAngle = new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);
                var dstTopLeftAngle = topLeftAngle;
                var dstTopRightAngle = new Rectangle((int)targetWidth - sourceImage.Width / 2, 0, sourceImage.Width / 2, sourceImage.Height / 2);
                var dstBottomLeftAngle = new Rectangle(0, (int)targetHeight - sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);
                var dstBottomRightAngle = new Rectangle((int)targetWidth - sourceImage.Width / 2, (int)targetHeight - sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);

                g.DrawImage(sourceImage, dstTopLeftAngle, topLeftAngle, GraphicsUnit.Pixel);
                g.DrawImage(sourceImage, dstTopRightAngle, topRightAngle, GraphicsUnit.Pixel);
                g.DrawImage(sourceImage, dstBottomLeftAngle, bottomLeftAngle, GraphicsUnit.Pixel);
                g.DrawImage(sourceImage, dstBottomRightAngle, bottomRighttAngle, GraphicsUnit.Pixel);

                var bgWidth = targetWidth - dstTopLeftAngle.Width - dstTopRightAngle.Width;
                var bgHeight = targetHeight - dstTopLeftAngle.Height - dstBottomLeftAngle.Height;
                g.FillRectangle(new TextureBrush(backgroundImage), new Rectangle(dstTopLeftAngle.Width, dstTopLeftAngle.Height - 1, (int)bgWidth, (int)bgHeight + 1));


                var topTextureImg = new Bitmap(1, sourceImage.Height == 1 ? 1 : sourceImage.Height / 2);
                using (var tg = Graphics.FromImage(topTextureImg))
                {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, 1, sourceImage.Height / 2),
                        new Rectangle(sourceImage.Width / 2, 0, sourceImage.Width / 2 + sourceImage.Width % 2, sourceImage.Height / 2),
                        GraphicsUnit.Pixel
                        );
                }
                var topBrush = new TextureBrush(topTextureImg);
                var topBorderRect = new Rectangle(sourceImage.Width / 2, 0, (int)targetWidth - sourceImage.Width + sourceImage.Width % 2, sourceImage.Height / 2);
                g.FillRectangle(topBrush, topBorderRect);

                var bottomTextureImg = new Bitmap(1, sourceImage.Height == 1 ? 1 : sourceImage.Height / 2);
                using (var tg = Graphics.FromImage(bottomTextureImg))
                {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, 1, sourceImage.Height / 2),
                        new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, 1, sourceImage.Height / 2),
                        GraphicsUnit.Pixel
                        );
                }
                for (var x = sourceImage.Width / 2; x < (int)targetWidth - sourceImage.Width / 2; x++)
                {
                    var bottomBorderRect = new Rectangle(x, (int)targetHeight - sourceImage.Height / 2, 1, sourceImage.Height / 2);
                    g.DrawImage(bottomTextureImg, bottomBorderRect);
                }

                var leftTextureImg = new Bitmap(sourceImage.Width == 1 ? 1 : sourceImage.Width / 2, 1);
                using (var tg = Graphics.FromImage(leftTextureImg))
                {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, sourceImage.Width / 2, 1),
                        new Rectangle(0, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2 + 1),
                        GraphicsUnit.Pixel
                        );
                }
                var leftBrush = new TextureBrush(leftTextureImg);
                var leftBorderRect = new Rectangle(0, sourceImage.Height / 2 + 1, sourceImage.Width / 2, (int)targetHeight - sourceImage.Height - 1 + sourceImage.Height % 2);
                g.FillRectangle(leftBrush, leftBorderRect);

                var rightTextureImg = new Bitmap(sourceImage.Width == 1 ? 1 : sourceImage.Width / 2, 1);
                using (var tg = Graphics.FromImage(rightTextureImg))
                {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, sourceImage.Width / 2, 1),
                        new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2),
                        GraphicsUnit.Pixel
                        );
                }
                for (var y = sourceImage.Height / 2; y < (int)targetHeight - sourceImage.Height / 2; y++)
                {
                    var rightBorderRect = new Rectangle((int)targetWidth - sourceImage.Width / 2, y, sourceImage.Width / 2, 1);
                    g.DrawImage(rightTextureImg, rightBorderRect);
                }
            }
            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                targetImage.BeginInit();
                targetImage.StreamSource = ms;
                targetImage.CacheOption = BitmapCacheOption.OnLoad;
                targetImage.EndInit();
                ms.Close();
            }
            return targetImage;
        }

        public static SolidColorBrush GetColorBrushFromString(string color)
        {
            if (color == null) return new SolidColorBrush(Colors.White);
            var ret = new SolidColorBrush();
            var dyes = color.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            byte alfa, r, g, b;
            if (dyes.Count<string>() == 4)
            {
                Byte.TryParse(dyes[3], NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out alfa);
                Byte.TryParse(dyes[0], NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out r);
                Byte.TryParse(dyes[1], NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out g);
                Byte.TryParse(dyes[2], NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out b);
                ret.Color = dyes.Length == 4
                    ? Color.FromArgb(alfa, r, g, b)
                    : Colors.White;
                return ret;
            }
            else
            {
                return System.Windows.Media.Brushes.White;
            }
        }
    }
}
