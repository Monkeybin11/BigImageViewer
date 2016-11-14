using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLImgViewer
{
    public class LineEqData
    {
        public double Ax;
        public double Bx;
        public double Ay;
        public double By;

        public void SetLlineEqX(double a, double b)
        {
            Ax = a;
            Bx = b;
        }
        public void SetLlineEqY(double a, double b)
        {
            Ay = a;
            By = b;
        }
    }
}
