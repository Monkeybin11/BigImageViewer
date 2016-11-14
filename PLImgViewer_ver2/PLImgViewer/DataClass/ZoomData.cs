using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
    public class ZoomData
    {
        public int Sx;
        public int Sy;
        public int Ex;
        public int Ey;
        public int Wo;
        public int Ho;
        public int startNumX = 0;
        public int startNumY = 0;
        public int endNumX = 0;
        public int endNumY = 0;
        public double Scale;

        public ZoomData(int sx, int sy, int ex, int ey, int w, int h)
        {
            Sx = sx;
            Sy = sy;
            Ex = ex;
            Ey = ey;
            Wo = w;
            Ho = h;
        }
    }
}
