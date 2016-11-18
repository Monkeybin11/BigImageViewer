using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLImgViewer
{
    public interface IGradation
    {
        System.Drawing.Color[] GetGradation(int count, bool inverse);
        System.Windows.Media.Color[] GetGradationWPF(int count, bool inverse);
    }
}
