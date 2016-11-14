using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
    public class ColorConvert
    {
        public byte[,] ToGrayscale(Bitmap bitmap)
        {
            var result = new byte[bitmap.Width, bitmap.Height];
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var grayColor = ToGrayscaleColor(bitmap.GetPixel(x, y));
                    result[x, y] = (byte)grayColor;
                }
            return result;
        }

        private byte ToGrayscaleColor(System.Drawing.Color color)
        {
            var level = (byte)((color.R + color.G + color.B) / 3);
            return level;
        }

    }
}
