using OxyPlot;
using OxyPlot.Series;

namespace Kritik
{
    public partial class OscillationShapesViewModel
    {
        private static class Plotter
        {
            public static PlotModel GetMainPlotModel()
            {
                PlotModel model = new();
                model.Axes.Add(new OxyPlot.Axes.LinearAxis
                {
                    Position = OxyPlot.Axes.AxisPosition.Bottom,
                    IsZoomEnabled = false,
                    IsPanEnabled = false,
                    Title = "x [m]",
                    Font = "Calibri",
                    FontSize = 13
                });
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
                model.Padding = new OxyThickness(0);
                model.PlotAreaBorderThickness = new OxyThickness(0, 0, 0, 1);
                model.Background = OxyColors.White;

                return model;
            }
            public static PlotModel GetThumbnailPlotModel()
            {
                PlotModel model = new();
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false });
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
                model.Padding = new OxyThickness(4);
                model.Background = OxyColors.White;
                return model;
            }

            public static PlotModel ModelFromString(string text, int x, int y, string font, int fontSize)
            {
                PlotModel model = new PlotModel();
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false });
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
                model.Background = OxyColors.White;
                model.PlotAreaBorderThickness = new OxyThickness(0);
                OxyPlot.Annotations.TextAnnotation textAnnotation = new();
                textAnnotation.TextHorizontalAlignment = HorizontalAlignment.Left;
                textAnnotation.Text = text;
                textAnnotation.TextPosition = new DataPoint(x, y);
                textAnnotation.StrokeThickness = 0;
                textAnnotation.Font = font;
                textAnnotation.FontSize = fontSize;
                model.Annotations.Add(textAnnotation);
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
            public static LineSeries NewAxisLine(double[] x, OxyColor color = default)
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
}
