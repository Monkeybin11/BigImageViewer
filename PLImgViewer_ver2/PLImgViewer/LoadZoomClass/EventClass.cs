using Accord.Imaging.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PLImgViewer
{
    public class EventClass
    {
        public StackPanel Btn;
        public Image Img;
        public int Rownum;
        public int Colnum;
        public string[,] ImgPathBox;
        public double Scale;

        public EventClass(StackPanel btn, Image img, int row, int col, string[,] pathbox,double scale)
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
            FormatConvert fcv = new FormatConvert();
            MatrixToImage Matrix2Bitmap = new MatrixToImage();
            byte[,] byteMatrix = fcv.Dat2DownMat(ImgPathBox[Rownum, Colnum], ImgInfo.WH, Scale);
            Img.Source = CreateBitmapSourceClass.CreateBitmapSource(byteMatrix);
        }
    }
}
