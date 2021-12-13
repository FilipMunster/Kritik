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
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Dot });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, MajorGridlineStyle = LineStyle.Dot, TextColor = OxyColors.Transparent });
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

        public static LineSeries NewCircleLine(double[] x, double[] y, OxyColor color = default)
        {
            if (x.Length != y.Length) { return null; }
            LineSeries line = new();
            for (int i = 0; i < x.Length; i++)
            {
                line.Points.Add(new DataPoint(x[i], y[i]));
            }
            line.MarkerType = MarkerType.Circle;
            line.MarkerFill = color == default ? OxyColors.DarkGray : color;
            line.LineStyle = LineStyle.None;
            return line;
        }
    }
}
