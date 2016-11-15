using Accord.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PLImgViewer
{
    public class LineProfile
    {
        LineEqData EqData;
        LineProfileData LineData;
        List<int> XData;
        List<int> YData;

        public void SetLineProfile(Point startRealP, Point endRealP, ControlData condata)
        {
            EqData = new LineEqData();
            LineData = new LineProfileData();
            XData = new List<int>();
            YData = new List<int>();
            InitData(startRealP, endRealP, condata);
        }

        public Func<Point , Func<Point , double>> EuclideanDistance(double resol ) // Need Currying for Simple Implement
        {
            var MultResolution  = GetMultiply(resol);
           

            Func<Point, Func<Point,double>> calcDistance = ( inputStartPos =>
            {
                var DisFromStart    = GetDis(inputStartPos);

                Func<Point , double> calcDis2End = (inputEndPos=>
                {
                    return MultResolution( Math.Sqrt( DisFromStart(inputEndPos) )  );
                } );

                return calcDis2End;
            });
            
            return calcDistance;
        }

        #region Helper
        Func<double , double> GetAdd( double input )
        {
            return x => input + x;
        }

        Func<double, double> GetSub( double input )
        {
            return x=> input - x;
        }

        Func<double,double> GetMultiply (double input )
        {
            return x => input * x;
        }

        Func<Point,double> GetDis (Point endPoint )
        {
            return x => Math.Pow(( endPoint.X - x.X ),2) + Math.Pow(( endPoint.Y - x.Y ),2); 
        }

        

        #endregion

        #region InitData Member
        public void InitData(Point startRealP, Point endRealP, ControlData condata)
        {
                SetLineData(startRealP, endRealP, condata); // 라인 데이터 셋 -> ok
                SetLineEq(startRealP, endRealP);
        }

        void SetLineEq(Point startRealP, Point endRealP)
        {
            EqData = new LineEqData();

            double Ax = (endRealP.Y - startRealP.Y) / (endRealP.X - startRealP.X);
            double Bx = 1;
            EqData.SetLlineEqX(Ax, Bx);

            double Ay = (endRealP.X - startRealP.X) / (endRealP.Y - startRealP.Y);
            double By = 1;
            EqData.SetLlineEqY(Ay, By);
        }

        void SetDataList(List<int> xlist, List<int> ylist, int xNum)
        {
            for (int i = 0; i < xNum; i++)
            {
                double x = i;
                xlist.Add(i);
                ylist.Add((int)(EqData.Ax * x + EqData.Bx));
            }
        }

        void SetLineData(Point startRealP, Point endRealP, ControlData condata)
        {
            Point ordstart;
            Point ordend;

            if (Math.Abs(startRealP.X - endRealP.X) >= Math.Abs(startRealP.Y - endRealP.Y))
            {
                SetStartendForX(startRealP, endRealP, out ordstart, out ordend);
            }
            else
            {
                SetStartendForY(startRealP, endRealP, out ordstart, out ordend);
            }

            LineData.StartPoint = ordstart;
            LineData.EndPoint   = ordend;

            for (int i = 0; i < condata.GridColNum; i++)
            {
                if (ordstart.X < (i + 1) * ImgInfo.W)
                {
                    LineData.StartNumX = i;
                    break;
                }
            }

            for (int i = 0; i < condata.GridRowNum; i++)
            {
                if (ordstart.Y < (i + 1) * ImgInfo.H)
                {
                    LineData.StartNumY = i;
                    break;
                }
            }

            for (int i = condata.GridColNum; i > 0; i--)
            {
                if (ordend.X > i * ImgInfo.W)
                {
                    LineData.EndNumX = i;
                    break;
                }
            }

            for (int i = condata.GridRowNum; i > 0; i--)
            {
                if (ordend.Y > i * ImgInfo.H)
                {
                    LineData.EndNumY = i;
                    break;
                }
            }
        }

        void SetStartendForX(Point start,Point end,out Point ordstart, out Point ordend)
        {
            ordstart   = new Point();
            ordend     = new Point();

            if (start.X < end.X)
            {
                ordstart = start;
                ordend = end;
            }
            else
            {
                ordstart = end;
                ordend = start;
            }
        }
        void SetStartendForY(Point start, Point end, out Point ordstart, out Point ordend)
        {
            ordstart = new Point();
            ordend = new Point();

            if (start.Y < end.Y)
            {
                ordstart = start;
                ordend = end;
            }
            else
            {
                ordstart = end;
                ordend = start;
            }
        }
        #endregion

        #region GetLineProfilePixel Data
        public byte[] GetLineProfileData(string[,] inputPath)
        {
            FormatConvert fcv = new FormatConvert();
            /* X축과 Y축 중에 더 긴쪽을 중심으로 전개한다. 같은 경우는 X축으로 */
            if ( Math.Abs(LineData.StartPoint.X - LineData.EndPoint.X) >= Math.Abs(LineData.StartPoint.Y - LineData.EndPoint.Y))
            {
                return XOrder(inputPath,LineData);
            }
            else
            {
                return YOrder(inputPath, LineData);
            }

            /* y의 끝점이 한개의 이미지의 y끝점에서  안에 있는지, 이미지보다 위로 가는지 아래로 가는지*/
        }

        byte[] XOrder(string[,] inputPath, LineProfileData lineData) // lineData 는 문제없음.
        {
            double resultForAssert = lineData.EndPoint.X - lineData.StartPoint.X;
            

            int x = LineData.StartNumX;
            int y = LineData.StartNumY;
            int xend = LineData.EndNumX;
            int yend = LineData.EndNumY;
            try
            {

                FormatConvert fcv = new FormatConvert();
                List<byte> outputlist = new List<byte>();
                /* y의 끝점이 한개의 이미지의 y끝점에서  안에 있는지, 이미지보다 위로 가는지 아래로 가는지*/
                int innerSX = (int)lineData.StartPoint.X - x * ImgInfo.W;
                int innerSY = (int)lineData.StartPoint.Y - y * ImgInfo.H;
                int innerEX = (int)lineData.EndPoint.X - xend * ImgInfo.W;

                while (true) 
                {
                    byte[,] data = fcv.Dat2Mat(inputPath[y, x]); // string[row,col] 순 이므로 [y,x] 순이 된다.  =>ok
                    double Bx = innerSY - EqData.Ax * innerSX; //inner 시작점 => ok

                    if (x == lineData.EndNumX) // End
                    {
                        if (y == yend)
                        {
                            for (int j = innerSX; j < innerEX; j++)
                            {
                                int yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]); // j = x
                            }
                            break;
                        }
                        else if (y > yend)
                        {
                            for (int j = innerSX; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + Bx < 0)
                                {
                                    innerSX = j;
                                    break;
                                }
                                int yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]); // j = x
                            }
                            innerSY = ImgInfo.H;
                            y--;
                        }
                        else
                        {
                            for (int j = innerSX; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + Bx > ImgInfo.H)
                                {
                                    innerSX = j;
                                    break;
                                }
                                int yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]);
                            }
                            innerSY = 0;
                            y++;
                        }
                    } // X is in End


                    else if (x == lineData.StartNumX) // Start
                    {
                        if ((EqData.Ax * ImgInfo.W + Bx) < 0)
                        {
                            for (int j = innerSX; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + Bx < 0)
                                {
                                    innerSX = j;
                                    innerSY = (y * ImgInfo.H);
                                    break;
                                }
                                int yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]); // j = x
                            }
                            innerSY = ImgInfo.H;
                            y--;

                        }
                        else if ((EqData.Ax * ImgInfo.W + Bx) > ImgInfo.H)
                        {

                            for (int j = innerSX; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + Bx > ImgInfo.H)
                                {
                                    innerSX = j;
                                    innerSY = (y * ImgInfo.H);
                                    break;
                                }
                                int yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]);
                            }
                            innerSY = 0;
                            y++;
                        }
                        else
                        {
                            int yPos = 0;
                            for (int j = innerSX; j < ImgInfo.W; j++)
                            {
                                yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]);
                            }
                            innerSX = 0;
                            innerSY = yPos;
                            x++;
                        }
                    }
                    else // Middle
                    {
                        if ((EqData.Ax * ImgInfo.W + Bx) < 0)
                        {
                            for (int j = 0; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + Bx < 0)
                                {
                                    innerSX = j;
                                    break;
                                }
                                int yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]); // j = x
                            }
                            innerSY = ImgInfo.H;
                            y--;
                        }
                        else if ((EqData.Ax * ImgInfo.W + Bx) > ImgInfo.H)
                        {
                            for (int j = 0; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + Bx > ImgInfo.H)
                                {
                                    innerSX = j;
                                    break;
                                }
                                int yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]);
                            }
                            innerSY = 0;
                            y++;
                        }
                        else // mid
                        {
                            int yPos = 0;
                            for (int j = innerSX; j < ImgInfo.W; j++)
                            {
                                yPos = (int)(EqData.Ax * j + Bx - 1) >= 0 ? (int)(EqData.Ax * j + Bx - 1) : 0;
                                outputlist.Add(data[yPos, j]);
                            }
                            innerSX = 0;
                            innerSY = yPos;
                            x++;
                        }
                    } // X is not in End
                }
                

                byte[] output = ConvertList2ByteArr(outputlist);
                Debug.Assert(resultForAssert == output.Length);
                return output;

            }
            catch (Exception eex)
            {
                MessageBox.Show(eex.ToString());
                return new byte[0];
            }
        }

        byte[] YOrder(string[,] inputPath, LineProfileData lineData)
        {
           
            double resultForAssert = lineData.EndPoint.Y - lineData.StartPoint.Y;

            int x = LineData.StartNumX;
            int y = LineData.StartNumY;
            int xend = LineData.EndNumX;
            int yend = LineData.EndNumY;
            try
            {
                FormatConvert fcv = new FormatConvert();
                /* y의 끝점이 한개의 이미지의 y끝점에서  안에 있는지, 이미지보다 위로 가는지 아래로 가는지*/
                List<byte> outputlist = new List<byte>();
                int innerSX = (int)lineData.StartPoint.X - x * ImgInfo.W;
                int innerSY = (int)lineData.StartPoint.Y - y * ImgInfo.H;
                int innerEY = (int)lineData.EndPoint.Y - yend * ImgInfo.H;

                while (true)
                {
                    byte[,] data = fcv.Dat2Mat(inputPath[y, x]); // string[row,col] 순 이므로 [y,x] 순이 된다. 
                    double By = innerSX - EqData.Ay * innerSY; // ok

                    if (y == lineData.EndNumY)
                    {
                        if (x == xend)
                        {
                            for (int j = innerSY; j < innerEY; j++)
                            {
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0? (int)(EqData.Ay * j + By - 1): 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            break;
                        }
                        else if (x > xend)
                        {
                            for (int j = innerSY; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + By < 0)
                                {
                                    innerSY = j;
                                    break;
                                }
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSX = ImgInfo.W;
                            x--;
                        }
                        else
                        {
                            for (int j = 0; j < ImgInfo.W; j++)
                            {
                                if (EqData.Ax * j + By > ImgInfo.W)
                                {
                                    innerSY = j;
                                    break;
                                }
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSX = 0;
                            x++;
                        }
                    } // Y is in End


                    else if (y == lineData.StartNumY)
                    {
                        if ((EqData.Ay * ImgInfo.H + By) < 0)
                        {
                            for (int j = innerSY; j < ImgInfo.H; j++)
                            {
                                if (EqData.Ay * j + By < 0) // 칸을 넘어갔을경우
                                {
                                    innerSY = j;
                                    break;
                                }
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSX = ImgInfo.W;
                            x--;
                        }
                        else if ((EqData.Ay * ImgInfo.H + By) > ImgInfo.W)
                        {
                            for (int j = 0; j < ImgInfo.H; j++)
                            {
                                if (EqData.Ay * j + By > ImgInfo.W)
                                {
                                    innerSY = j;
                                    break;
                                }
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSX = 0;
                            x++;
                        }
                        else
                        {
                            for (int j = innerSY; j < ImgInfo.H; j++)
                            {
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSY = 0;
                            y++;
                        }
                    }
                    else
                    {
                        if ((EqData.Ay * ImgInfo.H + By) < 0)
                        {
                            for (int j = innerSY; j < ImgInfo.H; j++)
                            {
                                if (EqData.Ay * j + By < 0)
                                {
                                    innerSY = j;
                                    break;
                                }
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSX = ImgInfo.W;
                            x--;
                        }
                        else if ((EqData.Ay * ImgInfo.H + By) > ImgInfo.W)
                        {
                            for (int j = innerSY; j < ImgInfo.H; j++)
                            {
                                if (EqData.Ay * j + By > ImgInfo.W)
                                {
                                    innerSY = j;
                                    break;
                                }
                                int xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSX = 0;
                            x++;
                        }
                        else
                        {
                            int xPos = 0;
                            for (int j = innerSY; j < ImgInfo.H; j++)
                            {
                                xPos = (int)(EqData.Ay * j + By - 1) >= 0 ? (int)(EqData.Ay * j + By - 1) : 0;
                                outputlist.Add(data[j, xPos]);
                            }
                            innerSY = 0;
                            innerSX = xPos;
                            y++;
                        }
                    } // Y is not in End
                }

                /* 하나의 라인데이터가 리스트로 저장이 된다. 이 리스트를 byte[] 로 변환해준다. */

                byte[] output = ConvertList2ByteArr(outputlist);
                Debug.Assert(resultForAssert == output.Length);
                return output;

            }
            catch (Exception eex)
            {
                Console.WriteLine(eex.ToString());
                return new byte[0];
            }
        }

        void ChangeStartEndYorder(LineProfileData lineData)
        {
            if (lineData.StartPoint.Y > lineData.EndPoint.Y)
            {
                Point startPoint    = lineData.EndPoint;
                Point endPoint  = lineData.StartPoint;
                int startNumX   = lineData.EndNumX;
                int endnumX     = lineData.StartNumX;
                int startNumY   = lineData.EndNumY;
                int endnumY     = lineData.StartNumY;

                lineData.EndPoint = endPoint;
                lineData.StartPoint = startPoint;
                lineData.EndNumX = endnumX;
                lineData.StartNumX = startNumX;
                lineData.EndNumY = endnumY;
                lineData.StartNumY = startNumY;
            }
        }

        byte[] ConvertList2ByteArr(List<byte> input)
        {
            byte[] output = new byte[input.Count];
            for (int i = 0; i < input.Count; i++)
            {
                output[i] = input[i];
            }

            return output;
        }

        byte[] PickLinePixelData(byte[,] input ,Point innerstart, Point innerend)
        {
            int sx = (int)innerstart.X;
            int ex = (int)innerend.X;

            try
            {
                byte[] output = new byte[(int)(innerend.X - innerstart.X + 1)];
                double B = innerstart.Y - EqData.Ax * innerstart.X;
                for (int i = sx; i <= ex; i++)
                {
                    output[i - sx] = (byte)(EqData.Ax * i + B); 
                }
                return output;

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                return new byte[0];
            }
        }


        #endregion
    }
}
