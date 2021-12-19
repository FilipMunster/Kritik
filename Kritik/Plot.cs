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

        // Rovnostraný trojúhelník s vrcholem v zadaném bodě
        private static readonly ScreenPoint[] customTriangle = new ScreenPoint[] { new ScreenPoint(0, 0), new ScreenPoint(-0.5, 0.866), new ScreenPoint(0.5, 0.866), new ScreenPoint(0, 0) };
        public static PlotModel NewVelky()
        {
            PlotModel model = new();
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, 
                MajorGridlineStyle = LineStyle.Dot, 
                IsZoomEnabled = false,
                IsPanEnabled = false});
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

        public static double dTuhy;
        public static PlotModel SchemaHridele(ObservableCollection<Hridel.PrvekTab> prvky, Hridel.PrvekTab oznacenyRadek, double sirkaModelu)
        {
            PlotModel model = new();
            if (prvky != null && prvky.Count > 0)
            {                
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false, IsPanEnabled = false });
                model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false, IsPanEnabled = false });
                model.Padding = new OxyThickness(0);
                model.PlotAreaBorderThickness = new OxyThickness(0);

                double maxD = prvky.Max(x => x.De);
                double maxM = prvky.Max(x => x.M);
                dTuhy = maxD > 0 ? maxD * 0.25 : 500;
                LineSeries oznacenyPrvek = new();
                List<double> xovePozice = new List<double> { 0 };
                foreach (Hridel.PrvekTab p in prvky) { xovePozice.Add(xovePozice.Last() + p.L); }
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
                            line = KreslitHridel(x, p.L, p.De, p.Di);
                            break;
                        case Hridel.beamPlusKeyword:
                            line = KreslitHridel(x, p.L, p.De, p.Di);
                            break;
                        case Hridel.rigidKeyword:
                            line = KreslitTuhy(x, p.L, dTuhy);
                            break;
                        case Hridel.diskKeyword:
                            line = KreslitDisk(x, p.M, maxM, maxD);
                            line.Tag = i;
                            line.MouseDown += (s, e) => { MainWindow.AppWindow.DiskPruzinaMouseDown(s, e); };
                            break;
                        case Hridel.springKeyword:
                            line = KreslitPodporu(x, prvky, i, dTuhy);
                            line.Tag = i;
                            line.MouseDown += (s, e) => { MainWindow.AppWindow.DiskPruzinaMouseDown(s, e); };
                            break;
                        case Hridel.magnetKeyword:
                            line = KreslitMagnet(x, prvky, i, dTuhy);
                            break;
                    }

                    line.Color = p == oznacenyRadek ? OxyColors.Red : OxyColors.Black;
                    line.MarkerStroke = p == oznacenyRadek ? OxyColors.Red : OxyColors.Black;
                    line.LineStyle = LineStyle.Solid;

                    if (p != oznacenyRadek) { model.Series.Add(line); } else { oznacenyPrvek = line; }

                    // Vykreslení průhledných rámečků u disku a podpory:
                    if (p.Typ == Hridel.diskKeyword) { model.Series.Add(KreslitDiskRamecek(x, maxD)); }
                    if (p.Typ == Hridel.springKeyword) { model.Series.Add(KreslitPodporuRamecek(x, prvky, i, dTuhy, maxD, xMax)); }
                    // Vykreslit výplň tuhého prvku
                    if (p.Typ == Hridel.rigidKeyword) { model.Series.Add(KreslitTuhyVypln(x, p.L, dTuhy)); }
                    // Vykreslit prvky na hřídeli+
                    if (p.Typ == Hridel.beamPlusKeyword) 
                    {
                        foreach (LineSeries l in KreslitHridelPlus(x, p, xMax, maxM, maxD, dTuhy, sirkaModelu)) { model.Series.Add(l); }
                    }
                }
                model.Series.Add(oznacenyPrvek);
            }
            return model;
        }

        private static void Line_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static LineSeries KreslitHridel(double x, double L, double D, double vrtani)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x + L, D / 2));
            line.Points.Add(new DataPoint(x + L, -D / 2));
            line.Points.Add(new DataPoint(x, -D / 2));
            line.Points.Add(new DataPoint(x, D / 2));

            if (vrtani > 0) {
                LineSeries lineVrt = KreslitHridel(x, L, vrtani, 0);
                foreach (DataPoint p in lineVrt.Points) { line.Points.Add(p); }
            }
            return line;
        }
        private static List<LineSeries> KreslitHridelPlus(double x, Hridel.PrvekTab p, double xMax, double maxM, double maxD, double dTuhy, double sirkaModelu)
        {
            List<LineSeries> lines = new();
            ObservableCollection<Hridel.PrvekTab> prvky = new(); prvky.Add(p); prvky.Add(p);

            // Vykouzlení počtu vykreslených prvků tak, aby to vypadlo dobře:
            double Ltx = 0.0369; // šířka trojúhelníku ku šířce modelu
            double Lt = Ltx * sirkaModelu + 5; // šířka trojúhelníku v [px] + mezera [px]
            Lt = Lt * xMax / (sirkaModelu - sirkaModelu * 0.1); // šířka trojúhelníku v [mm] -(minus) zhruba okraj konec hřídele k okraji Modelu
            double deleni = Math.Floor(p.L / Lt);
            deleni = p.Deleni < deleni ? p.Deleni : deleni;

            double dx = p.L / (deleni + 1);

            for (int i = 0; i < deleni; i++)
            {
                x += dx;
                if (p.M > 0)
                {
                    lines.Add(KreslitDisk(x, p.M / deleni, maxM, maxD));
                    lines.Add(KreslitDiskRamecek(x, maxD));
                }
                else if (p.K > 0)
                {
                    lines.Add(KreslitPodporu(x, prvky, 0, dTuhy));
                    lines.Add(KreslitPodporuRamecek(x, prvky, 0, dTuhy, maxD, xMax));
                }
            }
            return lines;
        }
        private static LineSeries KreslitTuhy(double x, double L, double D)
        {
            LineSeries line = new();
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x + L, D / 2));
            line.Points.Add(new DataPoint(x + L, -D / 2));
            line.Points.Add(new DataPoint(x, -D / 2));
            line.Points.Add(new DataPoint(x, D / 2));
            return line;
        }
        private static LineSeries KreslitTuhyVypln(double x, double L, double D)
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
        private static LineSeries KreslitDisk(double x, double M, double maxM, double maxD)
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
        private static LineSeries KreslitDiskRamecek(double x, double maxD)
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

        private static LineSeries KreslitPodporu(double x, ObservableCollection<Hridel.PrvekTab> p, int i, double dTuhy)
        {
            double dLevy;
            double dPravy;
            if (i > 0) { dLevy = p[i - 1].Typ == Hridel.rigidKeyword? dTuhy : p[i - 1].De; } else { dLevy = 0; }
            if (i < p.Count - 1) { dPravy = p[i + 1].Typ == Hridel.rigidKeyword ? dTuhy : p[i + 1].De; } else { dPravy = 0; }
            double D = Math.Max(dLevy, dPravy);

            LineSeries line = new();
            line.Points.Add(new DataPoint(x, -D / 2));

            line.MarkerOutline = customTriangle;
            line.MarkerType = MarkerType.Custom;
            line.MarkerStroke = OxyColors.Black;
            line.MarkerFill = OxyColors.SteelBlue;
            line.MarkerSize = 25;
            return line;
        }
        private static LineSeries KreslitPodporuRamecek(double x, ObservableCollection<Hridel.PrvekTab> p, int i, double dTuhy, double maxD, double L)
        {
            double dLevy;
            double dPravy;
            if (i > 0) { dLevy = p[i - 1].Typ == Hridel.rigidKeyword ? dTuhy : p[i - 1].De; } else { dLevy = 0; }
            if (i < p.Count - 1) { dPravy = p[i + 1].Typ == Hridel.rigidKeyword ? dTuhy : p[i + 1].De; } else { dPravy = 0; }
            double D = Math.Max(dLevy, dPravy);

            LineSeries line = new();
            double xMinus = x - L * 0.02;
            double xPlus = x + L * 0.02;
            line.Points.Add(new DataPoint(xMinus, -D / 2));

            if (p.Any(x => x.Typ == Hridel.diskKeyword)) { line.Points.Add(new DataPoint(xPlus, -D / 2 - maxD * 0.28)); }
            else { line.Points.Add(new DataPoint(xPlus, -D / 2 - maxD * 0.18)); }
            line.Color = OxyColors.Transparent;
            if (xMinus < xMin) { xMin = xMinus; }
            if (xPlus > xMax) { xMax = xPlus; }
            return line;
        }

        private static LineSeries KreslitMagnet(double x, ObservableCollection<Hridel.PrvekTab> p, int i, double dTuhy)
        {
            double dLevy;
            double dPravy;
            if (i > 0) { dLevy = p[i - 1].Typ == Hridel.rigidKeyword ? dTuhy : p[i - 1].De; } else { dLevy = 0; }
            if (i < p.Count - 1) { dPravy = p[i + 1].Typ == Hridel.rigidKeyword ? dTuhy : p[i + 1].De; } else { dPravy = 0; }
            double D = Math.Max(dLevy, dPravy);

            LineSeries line = new();
            line.Points.Add(new DataPoint(x, D / 2));
            line.Points.Add(new DataPoint(x, -D / 2));
            return line;
        }
    }
}
