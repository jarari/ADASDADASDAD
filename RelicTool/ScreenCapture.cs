using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RelicTool {
    public class ScreenCapture {
        private static int count = 0;
        private static int sequence = 10;
        private static double maxfactor = (double)1 / 4;
        private static double minfactor = (double)1 / 12;
        public static void SaveScreenshot(IntPtr hwnd, Stream stream) {
            var rect = new User32.Rect();
            User32.GetWindowRect(hwnd, ref rect);

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            if (width <= 0 || height <= 0)
                return;

            Bitmap bmp;
            if (MainBehavior.RewardScreen) {
                bmp = new Bitmap(width, (int)(height / 8f), PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(rect.left, rect.top + (int)(height / 8f * 3f), 0, 0, new Size(width, (int)(height / 8f)), CopyPixelOperation.SourceCopy);
                //bmp = ReplaceColor(bmp, Color.FromArgb(255, 96, 73, 58), 40, 40, 40, Color.FromArgb(255, 160, 141, 90));
                Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                bmp = Sharpen(bmp, minfactor + (maxfactor - minfactor) / (double)sequence * (double)count);
                //bmp.Save(Application.StartupPath + "\\test.png", ImageFormat.Png);
            }
            else{
                bmp = new Bitmap(width, (int)(height / 4f), PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, (int)(height / 4f)), CopyPixelOperation.SourceCopy);
            }

            bmp.Save(stream, ImageFormat.Png);

            bmp.Dispose();

            count++;
            if (count > sequence)
                count = 0;
        }

        private class User32 {
            [StructLayout(LayoutKind.Sequential)]
            public struct Rect {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
        }
        /*
        private static bool IsInRange(Color c1, Color c2, int rangeR, int rangeG, int rangeB) {
            return ((Math.Abs(c1.R - c2.R)) <= rangeR / 2 && (Math.Abs(c1.G - c2.G)) <= rangeG / 2 && (Math.Abs(c1.B - c2.B)) <= rangeB / 2);
        }

        static unsafe Bitmap ReplaceColor(Bitmap source,
                                  Color toReplace,
                                  int rangeR,
                                  int rangeG,
                                  int rangeB,
                                  Color replacement) {
            const int pixelSize = 4; // 32 bits per pixel

            Bitmap target = new Bitmap(
              source.Width,
              source.Height,
              PixelFormat.Format32bppArgb);

            BitmapData sourceData = null, targetData = null;

            try {
                sourceData = source.LockBits(
                  new Rectangle(0, 0, source.Width, source.Height),
                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                targetData = target.LockBits(
                  new Rectangle(0, 0, target.Width, target.Height),
                  ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                for (int y = 0; y < source.Height; ++y) {
                    byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                    byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                    for (int x = 0; x < source.Width; ++x) {
                        byte b = sourceRow[x * pixelSize + 0];
                        byte g = sourceRow[x * pixelSize + 1];
                        byte r = sourceRow[x * pixelSize + 2];
                        byte a = sourceRow[x * pixelSize + 3];

                        if (IsInRange(toReplace, Color.FromArgb(a, r, g, b), rangeR, rangeG, rangeB)) {
                            r = replacement.R;
                            g = replacement.G;
                            b = replacement.B;
                        }

                        targetRow[x * pixelSize + 0] = b;
                        targetRow[x * pixelSize + 1] = g;
                        targetRow[x * pixelSize + 2] = r;
                        targetRow[x * pixelSize + 3] = a;
                    }
                }
            }
            finally {
                if (sourceData != null)
                    source.UnlockBits(sourceData);

                if (targetData != null)
                    target.UnlockBits(targetData);
            }

            return target;
        }*/

        //Implemented from https://stackoverflow.com/questions/903632/sharpen-on-a-bitmap-using-c-sharp
        //Original code by Daniel Brückner, edited by niaher and David Johnson.
        //Modified to produce better results for OCR.
        public static Bitmap Sharpen(Bitmap image, double factor) {
            Bitmap sharpenImage = (Bitmap)image.Clone();

            int width = image.Width;
            int height = image.Height;

            // Create sharpening filter.
            const int filterWidth = 5;
            const int filterHeight = 5;

            double[,] filter = new double[filterWidth, filterHeight] {
                { -1, -1, -1, -1, -1 },
                { -1,  1,  1,  1, -1 },
                { -1,  1,  16,  1, -1 },
                { -1,  1,  1,  1, -1 },
                { -1, -1, -1, -1, -1 }
            };
            double bias = 0.0;

            Color[,] result = new Color[image.Width, image.Height];

            // Lock image bits for read/write.
            BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Declare an array to hold the bytes of the bitmap.
            int bytes = pbits.Stride * height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

            int rgb;
            // Fill the color array with the new sharpened color values.
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < filterWidth; filterX++) {
                        for (int filterY = 0; filterY < filterHeight; filterY++) {
                            int imageX = (x - filterWidth / 2 + filterX + width) % width;
                            int imageY = (y - filterHeight / 2 + filterY + height) % height;

                            rgb = imageY * pbits.Stride + 3 * imageX;

                            red += rgbValues[rgb + 2] * filter[filterX, filterY];
                            green += rgbValues[rgb + 1] * filter[filterX, filterY];
                            blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }

            // Update the image with the sharpened pixels.
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    rgb = y * pbits.Stride + 3 * x;

                    rgbValues[rgb + 2] = result[x, y].R;
                    rgbValues[rgb + 1] = result[x, y].G;
                    rgbValues[rgb + 0] = result[x, y].B;
                }
            }

            // Copy the RGB values back to the bitmap.
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
            // Release image bits.
            sharpenImage.UnlockBits(pbits);

            return sharpenImage;
        }
    }
}
