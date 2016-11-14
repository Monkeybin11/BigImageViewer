using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Imaging.Converters;
using System.Drawing;
using System.Drawing.Imaging;
using Accord.Math;
using System.Windows.Forms;

namespace PLImgViewer
{
    public class FormatConvert
    {
        MatrixToImage Matrix2Bitmap;
        ImageToMatrix Bitmap2matrix;
        public byte[] Dat2Vec(string path,int length)
        {
            Stream loadstream = new FileStream(path, FileMode.Open);
            byte[] oneshotVector = new byte[length];
            loadstream.Read(oneshotVector, 0, oneshotVector.Length);
            loadstream.Dispose();
            return oneshotVector;
        }

        public byte[,] Dat2Mat(string path)
        {
            try
            {
                Matrix2Bitmap = new MatrixToImage();
                Bitmap2matrix = new ImageToMatrix();

                Stream loadstream = new FileStream(path, FileMode.Open);
                byte[] oneshotVector = new byte[ImgInfo.WH];
                loadstream.Read(oneshotVector, 0, oneshotVector.Length);
                loadstream.Dispose();
                byte[,] temp = Vec2Mat(oneshotVector, ImgInfo.W, ImgInfo.H); // ok
                oneshotVector = null;
                return temp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return new byte[0, 0];
            }
        }

        public byte[,] Dat2DownMat(string path, int length, double scale)
        {
            try
            {
                Matrix2Bitmap = new MatrixToImage();
                Bitmap2matrix = new ImageToMatrix();

                Stream loadstream = new FileStream(path, FileMode.Open);
                byte[] oneshotVector = new byte[length];
                loadstream.Read(oneshotVector, 0, oneshotVector.Length);
                loadstream.Dispose();
                byte[,] temp = Vec2Mat(oneshotVector, ImgInfo.W, ImgInfo.H); // ok
                oneshotVector = null;

                Bitmap tempbit = new Bitmap(ImgInfo.W, ImgInfo.H, PixelFormat.Format4bppIndexed);
                Matrix2Bitmap.Convert(temp, out tempbit);
                temp = null;
                Bitmap tempdown = new Bitmap(tempbit, new Size((int)(ImgInfo.W / scale), (int)(ImgInfo.H / scale)));
                tempbit = null;
                byte[,] arrdown = new byte[(int)(tempdown.Width / scale), (int)(tempdown.Height / scale)];
                Bitmap2matrix.Convert(tempdown, out arrdown);
                tempdown = null;
                //arrdown = Matrix.Transpose(arrdown);
                return arrdown;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return new byte[0, 0];
            }
        }

        public byte[,] Vec2Mat(byte[] input, int width, int height)
        {
            byte[,] oneShotArray = Accord.Math.Matrix.Reshape(input, width, height);
            return Accord.Math.Matrix.Transpose(oneShotArray);
        }

    }
}
