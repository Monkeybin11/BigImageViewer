using Accord.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;

namespace PLImgViewer
{
    public class MatrixIO
    {
        public int Rowsize;
        public int ColumnSize;
        public List<List<byte[,]>> GetMatInDir(string dirPath,int length,int scale)
        {
            FormatConvert fcv = new FormatConvert();
            List<List<byte[,]>> output = new List<List<byte[,]>>();

           string[] filenames = System.IO.Directory.GetFiles(dirPath);
            
            List<byte[,]> box0 = new List<byte[,]>();
            List<byte[,]> box1 = new List<byte[,]>();
            List<byte[,]> box2 = new List<byte[,]>();
            List<byte[,]> box3 = new List<byte[,]>();

            foreach (string onefile in filenames)
            {
                string item = System.IO.Path.GetFileName(onefile);
                string[] temp = System.Text.RegularExpressions.Regex.Split(item, "_");

                //Debug.Assert(onefile != "000_000.dat");

                if (temp[0] == "000")
                {
                    box0.Add(fcv.Dat2DownMat(onefile, length, scale));
                }
                if (temp[0] == "001")
                {
                    box1.Add(fcv.Dat2DownMat(onefile, length, scale));
                }
                if (temp[0] == "002")
                {
                    box2.Add(fcv.Dat2DownMat(onefile, length, scale));
                }
                if (temp[0] == "003")
                {
                    box3.Add(fcv.Dat2DownMat(onefile, length, scale));
                }
            }

            if (box0.Count > 0)
            { output.Add(box0); }
            if (box1.Count > 0)
            { output.Add(box1); }
            if (box2.Count > 0)
            { output.Add(box2); }
            if (box3.Count > 0)
            { output.Add(box3); }

            //Debug.Assert(output.Count == 2);
            //Debug.Assert(output[0].Count == 3);

            return output;
        }

        public List<List<string>> GetPathInDir(string dirPath)
        {
            FormatConvert fcv = new FormatConvert();
            List<List<string>> output = new List<List<string>>();

            string[] filenames = System.IO.Directory.GetFiles(dirPath);

            List<string> box0 = new List<string>();
            List<string> box1 = new List<string>();
            List<string> box2 = new List<string>();
            List<string> box3 = new List<string>();

            foreach (string onefile in filenames)
            {
                string item = System.IO.Path.GetFileName(onefile);
                string[] temp = System.Text.RegularExpressions.Regex.Split(item, "_");

                //Debug.Assert(onefile != "000_000.dat");
                item = dirPath + "\\" + item;
                if (temp[0] == "000")
                {
                    box0.Add(item);
                }
                if (temp[0] == "001")
                {
                    box1.Add(item);
                }
                if (temp[0] == "002")
                {
                    box2.Add(item);
                }
                if (temp[0] == "003")
                {
                    box3.Add(item);
                }
            }

            if (box0.Count > 0)
            { output.Add(box0); }
            if (box1.Count > 0)
            { output.Add(box1); }
            if (box2.Count > 0)
            { output.Add(box2); }
            if (box3.Count > 0)
            { output.Add(box3); }

            //Debug.Assert(output.Count == 2);
            //Debug.Assert(output[0].Count == 3);

            return output;
        }
    }
}
