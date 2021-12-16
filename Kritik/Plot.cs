using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;

namespace Kritik
{
    internal static class Plot
    {
        /// <summary>
        /// Maximální x-ová souřadnice na schématu hřídele [mm]
        /// </summary>
        public static double xMax;
        /// <summary>
        /// Minimální x-ová souřadnice na schématu hřídele [mm]
        /// </summary>
        public static double xMin;
        public static PlotModel NewVelky()
        {
            PlotModel model = new();
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Dot });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
            model.Padding = new OxyThickness(4);
            model.PlotAreaBorderThickness = new OxyThickness(0, 0, 0, 1);

            // Přidám neviditelnou čáru dlouho stejně jako je schéma hřídele, aby lícovaly oba modely vykreslené pod sebou
            LineSeries line = new LineSeries();
            line.Points.Add(new DataPoint(xMin / 1000, 0));
            line.Points.Add(new DataPoint(xMax / 1000, 0));
            line.Color = OxyColors.Transparent;
            model.Series.Add(line);

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
            //line.MouseDown += Line_MouseDown; // + použítí tagu
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

        public static PlotModel SchemaHridele(ObservableCollection<Hridel.PrvekTab> prvky, Hridel.PrvekTab oznacenyRadek)
        {
            PlotModel model = new();
            if (prvky != null && prvky.Count > 0)
            {                
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false });
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
                model.Padding = new OxyThickness(0);
                model.PlotAreaBorderThickness = new OxyThickness(0);

                double maxD = prvky.Max(x => x.De);
                double maxM = prvky.Max(x => x.M);
                double dTuhy = maxD * 0.2;
                LineSeries oznacenyPrvek = new();
                List<double> xovePozice = new List<double> { 0 };
                foreach (Hridel.PrvekTab p in prvky.SkipLast(1)) { xovePozice.Add(xovePozice.Last() + p.L); }
                xMin = 0;
                xMax = xovePozice.Last();

                model.Series.Add(NewOsa(xovePozice.ToArray()));

                for (int i = 0; i < prvky.Count; i++)
                {
                    double x = xovePozice[i];
                    Hridel.PrvekTab p = prvky[i];
                    LineSeries line = new();
                    switch (p.Typ)
                    {
                        case Hridel.beamKeyword:
                            line = KreslitHridel(x, p.L, p.De);
                            break;
                        case Hridel.beamPlusKeyword:
                            line = KreslitHridelPlus(x, p.L, p.De);
                            break;
                        case Hridel.rigidKeyword:
                            line = KreslitTuhy(x, p.L, dTuhy);
                            break;
                        case Hridel.diskKeyword:
                            line = KreslitDisk(x, p.M, maxM, maxD);
                            break;
                        case Hridel.springKeyword:
                            if (prvky[i > 0 ? i - 1 : i + 1].Typ != Hridel.rigidKeyword) 
                            {
                                line = KreslitPodporu(x, prvky[i > 0 ? i - 1 : i + 1].De, maxD);
                            } else { line = KreslitPodporu(x, dTuhy, maxD); }            // vyjímka pro tuhý prvek, protože nemá D                
                            break;
                        case Hridel.magnetKeyword:
                            if (prvky[i > 0 ? i - 1 : i + 1].Typ != Hridel.rigidKeyword)
                            {
                                line = KreslitMagnet(x, prvky[i > 0 ? i - 1 : i + 1].De, maxD);
                            }
                            else { line = KreslitMagnet(x, dTuhy, maxD); }
                            break;
                    }

                    line.Color = p == oznacenyRadek ? OxyColors.Red : OxyColors.Black;
                    line.LineStyle = LineStyle.Solid;

                    if (p != oznacenyRadek) { model.Series.Add(line); } else { oznacenyPrvek = line; }

                    if (p.Typ == Hridel.diskKeyword) // pokud kreslím disk, nakreslím kolem něho ještě průhledný rámeček
                    {
                        model.Series.Add(KreslitDiskRamecek(x, p.M, maxM, maxD));
                    }
                }
                model.Series.Add(oznacenyPrvek);
            }
            return model;
        }

        private static LineSeries KreslitHridel(double x, double L, double D)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x + L, D / 2));
            line.Points.Add(new DataPoint(x + L, -D / 2));
            line.Points.Add(new DataPoint(x, -D / 2));
            line.Points.Add(new DataPoint(x, D / 2));
            return line;
        }
        private static LineSeries KreslitHridelPlus(double x, double L, double D)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x + L, D / 2));
            line.Points.Add(new DataPoint(x + L, -D / 2));
            line.Points.Add(new DataPoint(x, -D / 2));
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x + L, -D / 2));
            return line;
        }
        private static LineSeries KreslitTuhy(double x, double L, double D)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x + L, D / 2));
            line.Points.Add(new DataPoint(x + L, -D / 2));
            line.Points.Add(new DataPoint(x, -D / 2));
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x + L, -D / 2));
            line.Points.Add(new DataPoint(x + L, D / 2));
            line.Points.Add(new DataPoint(x, -D / 2));
            return line;
        }
        private static LineSeries KreslitDisk(double x, double M, double maxM, double maxD)
        {
            LineSeries line = new();
            int markerMin = 5;
            int markerMax = 15;
            line.Points.Add(new DataPoint(x, -maxD / 2 * 1.1));
            line.Points.Add(new DataPoint(x, maxD / 2 * 1.1));
            line.MarkerType = MarkerType.Circle;
            line.MarkerSize = (markerMax - markerMin) / maxM * M + markerMin;
            line.MarkerStroke = OxyColors.Black;
            line.MarkerFill = OxyColors.SteelBlue;
            return line;
        }
        private static LineSeries KreslitDiskRamecek(double x, double M, double maxM, double maxD)
        {
            LineSeries line = new();
            double xMinus = x - maxD * 0.2;
            double xPlus = x + maxD * 0.2;
            line.Points.Add(new DataPoint(xMinus, -maxD / 2 * 1.4));
            line.Points.Add(new DataPoint(xPlus, maxD / 2 * 1.4));
            line.Color = OxyColors.Transparent;
            if (xMinus < xMin) { xMin = xMinus; }
            if (xPlus > xMax) { xMax = xPlus; }
            return line;
        }
        private static LineSeries KreslitPodporu(double x, double D, double maxD)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(x, -D / 2 ));
            line.Points.Add(new DataPoint(x, -D / 2 - maxD * 0.2));
            return line;
        }
        private static LineSeries KreslitMagnet(double x, double D, double maxD)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(x, -D / 2));
            line.Points.Add(new DataPoint(x, -D / 2 - maxD * 0.2));
            line.Points.Add(new DataPoint(x + maxD*0.1, -D / 2 - maxD * 0.2));
            return line;
        }
    }
}
