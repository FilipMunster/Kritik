using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kritik
{
    public partial class ShaftScheme
    {
        private static class Lines
        {
            /// An equilateral triangle with vertex at a given point
            private static readonly ScreenPoint[] customTriangle = new ScreenPoint[] {
                new ScreenPoint(0, 0), new ScreenPoint(-0.5, 0.866), new ScreenPoint(0.5, 0.866), new ScreenPoint(0, 0) };

            public static LineSeries GetAxisLine(double[] x, OxyColor color = default)
            {
                LineSeries line = new();
                line.Points.Add(new DataPoint(0, 0));
                line.Points.Add(new DataPoint(x[^1], 0));
                line.Color = color == default ? OxyColors.Black : color;
                line.LineStyle = LineStyle.LongDashDot;
                line.StrokeThickness = 1;
                return line;
            }

            public static LineSeries GetBeam(double x, double L, double D, double Di)
            {
                LineSeries line = new();
                line.Points.Add(new DataPoint(x, D / 2));
                line.Points.Add(new DataPoint(x + L, D / 2));
                line.Points.Add(new DataPoint(x + L, -D / 2));
                line.Points.Add(new DataPoint(x, -D / 2));
                line.Points.Add(new DataPoint(x, D / 2));

                if (Di > 0)
                {
                    LineSeries lineInner = GetBeam(x, L, Di, 0);
                    foreach (DataPoint p in lineInner.Points)
                    {
                        line.Points.Add(p);
                    }
                }
                return line;
            }
            public static List<LineSeries> GetBeamPlus(double x, ShaftElementForDataGrid element, ref double xMin, ref double xMax,
                double maxM, double maxD, double rigidDiameter, double modelWidth)
            {
                List<LineSeries> lines = new();
                ObservableCollection<ShaftElementForDataGrid> elements = new();
                elements.Add(element);
                elements.Add(element);

                // Making up the number of elements to draw so it looks good:
                double Ltx = 0.0369; // (triangle width) / (model width)
                double Lt = Ltx * modelWidth + 5; // triangle width [px] + space [px]
                Lt = Lt * xMax / (modelWidth - modelWidth * 0.1); // triangle width [mm] -(minus) space between shaft end and the model border (approx.)
                double division = Math.Floor(element.L / Lt);
                division = element.Division < division ? element.Division : division;

                double dx = element.L / (division + 1);

                for (int i = 0; i < division; i++)
                {
                    x += dx;
                    if (element.M > 0)
                    {
                        lines.Add(GetDisk(x, element.M / division, maxM, maxD));
                        lines.Add(GetDiskBorder(x, maxD, ref xMin, ref xMax));
                    }
                    else if (element.K > 0)
                    {
                        lines.Add(GetSupport(x, elements, 0, rigidDiameter));
                        lines.Add(GetSupportBorder(x, elements, 0, rigidDiameter, maxD, ref xMin, ref xMax));
                    }
                }
                return lines;
            }
            public static LineSeries GetRigid(double x, double L, double D)
            {
                LineSeries line = new();
                line.Points.Add(new DataPoint(x, D / 2));
                line.Points.Add(new DataPoint(x + L, D / 2));
                line.Points.Add(new DataPoint(x + L, -D / 2));
                line.Points.Add(new DataPoint(x, -D / 2));
                line.Points.Add(new DataPoint(x, D / 2));
                return line;
            }
            public static LineSeries GetRigidFiller(double x, double L, double D)
            {
                LineSeries line = new();
                double dD = D / 6;
                double y = -D / 2 + dD;
                int sgn = 0;
                while (y < D / 2)
                {
                    line.Points.Add(new DataPoint(x + sgn * L, y));
                    sgn = sgn == 1 ? 0 : 1;
                    line.Points.Add(new DataPoint(x + sgn * L, y));
                    y += dD;
                }

                line.StrokeThickness = 1;
                line.Color = OxyColors.Black;
                return line;
            }
            public static LineSeries GetDisk(double x, double M, double maxM, double maxD)
            {
                LineSeries line = new();
                int markerMin = 4;
                int markerMax = 12;
                line.Points.Add(new DataPoint(x, -maxD / 2 * 1.4));
                line.Points.Add(new DataPoint(x, maxD / 2 * 1.4));
                line.MarkerType = MarkerType.Circle;
                line.MarkerSize = (markerMax - markerMin) / maxM * M + markerMin;
                line.MarkerStroke = OxyColors.Black;
                line.MarkerFill = OxyColors.SteelBlue;
                line.Color = OxyColors.Black;
                return line;
            }
            public static LineSeries GetDiskBorder(double x, double maxD, ref double xMin, ref double xMax)
            {
                LineSeries line = new();
                double xMinus = x - maxD * 0.17;
                double xPlus = x + maxD * 0.17;
                line.Points.Add(new DataPoint(xMinus, -maxD / 2 * 1.72));
                line.Points.Add(new DataPoint(xPlus, maxD / 2 * 1.72));
                line.Color = OxyColors.Transparent;
                if (xMinus < xMin) { xMin = xMinus; }
                if (xPlus > xMax) { xMax = xPlus; }
                return line;
            }

            public static LineSeries GetSupport(double x, ObservableCollection<ShaftElementForDataGrid> elements, int i, double rigidDiameter)
            {
                double D = MaxDAtIndex(elements, i, rigidDiameter);

                LineSeries line = new();
                line.Points.Add(new DataPoint(x, -D / 2));

                line.MarkerOutline = customTriangle;
                line.MarkerType = MarkerType.Custom;
                line.MarkerStroke = OxyColors.Black;
                line.MarkerFill = OxyColors.SteelBlue;
                line.MarkerSize = 25;
                return line;
            }
            public static LineSeries GetSupportBorder(double x, ObservableCollection<ShaftElementForDataGrid> elements, int i, double rigidDiameter, double maxD, ref double xMin, ref double xMax)
            {
                double D = MaxDAtIndex(elements, i, rigidDiameter);

                LineSeries line = new();
                double xMinus = x - (xMax * 0.02);
                double xPlus = x + (xMax * 0.02);
                line.Points.Add(new DataPoint(xMinus, -D / 2));

                if (elements.Any(x => x.Type == ElementType.disc))
                {
                    line.Points.Add(new DataPoint(xPlus, -D / 2 - maxD * 0.28));
                }
                else
                {
                    line.Points.Add(new DataPoint(xPlus, -D / 2 - maxD * 0.18));
                }

                line.Color = OxyColors.Transparent;
                if (xMinus < xMin) { xMin = xMinus; }
                if (xPlus > xMax) { xMax = xPlus; }
                return line;
            }

            public static LineSeries GetMagnet(double x, ObservableCollection<ShaftElementForDataGrid> elements, int i, double rigidDiameter)
            {
                double D = MaxDAtIndex(elements, i, rigidDiameter);

                LineSeries line = new();
                line.Points.Add(new DataPoint(x, D / 2));
                line.Points.Add(new DataPoint(x, -D / 2));
                return line;
            }

            private static double MaxDAtIndex(ObservableCollection<ShaftElementForDataGrid> elements, int i, double rigidDiameter)
            {
                double dLeft = 0;
                double dRight = 0;
                if (i > 0)
                {
                    dLeft = elements[i - 1].Type == ElementType.rigid ? rigidDiameter : elements[i - 1].De;
                }
                if (i < elements.Count - 1)
                {
                    dRight = elements[i + 1].Type == ElementType.rigid ? rigidDiameter : elements[i + 1].De;
                }

                return Math.Max(dLeft, dRight);
            }
        }
    }
}
