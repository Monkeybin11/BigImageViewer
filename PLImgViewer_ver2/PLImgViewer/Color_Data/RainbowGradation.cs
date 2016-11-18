using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLImgViewer
{
    public class RainbowGradation : IGradation
    {
        public System.Drawing.Color[] GetGradation(int count, bool inverse)
        {
            System.Drawing.Color[] result = new System.Drawing.Color[count];
            int index = 0;
            for (double i = 0; i < 0.8; i += 0.8 / (double)(count))
            {
                if (inverse)
                {
                    result[result.Length - index - 1] = WavelengthToRGB(i * 350 + 400);
                }
                else
                {
                    result[index] = WavelengthToRGB(i * 350 + 400);
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

                var color = WavelengthToRGB(i * 350 + 400);
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

        public System.Drawing.Color WavelengthToRGB(double wavelength)
        {
            double r, g, b;
            double sss, gamma;

            gamma = 0.45;    // Bruton had 0.80 - Poynton says 0.45

            if (wavelength < 380) wavelength = 380;
            if (wavelength > 780) wavelength = 780;

            if ((wavelength >= 380) && (wavelength < 440))
            {
                r = -1.0 * (wavelength - 440.0) / (440.0 - 380.0);
                g = 0;
                b = 1.0;
            }
            else if ((wavelength >= 440) && (wavelength < 490))
            {
                r = 0;
                g = (wavelength - 440.0) / (490.0 - 440.0);
                b = 1.0;
            }
            else if ((wavelength >= 490) && (wavelength < 510))
            {
                r = 0;
                g = 1.0;
                b = -1.0 * (wavelength - 510.0) / (510.0 - 490.0);
            }
            else if ((wavelength >= 510) && (wavelength < 580))
            {
                r = (wavelength - 510.0) / (580.0 - 510.0);
                g = 1.0;
                b = 0;
            }
            else if ((wavelength >= 580) && (wavelength < 645))
            {
                r = 1.0;
                g = -1.0 * (wavelength - 645.0) / (645.0 - 580.0);
                b = 0;
            }
            else
            {
                r = 1.0;
                g = 0;
                b = 0;
            }

            if (wavelength > 700)
            {
                sss = 0.3 + 0.7 * (780.0 - wavelength) / (780.0 - 700.0);
            }
            else if (wavelength < 420)
            {
                sss = 0.3 + 0.7 * (wavelength - 380.0) / (420.0 - 380.0);
            }
            else
            {
                sss = 1.0;
            }

            r = Math.Pow(sss * r, gamma);
            g = Math.Pow(sss * g, gamma);
            b = Math.Pow(sss * b, gamma);

            System.Drawing.Color clr = System.Drawing.Color.FromArgb(255, (byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));

            return clr;
        }


    }
}
