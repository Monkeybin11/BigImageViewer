using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
    public class ColorConvertMethod
    {
        public Func<byte[] , Color[]> ConvertColor( ColorCovMode colorMode )
        {
            ColorConvertMethod cv = new PLImgViewer.ColorConvertMethod();
            Func<byte[],Color[]> convertmethod = input => cv.ColorConvert(colorMode , input);
            return convertmethod;
        }

        public Color[] ColorConvert( ColorCovMode colorMode , byte[] input )
        {
            Color[] output;
            switch ( colorMode )
            {
                case ColorCovMode.Gray:
                    break;

                case ColorCovMode.Rainbow:
                    RainbowGradation colormaprain = new RainbowGradation();
                    System.Drawing.Color[] clrmaprain = colormaprain.GetGradation( 255 , false );
                    output = new Color[input.GetLength( 0 )];
                    for ( int i = 0 ; i < input.GetLength( 0 ) ; i++ )
                    {
                        output[i] = clrmaprain[input[i] == 255 ? 254 : input[i]];
                    }
                    return output;
                case ColorCovMode.HSV:
                    HSVGradation colormaphsv = new HSVGradation();
                    System.Drawing.Color[] clrmaphsv = colormaphsv.GetGradation( 255 , false );
                    output = new Color[input.GetLength( 0 )];
                    for ( int i = 0 ; i < input.GetLength( 0 ) ; i++ )
                    {
                        output[i] = clrmaphsv[ input [ i ] == 255 ? 254 : input [ i ] ];
                    }
                    return output;
            }
            return null;
        }
    }
}
