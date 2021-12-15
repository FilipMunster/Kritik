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
        public static PlotModel NewVelky()
        {
            PlotModel model = new();
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Dot });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
            model.Padding = new OxyThickness(4);
            return model;
        }
        public static PlotModel NewMaly()
        {
            PlotModel model = new();
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
            model.Padding = new OxyThickness(4);
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

        public static LineSeries NewOsa(double[] x, OxyColor color = default)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(0, 0));
            line.Points.Add(new DataPoint(x[^1], 0));
            line.Color = color == default ? OxyColors.Black : color;
            line.LineStyle = LineStyle.LongDashDot;
            line.StrokeThickness = 1;
            return line;
        }
    }
}
