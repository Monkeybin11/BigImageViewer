using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Imaging.Converters;
using Accord.Math;

namespace PLImgViewer
{
    public class ZoomClass
    {
        MatrixToImage Matrix2Bitmap;
        ImageToMatrix Bitmap2matrix;

        /* input real pos of image*/
        /* 메인 시퀀스  */

        public List<List<byte[,]>> AsyListDownScaledZoomedMat(string[,] input, ZoomData data)
        {
            try
            {

                /* data에 들어있는 리얼 포지션 좌표 값은 스케일링이 안되있다. */
                /* 1. 각 원본 Dat 파일에 어느 좌표부터 어느 좌표까지 가져와야 하는지 계산 */
                /* 2. 데이터를 가져온다음에 리사이즈 */
                /**/
                //Check Start End File Pos and scale ===
                data.Scale = CalcScale(input, data);

                double scale = data.Scale; // 여기서 스케일이 정해진다. 이것은 실제 데이터 가져온후 이미지 -> 스케일변환 -> 
                Console.WriteLine("Scale Value is " + $"  {scale} ");
                /* Class Instance*/
                FormatConvert fcv = new FormatConvert();
                Matrix2Bitmap = new MatrixToImage();
                Bitmap2matrix = new ImageToMatrix();

                int width = data.Ex - data.Sx;
                int height = data.Ey - data.Sy;
                int WCount = data.endNumX - data.startNumX;
                int HCount = data.endNumY - data.startNumY;


                int strPosX = 0;
                int strPosY = 0;
                int endPosX = 0;
                int endPosY = 0;

                List<List<byte[,]>> box = new List<List<byte[,]>>();

                if ((data.endNumX - data.startNumX) == 0 && (data.endNumY - data.startNumY) == 0)
                {
                    /*OK*/
                    #region
                    List<byte[,]> tempbox = new List<byte[,]>();
                    string path = input[data.startNumY, data.startNumX];
                    strPosX = data.Sx - data.startNumX * data.Wo;
                    strPosY = data.Sy - data.startNumY * data.Ho;
                    endPosX = data.Ex - data.endNumX * data.Wo;
                    endPosY = data.Ey - data.endNumY * data.Ho;

                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    box.Add(tempbox);
                    #endregion
                }
                else if ((data.endNumY - data.startNumY) == 0)
                {
                    /*OK*/
                    #region
                    for (int i = data.startNumX; i <= data.endNumX; i++)
                    {
                        string path = input[i, data.startNumY];

                        List<byte[,]> tempbox = new List<byte[,]>();
                        if (i == data.startNumX)
                        {
                            strPosX = data.Sx - data.startNumX * data.Wo;
                            strPosY = data.Sy - data.startNumY * data.Ho;
                            endPosX = data.Wo;
                            endPosY = data.Ey - data.endNumY * data.Ho;
                            tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                        }
                        else if (i == data.endNumX)
                        {
                            strPosX = 0;
                            strPosY = data.Sy - data.startNumY * data.Ho;
                            endPosX = data.Ex - data.endNumX * data.Wo;
                            endPosY = data.Ey - data.endNumY * data.Ho;
                            tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                        }
                        else
                        {
                            strPosX = 0;
                            strPosY = data.Sy - data.startNumY * data.Ho;
                            endPosX = data.Wo;
                            endPosY = data.Ey - data.endNumY * data.Ho;
                            tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                        }
                        box.Add(tempbox);
                    }
                    #endregion
                }
                else if ((data.endNumX - data.startNumX) == 0)
                {
                    /*OK*/
                    #region
                    List<byte[,]> tempbox = new List<byte[,]>();
                    for (int j = data.startNumY; j <= data.endNumY; j++)
                    {
                        string path = input[j, data.startNumX];
                        if (j == data.startNumY)
                        {
                            strPosX = data.Sx - data.startNumX * data.Wo;
                            strPosY = data.Sy - data.startNumY * data.Ho;
                            endPosX = data.Ex - data.startNumX * data.Wo;
                            endPosY = data.Ho;
                            tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                        }
                        else if (j == data.endNumY)
                        {
                            strPosX = data.Sx - data.startNumX * data.Wo;
                            strPosY = 0;
                            endPosX = data.Ex - data.startNumX * data.Wo;
                            endPosY = data.Ey - data.endNumY * data.Ho;
                            tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                        }
                        else
                        {
                            strPosX = data.Sx - data.startNumX * data.Wo;
                            strPosY = 0;
                            endPosX = data.Ex - data.startNumX * data.Wo;
                            endPosY = data.Ho;
                            tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                        }
                    }
                    box.Add(tempbox);
                    #endregion
                }
                else
                {
                    for (int i = data.startNumX; i <= data.endNumX; i++)
                    {
                        List<byte[,]> tempbox = new List<byte[,]>();

                        /* i는 col 이고 j 는 row 이다. 
                         * 따라서 input [j,i]로 해야한다.  */
                        if (i == data.startNumX)
                        {
                            #region Start X Pos
                            for (int j = data.startNumY; j <= data.endNumY; j++)
                            {
                                string path = input[j, i];
                                if (j == data.startNumY)
                                {
                                    /* Pixel Position in One File Matrix*/
                                    strPosX = data.Sx - i * data.Wo;
                                    strPosY = data.Sy - j * data.Ho;
                                    endPosX = data.Wo;
                                    endPosY = data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                                else if (j == data.endNumY)
                                {
                                    strPosX = data.Sx - i * data.Wo;
                                    strPosY = 0;
                                    endPosX = data.Wo;
                                    endPosY = data.Ey - j * data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                                else
                                {
                                    strPosX = data.Sx - i * data.Wo;
                                    strPosY = 0;
                                    endPosX = data.Wo;
                                    endPosY = data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                            }
                            #endregion
                        }
                        else if (i == data.endNumX)
                        {
                            #region End X Pos
                            for (int j = data.startNumY; j <= data.endNumY; j++)
                            {
                                string path = input[j, i];
                                if (j == data.startNumY)
                                {
                                    strPosX = 0;
                                    strPosY = data.Sy - j * data.Ho;
                                    endPosX = data.Ex - i * data.Wo;
                                    endPosY = data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                                else if (j == data.endNumY)
                                {
                                    strPosX = 0;
                                    strPosY = 0;
                                    endPosX = data.Ex - i * data.Wo;
                                    endPosY = data.Ey - j * data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                                else
                                {
                                    strPosX = 0;
                                    strPosY = 0;
                                    endPosX = data.Ex - i * data.Wo;
                                    endPosY = data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region Middle
                            for (int j = data.startNumY; j <= data.endNumY; j++)
                            {
                                string path = input[j, i];
                                if (j == data.startNumY)
                                {
                                    strPosX = 0;
                                    strPosY = data.Sy - j * data.Ho;
                                    endPosX = data.Wo;
                                    endPosY = data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                                else if (j == data.endNumY)
                                {
                                    strPosX = 0;
                                    strPosY = 0;
                                    endPosX = data.Wo;
                                    endPosY = data.Ey - j * data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                                else
                                {
                                    strPosX = 0;
                                    strPosY = 0;
                                    endPosX = data.Wo;
                                    endPosY = data.Ho;
                                    tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                                }
                            }
                            #endregion
                        }
                        box.Add(tempbox);
                    }
                }
                //outputlist = box.ToList();
                return box;

            }
            catch (Exception eex)
            {
                Console.WriteLine(eex.ToString());
                return null;
                throw;
            }
        }
        public List<List<byte[,]>> ListDownScaledZoomedMat(string[,] input, ZoomData data)
        {
            /* data에 들어있는 리얼 포지션 좌표 값은 스케일링이 안되있다. */
            /* 1. 각 원본 Dat 파일에 어느 좌표부터 어느 좌표까지 가져와야 하는지 계산 */
            /* 2. 데이터를 가져온다음에 리사이즈 */
            /**/
            //Check Start End File Pos and scale ===
            data.Scale = CalcScale(input, data);

            double scale = data.Scale; // 여기서 스케일이 정해진다. 이것은 실제 데이터 가져온후 이미지 -> 스케일변환 -> 
            Console.WriteLine("Scale Value is " + $"  {scale} ");
            /* Class Instance*/
            FormatConvert fcv = new FormatConvert();
            Matrix2Bitmap     = new MatrixToImage();
            Bitmap2matrix     = new ImageToMatrix();

            int width  = data.Ex - data.Sx;
            int height = data.Ey - data.Sy;
            int WCount = data.endNumX - data.startNumX;
            int HCount = data.endNumY - data.startNumY;


            int strPosX = 0;
            int strPosY = 0;
            int endPosX = 0;
            int endPosY = 0;

            List<List<byte[,]>> box = new List<List<byte[,]>>();

            if ((data.endNumX - data.startNumX) == 0 && (data.endNumY - data.startNumY) == 0)
            {
                /*OK*/
                #region
                List<byte[,]> tempbox = new List<byte[,]>();
                string path = input[data.startNumY , data.startNumX ];
                strPosX = data.Sx - data.startNumX * data.Wo;
                strPosY = data.Sy - data.startNumY * data.Ho;
                endPosX = data.Ex - data.endNumX * data.Wo;
                endPosY = data.Ey - data.endNumY * data.Ho;

                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                box.Add(tempbox);
                #endregion
            }
            else if ((data.endNumY - data.startNumY) == 0)
            {
                /*OK*/
                #region
                for (int i = data.startNumX; i <= data.endNumX; i++)
                {
                    string path = input[i, data.startNumY];

                    List<byte[,]> tempbox = new List<byte[,]>();
                    if (i == data.startNumX)
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else if (i == data.endNumX)
                    {
                        strPosX = 0;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Ex - data.endNumX * data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else
                    {
                        strPosX = 0;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    box.Add(tempbox);
                }
                #endregion
            }
            else if ((data.endNumX - data.startNumX) == 0)
            {
                /*OK*/
                #region
                List<byte[,]> tempbox = new List<byte[,]>();
                for (int j = data.startNumY; j <= data.endNumY; j++)
                {
                    string path = input[j ,data.startNumX];
                    if (j == data.startNumY)
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Ex - data.startNumX * data.Wo;
                        endPosY = data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else if (j == data.endNumY)
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = 0;
                        endPosX = data.Ex - data.startNumX * data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = 0;
                        endPosX = data.Ex - data.startNumX * data.Wo;
                        endPosY = data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                }
                box.Add(tempbox);
                #endregion
            }
            else
            {
                for (int i = data.startNumX; i <= data.endNumX; i++)
                {
                    List<byte[,]> tempbox = new List<byte[,]>();

                    /* i는 col 이고 j 는 row 이다. 
                     * 따라서 input [j,i]로 해야한다.  */
                    if (i == data.startNumX)
                    {
                        #region Start X Pos
                        for (int j = data.startNumY; j <= data.endNumY; j++) 
                        {
                            string path = input[j, i];
                            if (j == data.startNumY)
                            {
                                /* Pixel Position in One File Matrix*/
                                strPosX = data.Sx - i * data.Wo;
                                strPosY = data.Sy - j * data.Ho;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else if (j == data.endNumY)
                            {
                                strPosX = data.Sx - i * data.Wo;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ey - j * data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else
                            {
                                strPosX = data.Sx - i * data.Wo;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                        }
                        #endregion
                    }
                    else if (i == data.endNumX)
                    {
                        #region End X Pos
                        for (int j = data.startNumY; j <= data.endNumY; j++)
                        {
                            string path = input[j, i];
                            if (j == data.startNumY)
                            {
                                strPosX = 0;
                                strPosY = data.Sy - j * data.Ho;
                                endPosX = data.Ex - i * data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else if (j == data.endNumY)
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Ex - i * data.Wo;
                                endPosY = data.Ey - j * data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Ex - i * data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                        }
                        #endregion  
                    }
                    else
                    {
                        #region Middle
                        for (int j = data.startNumY; j <= data.endNumY; j++)
                        {
                            string path = input[j, i];
                            if (j == data.startNumY)
                            {
                                strPosX = 0;
                                strPosY = data.Sy - j * data.Ho;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else if (j == data.endNumY)
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ey - j * data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                        }
                        #endregion
                    }
                    box.Add(tempbox);
                }
            }
            return box;
        }
        public List<List<byte[,]>> ListDownScaledZoomedMat(List<List<string>> input, ZoomData data)
        {
            /* data에 들어있는 리얼 포지션 좌표 값은 스케일링이 안되있다. */
            /* 1. 각 원본 DAt 파일에 어느 좌표부터 어느 좌표까지 가져와야 하는지 계산 */
            /* 2. 데이터를 가져온다음에 리사이즈 */
            /**/
            /**/
            /**/
            /**/

            //Check Start End File Pos and scale ===
            data.Scale = CalcScale(input, data);
            double scale = data.Scale; // 여기서 스케일이 정해진다. 이것은 실제 데이터 가져온후 이미지 -> 스케일변환 -> 
            Console.WriteLine("Scale Value is " + $"  {scale} ");
            /* Class Instance*/
            FormatConvert fcv = new FormatConvert();
            Matrix2Bitmap = new MatrixToImage();
            Bitmap2matrix = new ImageToMatrix();

            int width = data.Ex - data.Sx;
            int height = data.Ey - data.Sy;
            int WCount = data.endNumX - data.startNumX;
            int HCount = data.endNumY - data.startNumY;


            int strPosX = 0;
            int strPosY = 0;
            int endPosX = 0;
            int endPosY = 0;

            List<List<byte[,]>> box = new List<List<byte[,]>>();

            if ((data.endNumX - data.startNumX) == 0 && (data.endNumY - data.startNumY) == 0)
            {
                /*OK*/
                #region
                List<byte[,]> tempbox = new List<byte[,]>();
                string path = input[data.startNumX][data.startNumY];
                strPosX = data.Sx - data.startNumX * data.Wo;
                strPosY = data.Sy - data.startNumY * data.Ho;
                endPosX = data.Ex - data.endNumX * data.Wo;
                endPosY = data.Ey - data.endNumY * data.Ho;

                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                box.Add(tempbox);
                #endregion
            }
            else if ((data.endNumY - data.startNumY) == 0)
            {
                /*OK*/
                #region
                for (int i = data.startNumX; i <= data.endNumX; i++)
                {
                    string path = input[i][data.startNumY];

                    List<byte[,]> tempbox = new List<byte[,]>();
                    if (i == data.startNumX)
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else if (i == data.endNumX)
                    {
                        strPosX = 0;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Ex - data.endNumX * data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else
                    {
                        strPosX = 0;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    box.Add(tempbox);
                }
                #endregion
            }
            else if ((data.endNumX - data.startNumX) == 0)
            {
                /*OK*/
                #region
                List<byte[,]> tempbox = new List<byte[,]>();
                for (int j = data.startNumY; j <= data.endNumY; j++)
                {
                    string path = input[data.startNumX][j];
                    if (j == data.startNumY)
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = data.Sy - data.startNumY * data.Ho;
                        endPosX = data.Ex - data.startNumX * data.Wo;
                        endPosY = data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else if (j == data.endNumY)
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = 0;
                        endPosX = data.Ex - data.startNumX * data.Wo;
                        endPosY = data.Ey - data.endNumY * data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                    else
                    {
                        strPosX = data.Sx - data.startNumX * data.Wo;
                        strPosY = 0;
                        endPosX = data.Ex - data.startNumX * data.Wo;
                        endPosY = data.Ho;
                        tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                    }
                }
                box.Add(tempbox);
                #endregion
            }
            else
            {
                for (int i = data.startNumX; i <= data.endNumX; i++)
                {
                    List<byte[,]> tempbox = new List<byte[,]>();

                    if (i == data.startNumX)
                    {
                        #region Start X Pos
                        for (int j = data.startNumY; j <= data.endNumY; j++)
                        {
                            string path = input[i][j];
                            if (j == data.startNumY)
                            {
                                /* Pixel Position in One File Matrix*/
                                strPosX = data.Sx - i * data.Wo;
                                strPosY = data.Sy - j * data.Ho;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else if (j == data.endNumY)
                            {
                                strPosX = data.Sx - i * data.Wo;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ey - j * data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else
                            {
                                strPosX = data.Sx - i * data.Wo;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                        }
                        #endregion
                    }
                    else if (i == data.endNumX)
                    {
                        #region End X Pos
                        for (int j = data.startNumY; j <= data.endNumY; j++)
                        {
                            string path = input[i][j];
                            if (j == data.startNumY)
                            {
                                strPosX = 0;
                                strPosY = data.Sy - j * data.Ho;
                                endPosX = data.Ex - i * data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else if (j == data.endNumY)
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Ex - i * data.Wo;
                                endPosY = data.Ey - j * data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Ex - i * data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                        }
                        #endregion  
                    }
                    else
                    {
                        #region Middle
                        for (int j = data.startNumY; j <= data.endNumY; j++)
                        {
                            string path = input[i][j];
                            if (j == data.startNumY)
                            {
                                strPosX = 0;
                                strPosY = data.Sy - j * data.Ho;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else if (j == data.endNumY)
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ey - j * data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                            else
                            {
                                strPosX = 0;
                                strPosY = 0;
                                endPosX = data.Wo;
                                endPosY = data.Ho;
                                tempbox.Add(PutinListBox(path, fcv, strPosX, strPosY, endPosX, endPosY, scale));
                            }
                        }
                        #endregion
                    }
                    box.Add(tempbox);
                }
            }

            return box;
        }

        private double CalcScale(string[,] input, ZoomData data)
        {
            CheckStartPosition(input, data); // Start Num is 0

            int W = data.Ex - data.Sx;
            int H = data.Ey - data.Sy;

            /*
             길이가 문제가 아니라, 얼마만큼 많은 파일이 속에 포함되어 있냐가 문제다. 
             스타트 넘버와 엔드 넘버를 조합해서 스케일을 정하자. 
             */
            int temp = W * H;
            int Xcount = data.endNumX - data.startNumX + 1;
            int Ycount = data.endNumY - data.startNumY + 1;
            int total = Xcount * Ycount;

            if (total <= 1)
            { return 1.0; }
            else if (total < 2)
            { return 2.0; }
            else if (total < 3)
            { return 3.0; }
            else if (total < 4)
            { return 4.0; }
            else if (total < 5)
            { return 5.0; }
            else if (total < 6)
            { return 6.0; }
            else if (total < 7)
            { return 7.0; }
            else if (total < 8)
            { return 8.0; }
            else if (total < 9)
            { return 9.0; }
            else
            { return 10.0; }
            
        }
        private double CalcScale(List<List<string>> input, ZoomData data)
        {
            CheckStartPosition(input, data); // Start Num is 0

            int W = data.Ex - data.Sx;
            int H = data.Ey - data.Sy;

            /*
             길이가 문제가 아니라, 얼마만큼 많은 파일이 속에 포함되어 있냐가 문제다. 
             스타트 넘버와 엔드 넘버를 조합해서 스케일을 정하자. 

              검은 선이 생기는 문제가 있다. 
             */
            int temp = W * H;

            int Xcount = data.endNumX - data.startNumX + 1;
            int Ycount = data.endNumY - data.startNumY + 1;

            int total = Xcount * Ycount;

            Console.WriteLine("total Num is  " + $"{total}");

            if (total < 1)
            { return 1.0; }
            else if (total < 2)
            { return 2.0; }
            else if (total < 3)
            { return 3.0; }
            else if (total < 4)
            { return 4.0; }
            else if (total < 5)
            { return 5.0; }
            else if (total < 6)
            { return 6.0; }
            else if (total < 7)
            { return 7.0; }
            else if (total < 8)
            { return 8.0; }
            else if (total < 9)
            { return 9.0; }
            else
            { return 10.0; }
        }

        byte[,] SizeDown(byte[,] pickedMat, double scale)
        {
            Bitmap tempbit = new Bitmap(pickedMat.GetLength(0), pickedMat.GetLength(1));
            Matrix2Bitmap.Convert(pickedMat, out tempbit);
            pickedMat = null;

            Bitmap tempDownbut = new Bitmap(tempbit, new System.Drawing.Size((int)(tempbit.Width / scale), (int)(tempbit.Height / scale)));
            tempbit = null;

            byte[,] downMat = new byte[tempDownbut.Width, tempDownbut.Height];
            Bitmap2matrix.Convert(tempDownbut, out downMat);

            return downMat = Matrix.Transpose(downMat); // DownScaled byte[,]
        }

        void CheckStartPosition(string[,] input, ZoomData data)
        {
            for (int i = 0; i < input.GetLength(0); i++)
            {
                if (data.Sx < (i + 1) * data.Wo)
                {
                    data.startNumX = i;
                    break;
                }
            }

            for (int i = 0; i < input.GetLength(1); i++)
            {
                if (data.Sy < (i + 1) * data.Ho)
                {
                    data.startNumY = i;
                    break;
                }
            }

            for (int i = input.GetLength(0); i >= 0; i--)
            {
                if (data.Ex > i * data.Wo)
                {
                    data.endNumX = i;
                    break;
                }
            }

            for (int i = input.GetLength(1); i >= 0; i--)
            {
                if (data.Ey > i * data.Ho)
                {
                    data.endNumY = i;
                    break;
                }
            }
        }
        void CheckStartPosition(List<List<string>> input, ZoomData data)
        {
            for (int i = 0; i < input.Count; i++)
            {
                if (data.Sx < (i + 1) * data.Wo)
                {
                    data.startNumX = i;
                    break;
                }
            }

            for (int i = 0; i < input[0].Count; i++)
            {
                if (data.Sy < (i + 1) * data.Ho)
                {
                    data.startNumY = i;
                    break;
                }
            }

            for (int i = input.Count; i > 0; i--)
            {
                if (data.Ex > i * data.Wo)
                {
                    data.endNumX = i;
                    break;
                }
            }

            for (int i = input[0].Count; i > 0; i--)
            {
                if (data.Ey > i * data.Ho)
                {
                    data.endNumY = i;
                    break;
                }
            }
        }

        void SetZoomData(ZoomData data, int sx, int sy, int ex, int ey)
        {
            data.Sx = sx;
            data.Sy = sy;
            data.Ex = ex;
            data.Ey = ey;
            data.Wo = ex - sx;
            data.Ho = ey - ex;
        }

        byte[,] PickPixel(byte[,] input, int startX, int startY, int endX, int endY)
        {
            try
            {

                byte[,] output = new byte[endX - startX, endY - startY];
                int outi = 0;
                for (int i = startX; i < endX; i++)
                {
                    int outj = 0;
                    //Bitmap tempbit = new Bitmap(ImgInfo.W, ImgInfo.H );
                    for (int j = startY; j < endY; j++)
                    {

                        if (j == endY) { j = j - 1; }
                        if (i == endX) { i = i - 1; }
                        output[outi, outj] = input[i, j];
                        outj++;
                    }
                    outi++;
                }
                return Matrix.Transpose(output);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new byte[0, 0];

            }
        }

        byte[,] PutinListBox(string path, FormatConvert fcv, int strPosX, int strPosY, int endPosX, int endPosY, double scale)
        {
            byte[,] originalFileMat = fcv.Dat2DownMat(path, ImgInfo.WH, scale);
            byte[,] transOri = originalFileMat.Transpose();
            byte[,] pickedMat = PickPixel(transOri, (int)(strPosX / scale), (int)(strPosY / scale), (int)(endPosX / scale), (int)(endPosY / scale));
            originalFileMat = null;
            return Matrix.Transpose(pickedMat);
        }
        public ZoomData SetStartEndPoint(System.Windows.Point start, System.Windows.Point end,int ImgW,int ImgH)
        {
            int realstartX = Math.Min((int)start.X, (int)end.X);
            int realstartY = Math.Min((int)start.Y, (int)end.Y);
            int realendX   = Math.Max((int)start.X, (int)end.X);
            int realendY   = Math.Max((int)start.Y, (int)end.Y);
            ZoomData zomdata = new ZoomData(realstartX, realstartY, realendX, realendY, ImgW, ImgH);
            return zomdata;
        }

        public void CalcRealWH(double scale, ControlData condata ,System.Windows.Point start, System.Windows.Point end, out System.Windows.Point realPStart, out System.Windows.Point realPEnd)
        {
            double ratioW = condata.GridColNum * ImgInfo.W / condata.CanvWidth;//* scale;
            double ratioH = condata.GridRowNum * ImgInfo.H / condata.CanvHeight; //* scale;
            
            realPStart = new System.Windows.Point();
            realPEnd = new System.Windows.Point();

            realPStart.X = (int)((double)start.X * ratioW);
            realPStart.Y = (int)((double)start.Y * ratioH);
            realPEnd.X = (int)((double)end.X * ratioW);
            realPEnd.Y = (int)((double)end.Y * ratioH);
        }

    }
}
