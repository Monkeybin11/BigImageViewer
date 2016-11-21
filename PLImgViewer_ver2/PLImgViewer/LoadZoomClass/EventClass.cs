using Accord.Imaging.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Accord.Math;
using System.Drawing;

namespace PLImgViewer
{
    public class EventClass
    {
        public StackPanel Btn;
        public System.Windows.Controls.Image Img;
        public int Rownum;
        public int Colnum;
        public string[,] ImgPathBox;
        public double Scale;
        public BitmapSource OriginalImg;
        public BitmapSource RainbowImg;
        public BitmapSource HSVImg;

        public EventClass(StackPanel btn , System.Windows.Controls.Image img , int row, int col, string[,] pathbox,double scale)
        {
            Btn = btn;
            Rownum = row;
            Colnum = col;
            Img = img;
            ImgPathBox = pathbox;
            Scale = scale;
        }

        public void DropEventMethod(object ss, DragEventArgs ee)
        {
            string[] files = (string[])ee.Data.GetData(DataFormats.FileDrop);
            ImgPathBox[Rownum, Colnum] = files[0];

            FormatConvert fcv           = new FormatConvert();
            MatrixToImage Matrix2Bitmap = new MatrixToImage();

            byte[,] byteMatrix          = fcv.Dat2DownMat(ImgPathBox[Rownum, Colnum], ImgInfo.WH, Scale);
            BitmapSource result         = CreateBitmapSourceClass.CreateBitmapSource( byteMatrix );

            Img.Source  = result;
            OriginalImg = result;

            Task.Run( () => RainbowImg = CreateColoredImg( byteMatrix , byteMatrix.GetLength( 1 ) , byteMatrix.GetLength( 0 ) , ColorCovMode.Rainbow));
            Task.Run( () => HSVImg = CreateColoredImg( byteMatrix , byteMatrix.GetLength( 1 ) , byteMatrix.GetLength( 0 ) , ColorCovMode.HSV));
        }

        BitmapSource CreateColoredImg( byte[,] byteMatrix ,int width, int height , ColorCovMode colormod)
        {
            byte[] flatMatrix = byteMatrix.Flatten<byte>();
            Color[] rainbowArr = ConvertColor( colormod )( flatMatrix );
            ArrayToImage convertor = new ArrayToImage(width,height);
            System.Drawing.Bitmap imgbit= new System.Drawing.Bitmap(width,height);
            convertor.Convert( rainbowArr , out imgbit );
            return CreateBitmapSourceClass.ToWpfBitmap( imgbit );
        }

        Func<byte[] , Color[]> ConvertColor( ColorCovMode colorMode )
        {
            Func<byte[],Color[]> convertmethod = input => ColorConvertMethod(colorMode , input);
            return convertmethod;
        }
        Color[] ColorConvertMethod( ColorCovMode colorMode , byte[] input )
        {
            Color[] output;
            switch ( colorMode )
            {
                case ColorCovMode.None:
                    break;

                case ColorCovMode.Rainbow:
                    RainbowGradation colormap = new RainbowGradation();
                    System.Drawing.Color[] clrmap = colormap.GetGradation( 255 , false );
                    output = new Color[input.GetLength( 0 )];
                    for ( int i = 0 ; i < input.GetLength( 0 ) ; i++ )
                    {
                        output[i] = clrmap[input[i]];
                    }
                    return output;
            }
            return null;
        }



    }
}
