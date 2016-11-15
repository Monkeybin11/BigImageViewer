using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace PLImgViewer
{
    public delegate void TransRealPos( double distance);
    public class MainModule
    {
        List<List<EventClass>>  EvtList;
        ZoomClass               ZomClass;
        ZoomData                ZomData;
        StitchMatrix            Stm;
        ScaleData               scalData;
        ControlData             ConData;
        Grid                    ImgGrid;
        LineProfile             LinePF;
        string[,]               ImgPathBox;

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
        public async Task<byte[,]> StartZoom(Point startPoint,Point endPoint)
        {
            if (ImgPathBox == null)
            { return new byte[0, 0]; }
            else
            {
                Point realstart = new Point();
                Point realend = new Point();
                ZomClass.CalcRealWH(scalData.OriginalScale, ConData, startPoint, endPoint, out realstart, out realend); // 실제 이미지의 픽셀위치로 변환
                ZomData = ZomClass.SetStartEndPoint(realstart, realend, ImgInfo.W, ImgInfo.H); // ZoomData 세팅
                List<List<byte[,]>>  splitedmat = await asyList2ScaledmatList(ImgPathBox, ZomData);
                Stm = new StitchMatrix();
                byte[,] output = await Task.Run(()=> Stm.StitchImage(splitedmat));
                //splitedmat = null;
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
        public async Task<byte[]> AsyStartProfile(Point startPoint, Point endPoint)
        {
            Point realstart = new Point();
            Point realend   = new Point();
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



    }
}
