using Accord.Imaging.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PLImgViewer
{
    public static class MakeBitmapSRC
    {
        public static BitmapSource CreateBitmapSource(byte[,] input)
        {
            try
            {
                MatrixToImage Matrix2Bitmap = new MatrixToImage();
                Bitmap bitout = new Bitmap(input.GetLength(0), input.GetLength(1));
                Matrix2Bitmap.Convert(input, out bitout);
                BitmapSource bitsource = ToWpfBitmap(bitout);
                return bitsource;
            }
            catch (Exception)
            {
                return null;
            }
        }
        static BitmapSource ToWpfBitmap(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Bmp);
                    stream.Position = 0;
                    BitmapImage result = new BitmapImage();
                    result.BeginInit();
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.StreamSource = stream;
                    result.EndInit();
                    result.Freeze();
                    return result;
                }
            }
            catch (Exception)
            {
                BitmapImage result = new BitmapImage();
                return result;
            }
        }
    }
}
