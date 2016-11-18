using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLImgViewer
{
    public class GrayGradation : IGradation
    {
        public System.Drawing.Color[] GetGradation(int count, bool inverse)
        {
            System.Drawing.Color[] result = new System.Drawing.Color[count];
            int index = 0;
            for (double i = 0; i < 0.8; i += 0.8 / (double)(count))
            {
                if (inverse)
                {
                    result[result.Length - index - 1] = GrayToRgb(i);
                }
                else
                {
                    result[index] = GrayToRgb(i);
                }
                index++;

            }
            return result;
        }

        public System.Windows.Media.Color[] GetGradationWPF(int count, bool inverse)
        {
            System.Windows.Media.Color[] result = new System.Windows.Media.Color[count];
            int index = 0;
            for (double i = 0; i < 0.8; i += 0.8 / (double)(count))
            {

                var color = GrayToRgb(i);
                if (inverse)
                {
                    result[result.Length - index - 1] = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
                }
                else
                {
                    result[index] = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
                }
                index++;

            }
            return result;
        }

        public System.Drawing.Color GrayToRgb(double h)
        {
            System.Drawing.Color rgb = System.Drawing.Color.FromArgb(
                    Convert.ToByte(h * 255.0f),
                    Convert.ToByte(h * 255.0f),
                    Convert.ToByte(h * 255.0f));
            return rgb;
        }
    }
}
