using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
    public class ViewData
    {
        public byte[,] Original;
        public byte[,] Zoomed;
        public ViewData()
        {
            Original = new byte[,] { };
            Zoomed   = new byte[,] { };
        }
    }
}
