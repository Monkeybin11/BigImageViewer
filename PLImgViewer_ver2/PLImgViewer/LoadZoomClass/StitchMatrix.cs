using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using System.Drawing;

namespace PLImgViewer
{
    public class StitchMatrix
    {
        Accord.Imaging.Converters.MatrixToImage Matrix2Bitmap;
        Accord.Imaging.Converters.ImageToMatrix Bitmap2matrix;

        public byte[,] StitchArr(List<List<byte[,]>> input)
        {
            byte[,] origin = StitchLine2Line(StitchOneLine(input));
            return origin;
        }

        public byte[,] StitchDownImage(List<List<byte[,]>> input)
        {
            byte[,] origin = StitchLine2Line(
                StitchOneLine(
                    BitList2Mat(
                        Byte2BitList(input))));
            return origin;
        }



        List<byte[,]> StitchOneLine(List<List<byte[,]>> input) // Not Trans,intput is Trans
        {
            List<byte[,]> output = new List<byte[,]>();
            int i = 0;
            foreach (var item in input)
            {
                byte[,] sumVec = new byte[,] { };
                foreach (var subitem in item)
                {
                    sumVec = Matrix.Concatenate<byte>(sumVec, subitem);
                    i++;
                }
                sumVec = Matrix.Transpose(sumVec);
                output.Add(sumVec);
            }
            return output;
        }

        byte[,] StitchLine2Line(List<byte[,]> input)
        {
            try
            {
                byte[,] output = input[0];
                for (int i = 1; i < input.Count; i++)
                {
                    output = Matrix.Concatenate(output, input[i]);
                }
                return output;

            }
            catch (Exception eex)
            {
                Console.WriteLine(eex.ToString());
                return new byte[,] { };
            }
        }
    

        List<List<Bitmap>> Byte2BitList(List<List<byte[,]>> input)
        {
            List<List<Bitmap>> output = new List<List<Bitmap>>();
            for (int i = 0; i < input.Count; i++)
            {
                List<Bitmap> temp = new List<Bitmap>();
                for (int j = 0; j < input[i].Count; j++)
                {
                    Matrix2Bitmap = new Accord.Imaging.Converters.MatrixToImage();
                    Bitmap bitout = new Bitmap(input[i][j].GetLength(0), input[i][j].GetLength(1));
                    Matrix2Bitmap.Convert(input[i][j], out bitout);
                    Bitmap downbit = new Bitmap(bitout, new Size(input[i][j].GetLength(0) / 10, input[i][j].GetLength(1) / 10));
                    temp.Add(downbit);
                }
                output.Add(temp);
            }
            return output;
        }
        List<List<byte[,]>> BitList2Mat(List<List<Bitmap>> input)
        {
            Bitmap2matrix = new Accord.Imaging.Converters.ImageToMatrix();
            List<List<byte[,]>> output = new List<List<byte[,]>>();
            for (int i = 0; i < input.Count; i++)
            {
                List<byte[,]> temp = new List<byte[,]>();
                for (int j = 0; j < input[i].Count; j++)
                {
                    byte[,] outMat = new byte[input[i][j].Width, input[i][j].Height];
                    Bitmap2matrix.Convert(input[i][j], out outMat);
                    temp.Add(outMat);
                }
                output.Add(temp);
            }
            return output;
        }
    }
}
