using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
    public class ControlData
    {
        public int ImgZoomW;
        public int ImgZoomH;
        public int CanvWidth;
        public int CanvHeight;
        public int GridRowNum;
        public int GridColNum;

        public void  SetData(int imgW, int imgH ,int canvW, int canvH)
        {
            ImgZoomW   = imgW   ;
            ImgZoomH   = imgH   ;
            CanvWidth  = canvW  ;
            CanvHeight = canvH  ;
        }
    }
}
