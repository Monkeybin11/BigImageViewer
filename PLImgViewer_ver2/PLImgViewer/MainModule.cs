using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Media;
using Accord.Imaging.Converters;
using Accord.Math;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace PLImgViewer
{
    public delegate void TransDwedColorArr();

    public enum ColorCovMode {
        Rainbow,
        HSV,
        None
    }

    public delegate void TransRealPos( double distance);
    public class MainModule
    {
        List<List<EventClass>>  EvtList; // First is row, second is col Number
        ZoomClass               ZomClass;
        ZoomData                ZomData;
        StitchMatrix            Stm;
        ScaleData               scalData;
        ControlData             ConData;
        Grid                    ImgGrid;
        LineProfile             LinePF;
        string[,]               ImgPathBox;
        ColorCovMode            ConvColorMode;
        System.Windows.Point    CurrentPoint;
        bool                    IsColorConverted;

        BitmapSource            ZoomGray;
        BitmapSource            ZoomRain;
        BitmapSource            ZoomHsv;

        public event TransRealPos evtTransRealPos;

        public MainModule()
        {
            EvtList    = new List<List<EventClass>>();
            ZomClass   = new ZoomClass();
            scalData   = new ScaleData();
            ConData    = new ControlData();
            LinePF     = new LineProfile();


        }
        public void CreateGridPanelEvent(Canvas rootcanv)
        {
            CreateControl createcon = new CreateControl();
            //createcon.evtTransDwedSampleData += ApplyColorMethod; // convert color
            //createcon.evtTransDwedSamplePointData += SetCurrentPoint;

            /*----- Grid ------*/
            int width = (int)((ConData.CanvWidth)   / ConData.GridColNum);
            int height = (int)((ConData.CanvHeight) / ConData.GridRowNum);

            ImgGrid = createcon.DrawGrid(rootcanv, ConData.GridRowNum, ConData.GridColNum, width, height); 

            rootcanv.Children.Clear();
            rootcanv.Children.Add(ImgGrid);

            /*----- stack panel ------*/
            /*
             1. 스택패널 & 이미지 당 한개의 이벤트 클래스를 생성해 연결한다. 
             2. 생성된 이벤트 클라스는 전역변수 이벤트 클래스 리스트에 저장을 한다. 
             3. 이미지 경로 박스는 전역변수로서, 각 이벤트 클래스에는 박스의 [i,j]를 할당한다. 클래스의 멤버 string 하나가 박스의 [i,j] 라고 할 수 있다. 
             */
            for (int i = 0; i < ConData.GridRowNum; i++)
            {
                List<EventClass> temp = new List<EventClass>();
                for (int j = 0; j < ConData.GridColNum; j++)
                {
                    StackPanel stp = createcon.AttStackPanel(ImgPathBox, i, j, width, height, temp,scalData.OriginalScale); // ImgPathBox 는 MainModule 의 멤버
                    ImgGrid.Children.Add(stp);
                }
                EvtList.Add(temp);
            }
        }

        /* Zoom
         1. 이벤트로 받은 포인트를 가져온다. 
         2. 포인트를 정렬한다. 
         3. ZoomData에 방금 구한 데이터들을 넣는다. 
         4. 이제 위에서 구한 데이터로 매트릭스 리스트를 만든다. 
             
             */
        public async Task<BitmapSource> StartZoom( System.Windows.Point startPoint , System.Windows.Point endPoint )
        {
            if (ImgPathBox == null)
            { return null; }
            else
            {
                System.Windows.Point realstart = new System.Windows.Point();
                System.Windows.Point realend = new System.Windows.Point();
                ZomClass.CalcRealWH(scalData.OriginalScale, ConData, startPoint, endPoint, out realstart, out realend); // 실제 이미지의 픽셀위치로 변환
                ZomData = ZomClass.SetStartEndPoint(realstart, realend, ImgInfo.W, ImgInfo.H); // ZoomData 세팅
                List<List<byte[,]>>  splitedmat = await asyList2ScaledmatList(ImgPathBox, ZomData);
                Stm = new StitchMatrix();
                byte[,] stitchedArr = Stm.StitchArr(splitedmat);
                BitmapSource output = await Task.Run(()=> CreateBitmapSourceClass.CreateBitmapSource( stitchedArr));
                ZoomGray = output;
                await Task.Run( ()=> ZoomRain = Arr2Source( stitchedArr , ColorCovMode.Rainbow ) );
                await Task.Run( ()=> ZoomHsv = Arr2Source( stitchedArr , ColorCovMode.HSV ) );
                return output;
            }
        }

        async Task<List<List<byte[,]>>> asyList2ScaledmatList(string[,] input, ZoomData data)
        {
            List<List<byte[,]>> result = await Task<List<List<byte[,]>>>.Run(() => ZomClass.AsyListDownScaledZoomedMat(input, data));
            return result;
        }

        /* Line Profile
         * 1. 포인트 가져오기
         * 2. 포인트가 원본의 어느 위치인지 계산한다. 
         * 3. 원본위의 포인트를 이용해 직선을 만들고, 각 직선이 지나가는 Dat파일들을 구한다. 
         * 4. 각각의 dat파일에서 직선이 통과하는 점들의 값을 가져온다.
         * 5. 각각의 점들의 값의 리스트를 그래프로 그린다.  
         */

        #region LineProfile
        public async Task<byte[]> AsyStartProfile( System.Windows.Point startPoint , System.Windows.Point endPoint )
        {
            System.Windows.Point realstart = new System.Windows.Point();
            System.Windows.Point realend   = new System.Windows.Point();
            double scale    = 0;

            await Task.Run(()=> ZomClass.CalcRealWH(scale, ConData, startPoint, endPoint, out realstart, out realend)); // sjw need Currying
            evtTransRealPos( await Task.Run( () => LinePF.EuclideanDistance( ImgInfo.PixelResolution )( realstart )( realend ) ) );
            await Task.Run(()=> LinePF.SetLineProfile(realstart, realend, ConData)); 
            return LinePF.GetLineProfileData(ImgPathBox);
        }
        
        #endregion

        #region Set Method
        public void SetScale( int rowcol , int canvWH , int oriWH )
        {
            double unitnum  = rowcol * oriWH;
            double ratio =  oriWH / unitnum;
            int count = 1;
            while ( unitnum > 5000000 * count )
            {
                count++;
            }
            scalData.OriginalScale = count;
        }

        public void SetControlData( int imgW , int imgH , int canvW , int canvH )
        {
            ConData.SetData( imgW , imgH , canvW , canvH );
        }

        public void SetContDataRCNumAndPath( int gridrow , int gridcol )
        {
            ConData.GridRowNum = gridrow;
            ConData.GridColNum = gridcol;
            SetImgPath( ConData );
        }

        public void SetContDataRCNum( int gridrow , int gridcol )
        {
            ConData.GridRowNum = gridrow;
            ConData.GridColNum = gridcol;
        }

        void SetImgPath( ControlData conData )
        {
            ImgPathBox = new string[conData.GridRowNum , conData.GridColNum];
        }
        public void OnOffGridLine()
        {
            if ( ImgGrid.ShowGridLines == false )
            { ImgGrid.ShowGridLines = true; }
            else
            { ImgGrid.ShowGridLines = false; }
        }
        #endregion

        #region Convert Color
        public void SetCurrentPoint( System.Windows.Point input )
        {
            CurrentPoint = input;
        }

        public Func<byte[] , Color[]> ConvertColor(ColorCovMode colorMode)
        {
            Func<byte[],Color[]> convertmethod = input => ColorConvertMethod(colorMode , input);
            return convertmethod;
        }

        Color[] ColorConvertMethod( ColorCovMode colorMode , byte[] input)
        {
            Color[] output; 
            switch ( colorMode )
            {
                case ColorCovMode.None:
                    break;

                case ColorCovMode.Rainbow:
                    RainbowGradation colormaprain = new RainbowGradation();
                    System.Drawing.Color[] clrmaprain = colormaprain.GetGradation( 255 , false );
                    output = new Color[input.GetLength( 0 )];
                    for ( int i = 0 ; i < input.GetLength(0) ; i++ )
                    {
                        output[i] = clrmaprain[input[i]];
                    }
                    return output;
                case ColorCovMode.HSV:
                    HSVGradation colormaphsv = new HSVGradation();
                    System.Drawing.Color[] clrmaphsv = colormaphsv.GetGradation( 255 , false );
                    output = new Color[input.GetLength( 0 )];
                    for ( int i = 0 ; i < input.GetLength( 0 ) ; i++ )
                    {
                        output[i] = clrmaphsv[input[i]];
                    }
                    return output;
            }
            return null;
        }

        #endregion

        #region Color Convert
        public void Convert2Color()
        {
            for ( int i = 0 ; i < EvtList.Count ; i++ )
            {
                for ( int j = 0 ; j < EvtList[i].Count ; j++ )
                {
                    EvtList[i][j].Img.Source = EvtList[i][j].RainbowImg;
                }
            }
        }

        public void Convert2Gray()
        {
            for( int i = 0 ; i < EvtList.Count; i++ )
            {
                for ( int j = 0 ; j < EvtList[i].Count ; j++ )
                {
                    EvtList[i][j].Img.Source = EvtList[i][j].OriginalImg;
                }
            }
        }

        BitmapSource Arr2Source( byte[,] input , ColorCovMode colomod )
        {
            byte[] flatMatrix = input.Flatten<byte>();
            Color[] rainbowArr = ConvertColor( colomod )( flatMatrix );
            ArrayToImage convertor = new ArrayToImage(input.GetLength(1),input.GetLength(0));
            System.Drawing.Bitmap imgbit= new System.Drawing.Bitmap(input.GetLength(1),input.GetLength(0));
            convertor.Convert( rainbowArr , out imgbit );
            return CreateBitmapSourceClass.ToWpfBitmap( imgbit );
        }

        public void ZoomColorChange(System.Windows.Controls.Image imgpanel ,bool IsColorized)
        {
            var stackSource2ZoomPanel = ZoomImgSourceSet( imgpanel );
            if ( IsColorized ){
                stackSource2ZoomPanel( ZoomGray );
            }
            else
            {
                stackSource2ZoomPanel( ZoomHsv );
            }
        }

        Action<BitmapSource> ZoomImgSourceSet( System.Windows.Controls.Image imagepanel )
        {
            Action<BitmapSource> action = sourceInput =>
            {
                imagepanel.Source = sourceInput;
            };
            return action;
        }
        #endregion

    }
}
