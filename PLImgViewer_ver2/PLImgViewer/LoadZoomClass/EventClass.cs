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
using System.IO;

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

		public void SaveImg()
		{
			string dirpath = Path.GetDirectoryName( ImgPathBox[Rownum,Colnum]);
			string name = Path.GetFileNameWithoutExtension( ImgPathBox[Rownum,Colnum]);
			string filepath = dirpath + "\\" +Rownum.ToString() +"_"+ Colnum.ToString() +"_"+ name +".bmp";

			using ( var fs = new FileStream( filepath , FileMode.Create ) )
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add( BitmapFrame.Create( OriginalImg ) );
				enc.Save( fs );
			}
		}


        public void DropEventMethod(object ss, DragEventArgs ee)
        {
            string[] files = (string[])ee.Data.GetData(DataFormats.FileDrop);
            ImgPathBox[Rownum, Colnum] = files[0];
			AfterSetImage();
		}

		public void SetFile(string fpath)
		{
			ImgPathBox [ Rownum , Colnum ] = fpath;
			AfterSetImage();
		}

		Action AfterSetImage =>
		() =>
		{
			FormatConvert fcv           = new FormatConvert();
			MatrixToImage Matrix2Bitmap = new MatrixToImage();

			//byte[,] byteMatrix          = fcv.Dat2MatEMD(ImgPathBox[Rownum, Colnum], ImgInfo.WH, Scale);
			byte[] bytelist             = fcv.SimpleReadByte(ImgPathBox[Rownum, Colnum], ImgInfo.WH, Scale);

			BitmapSource result         = CreateBitmapSourceClass.CreateBitmapSource(bytelist ,ImgInfo.W,ImgInfo.H );

			Img.Source = result;
			OriginalImg = result;

			Task.Run( () => RainbowImg = CreateColoredImgVector( bytelist , ImgInfo.W , ImgInfo.H , ColorCovMode.Rainbow ) );
			Task.Run( () => HSVImg = CreateColoredImgVector( bytelist , ImgInfo.W , ImgInfo.H , ColorCovMode.HSV ) );
		};


        BitmapSource CreateColoredImg( byte[,] byteMatrix ,int width, int height , ColorCovMode colormod)
        {
            ColorConvertMethod cv = new ColorConvertMethod();
            byte[] flatMatrix = byteMatrix.Flatten<byte>();
            Color[] colorArr = cv.ConvertColor( colormod )( flatMatrix );
            ArrayToImage convertor = new ArrayToImage(width,height);
            System.Drawing.Bitmap imgbit= new System.Drawing.Bitmap(width,height);
            convertor.Convert( colorArr , out imgbit );
            return CreateBitmapSourceClass.ToWpfBitmap( imgbit );
        }

		BitmapSource CreateColoredImgVector( byte [] byteMatrix , int width , int height , ColorCovMode colormod )
		{
			ColorConvertMethod cv = new ColorConvertMethod();
			byte[] flatMatrix = byteMatrix;
			Color[] colorArr = cv.ConvertColor( colormod )( flatMatrix );
			ArrayToImage convertor = new ArrayToImage(width,height);
			System.Drawing.Bitmap imgbit= new System.Drawing.Bitmap(width,height);
			convertor.Convert( colorArr , out imgbit );
			return CreateBitmapSourceClass.ToWpfBitmap( imgbit );
		}


	}
}
