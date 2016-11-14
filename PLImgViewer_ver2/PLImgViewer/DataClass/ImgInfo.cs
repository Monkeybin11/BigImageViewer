﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
    public static class ImgInfo
    {
        public static int W;
        public static int H;
        public static int WH {

            get { return (W * H); }
        }

        public static void SetImgInfo(int w, int h)
        {
            W = w;
            H = h;
        }
    }
}
