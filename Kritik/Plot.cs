using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;

namespace Kritik
{
    internal static class Plot
    {
        public static PlotModel NewModel()
        {
            PlotModel model = new();
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
            return model;
        }


        public static LineSeries NewLine(double[] x, double[] y, OxyColor color = default)
        {
            if (x.Length != y.Length) { return null; }
            LineSeries line = new(); 
            for (int i = 0; i < x.Length; i++)
            {
                line.Points.Add(new DataPoint(x[i], y[i]));
            }
            line.Color = color == default ? OxyColors.Black : color;
            return line;
        }
    }
}
