using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PLImgViewer
{
    public delegate void TransDwedSamplePointData( System.Windows.Point input );
    public class CreateControl
    {
        public Grid DrawGrid( Canvas canvas ,int row, int col, int width, int height)
        {
            Grid grid = new Grid();
            grid.Width = canvas.Width;
            grid.Height = canvas.Height;
            grid.ShowGridLines = true;
            grid.Background = System.Windows.Media.Brushes.Transparent;
            Grid.SetRow(grid, 1);
            for (int i = 0; i < row; i++)
            {
                RowDefinition rowtemp = new RowDefinition();
                rowtemp.Height = new GridLength(height);
                grid.RowDefinitions.Add(rowtemp);
            }
            for (int i = 0; i < col; i++)
            {
                ColumnDefinition coltemp = new ColumnDefinition();
                coltemp.Width = new GridLength(width);
                grid.ColumnDefinitions.Add(coltemp);
            }
            return grid;
        }

        public StackPanel AttStackPanel(string[,] imgPath ,int i, int j, int width, int height, List<EventClass> temp, double scale)
        {
            StackPanel stp = new StackPanel();
            stp.AllowDrop = true;
            stp.Width = width;
            stp.Height = height;
            Grid.SetRow(stp, i);
            Grid.SetColumn(stp, j);

            stp.Background = System.Windows.Media.Brushes.Transparent;

            System.Windows.Controls.Image imgpanel = new System.Windows.Controls.Image();
            imgpanel.Width = width;
            imgpanel.Height = height;
            imgpanel.Stretch = Stretch.Fill;
            EventClass evc = new EventClass(stp, imgpanel, i, j, imgPath, scale);
            temp.Add(evc);
            stp.Drop += evc.DropEventMethod;

            stp.Children.Add(imgpanel);
            return stp;
        }

        public byte[,] TransDwedSample( byte[,] input)
        {
            return input;
        }

    }
}
