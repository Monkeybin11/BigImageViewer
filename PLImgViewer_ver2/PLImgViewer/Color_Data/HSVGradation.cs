using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLImgViewer
{
    public class HSVGradation : IGradation
    {
        public System.Drawing.Color[] GetGradation(int count, bool inverse)
        {
            System.Drawing.Color[] result = new System.Drawing.Color[count];
            int index = 0;
            for (double i = 0; i < 0.8; i += 0.8 / (double)(count))
            {
                if (inverse)
                {
                    result[result.Length - index - 1] = HSL2RGB(i, 0.5, 0.5);
                }
                else
                {
                    result[index] = HSL2RGB(i, 0.5, 0.5);
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

                var color = HSL2RGB(i, 0.5, 0.5);
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

        // Given H,S,L in range of 0-1
        // Returns a Color (RGB struct) in range of 0-255
        public System.Drawing.Color HSL2RGB(double h, double sl, double l)
        {
            double v;
            double r, g, b;

            r = l;   // default to gray
            g = l;
            b = l;
            v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = l + l - v;
                sv = (v - m) / v;
                h *= 6.0;
                sextant = (int)h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            System.Drawing.Color rgb = System.Drawing.Color.FromArgb(
                    Convert.ToByte(r * 255.0f),
                    Convert.ToByte(g * 255.0f),
                    Convert.ToByte(b * 255.0f));
            return rgb;
        }
    }
}
