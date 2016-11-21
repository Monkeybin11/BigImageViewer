using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Accord.Math;
using System.Drawing.Imaging;
using System.Diagnostics;
using MahApps.Metro.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace PLImgViewer
{
    public partial class MainWindow : MetroWindow
    {
        System.Windows.Shapes.Rectangle rect;
        Line Proline;
        System.Windows.Point startPoint;
        System.Windows.Point startLinePoint;
        System.Windows.Point endPoint;
        System.Windows.Point endLinePoint;
        MainModule  Mainmod;
        bool CtrlPushed;
        bool AltPushed;

        public SeriesCollection seriesbox { get; set; }
        public ChartValues<int> chartV { get; set; }
        List<int> XLabels;
        List<int> YValue;

        public MainWindow()
        {
            InitializeComponent();
            Mainmod = new MainModule();
            SetImgGridInfo();
            InitLineProfData();
            Mainmod.evtTransRealPos += new TransRealPos( DisplayDistance );
        }

        #region Init
        /*
        * ImgInfo 와 Grid 만들면, 실제 각각의 이미지의 WH,그리고 갯수를 ImgInfo 와 ControlData 에 저장을 한다. 
        */
        void SetImgGridInfo()
        {
            int width = Convert.ToInt32(txbW.Text);
            int height = Convert.ToInt32(txbH.Text);
            double pixelresol = Convert.ToDouble(txbResol.Text);
            ImgInfo.SetImgInfo(width, height, pixelresol );
        }

        void InitControlData()
        {
            int zomW   = (int)imgzommed.Width;
            int zomH   = (int)imgzommed.Height;
            int canvW  = (int)canvRoot.Width;
            int canvH  = (int)canvRoot.Height;
            int rownum = Convert.ToInt32(txbRowNum.Text);
            int colnum = Convert.ToInt32(txbColNum.Text);

            Mainmod.SetControlData(zomW, zomH, canvW, canvH);
            Mainmod.SetContDataRCNumAndPath(rownum, colnum);
        }

        void InitLineProfData()
        {
            YValue = new List<int>();
            XLabels = new List<int>();
            connectValue();
            makeseries();
            DataContext = this;
        }
        #endregion

        #region button Event
        private void btnCreateGrid_Click(object sender, RoutedEventArgs e)
        {
            InitControlData();

            int rownum = Convert.ToInt32(txbRowNum.Text);
            int colnum = Convert.ToInt32(txbColNum.Text);
            Mainmod.SetScale(rownum * colnum, (int)(canvRoot.Width * canvRoot.Height), ImgInfo.W * ImgInfo.H);
            Mainmod.CreateGridPanelEvent(canvRoot);
        }

        private void btnetInfo_Click(object sender, RoutedEventArgs e)
        {
            ImgInfo.W = Convert.ToInt32(txbW.Text);
            ImgInfo.H = Convert.ToInt32(txbH.Text);
            ImgInfo.PixelResolution = Convert.ToDouble( txbResol.Text );
            int rownum = Convert.ToInt32(txbRowNum.Text);
            int colnum = Convert.ToInt32(txbColNum.Text);
            Mainmod.SetContDataRCNum( rownum , colnum );
        }
        private void btnDisplayLine_Click(object sender, RoutedEventArgs e)
        {
            Mainmod.OnOffGridLine();
        }
        #endregion

        #region Event
        private void canvRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txbStartPos.Text = $" [ {e.GetPosition(canvRoot).X} , {e.GetPosition(canvRoot).Y} ]";
            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                startPoint = e.GetPosition(canvRoot);

                rect = new System.Windows.Shapes.Rectangle
                {
                    Stroke = System.Windows.Media.Brushes.LightBlue,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(rect, startPoint.X);
                Canvas.SetTop(rect, startPoint.X);
                canvRoot.Children.Add(rect);
                CtrlPushed = true;
            }

            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                canvRoot.Children.Remove(Proline);
                Proline = new Line();
                Proline.Stroke = System.Windows.Media.Brushes.Indigo;
                Proline.StrokeThickness = 1;
                canvRoot.Children.Add(Proline);
                startLinePoint = e.GetPosition(canvRoot);
                AltPushed = true;
            }   

        }

        private void canvRoot_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            txbEndPos.Text = $"   [ {e.GetPosition(canvRoot).X} , {e.GetPosition(canvRoot).Y} ]";
            if (e.LeftButton == MouseButtonState.Released)
                return;
            #region DrawRect
            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && rect != null)
            {
                try
                {

                    var pos = e.GetPosition(canvRoot);
                    var x = Math.Min(pos.X, startPoint.X);
                    var y = Math.Min(pos.Y, startPoint.Y);
                    var w = Math.Max(pos.X, startPoint.X) - x;
                    var h = Math.Max(pos.Y, startPoint.Y) - y;
                    rect.Width = w;
                    rect.Height = h;
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);

                }
                catch (Exception eex)
                {
                    Console.WriteLine(eex.ToString());
                }
            }
            #endregion

            #region DrawLine
            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) && Proline != null)
            {
                Proline.X1 = startLinePoint.X;
                Proline.Y1 = startLinePoint.Y;
                Proline.X2 = e.GetPosition(canvRoot).X;
                Proline.Y2 = e.GetPosition(canvRoot).Y;
            }
            #endregion
        }

        /* 시퀀스
        1. string[,] 으로 파일 경로 매트릭스 만들기
        2. 줌을 하는 파일의 갯수 + 원본 파일의 크기로 스케일 정하기.
        3. 줌하는 영역 계산. (시작하는 파일 끝나는 파일, 각 파일별로 시작 끝점 계산)
        4. 각 영역의 데이터를 매트릭스로 가져온다.
        5. 각 영역의 데이터 매트릭스를 리스케일링 한다. 
        6. 각 영역의 리스케일링 된 매트릭스를 합친다.
        7. 합쳐진 매트릭스를 비트맵소스로 변환한다.     
        */

        private async void canvRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            ZoomClass zom = new ZoomClass();
            StitchMatrix smat = new StitchMatrix();
            canvRoot.Children.Remove(rect);

            #region Zoom
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && CtrlPushed == true)
            {
                txbZoomStatus.Text = "Busy";
                endPoint = e.GetPosition(canvRoot);

                imgzommed.Source = await Mainmod.StartZoom(startPoint, endPoint);
                txbZoomStatus.Text = "Ready";
                rbtnZoomGray.IsChecked = true;
            }
            #endregion

            #region Line Profile
            if ((Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) && AltPushed == true)
            {
                txbZoomStatus.Text = "Busy";
                endLinePoint = e.GetPosition(canvRoot);
                byte[] result = await Mainmod.AsyStartProfile(startLinePoint,endLinePoint);
                AltPushed = false;

                await Task.Run((Action)(() => lineProChart.Dispatcher.BeginInvoke((Action)(()=>AsySetLineValue(Arr2List(result), seriesbox)))));
                txbZoomStatus.Text = "Ready";
            }
            #endregion
            Mouse.OverrideCursor = null;
        }

        private void canvRoot_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            canvRoot.Children.Remove(rect);
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && CtrlPushed == true)
            {
                canvRoot.Children.Remove(rect);
            }
            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                canvRoot.Children.Remove(Proline);
            }
        }
        #endregion

        #region LineProfile Display

        async void AsySetLineValue(List<int> Yinput,SeriesCollection seriescol)
        {
            await Task.Run(() => {
                YValue = Yinput;
                for (int i = 0; i < YValue.Count; i++)
                {
                    XLabels.Add(i);
                }

                connectValue();
            });

            await Task.Run(() => lineProChart.Dispatcher.BeginInvoke(
                (Action)( () => seriescol[0].Values = chartV)) );
        }

      
        

        void connectValue()
        {
            chartV = new ChartValues<int>(YValue);
        }
        void makeseries()
        {
            seriesbox = new SeriesCollection {
                new LineSeries {
                    Title = "LineProfileSeries",
                    Values = chartV,
                    PointGeometry = null

                }
            };
        }

        List<int> Arr2List(byte[] input)
        {
            List<int> output = new List<int>();

            for (int i = 0; i < input.Length; i++)
            {
                output.Add((int)input[i]);
            }
            return output;
        }
        #endregion

        #region Evetn From Control
        void DisplayDistance( double distance )
        {
            txbLineLength.Dispatcher.BeginInvoke( ( Action ) ( () =>
            txbLineLength.Text = distance.ToString("#.##") ) );
            Console.WriteLine( distance.ToString() );
        }

        #region Color Change
        private void rbtnZoomRB_Checked( object sender , RoutedEventArgs e )
        {
            Mainmod.ZoomColorChange( imgzommed , ColorCovMode.Rainbow );
        }

        private void rbtnZoomHSV_Checked( object sender , RoutedEventArgs e )
        {
            Mainmod.ZoomColorChange( imgzommed , ColorCovMode.HSV );
        }

        private void rbtnZoomGray_Checked( object sender , RoutedEventArgs e )
        {
            Mainmod?.ZoomColorChange( imgzommed , ColorCovMode.Gray );
        }

        private void rbtnOriRB_Checked( object sender , RoutedEventArgs e )
        {
            Mainmod.Convert2RB();
        }

        private void rbtnOriHSV_Checked( object sender , RoutedEventArgs e )
        {
            Mainmod.Convert2HSV();
        }

        private void rbtnOriGray_Checked( object sender , RoutedEventArgs e )
        {
            Mainmod?.Convert2Gray();
        }
        #endregion
        #endregion

        // ---- Color Change ---- //

    }
}
