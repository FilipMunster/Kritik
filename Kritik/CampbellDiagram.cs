using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kritik
{
    public class CampbellDiagram
    {
        private readonly KritikCalculation calculation;
        private readonly double maxRpm;
        private readonly double rpmStep;
        private readonly Strings strings;

        private List<Precession> forwardPrecessions = new List<Precession>();
        private List<Precession> backwardPrecessions = new List<Precession>();

        public CampbellDiagram(KritikCalculation kritikCalculation, double maxRpm, int rpmDivision, Strings strings)
        {
            this.calculation = kritikCalculation;
            this.maxRpm = maxRpm;
            this.rpmStep = maxRpm / rpmDivision;
            this.strings = strings;
        }

        public double MaxCriticalSpeed
        {
            get
            {
                double[] forwardCriticalSpeeds = new double[] { 0 };
                double[] backwardCriticalSpeeds = new double[] { 0 };
                if (forwardPrecessions.Count > 0)
                    (_, forwardCriticalSpeeds) = forwardPrecessions[^1].GetValues();
                if (backwardPrecessions.Count > 0)
                    (_, backwardCriticalSpeeds) = backwardPrecessions[^1].GetValues();
                return Math.Max(forwardCriticalSpeeds.Max(), backwardCriticalSpeeds.Max());
            }
        }

        public Precession[] ForwardPrecessions => this.forwardPrecessions.ToArray();
        public Precession[] BackwardPrecessions => this.backwardPrecessions.ToArray();

        public async Task CreateDiagramAsync(IProgress<int> progress, CancellationToken cancellationToken, bool createForwardPrecession, bool createBackwardPrecession)
        {
            calculation.Shaft.Properties.ShaftRotationInfluence = true;

            double rpm = 0;
            int i = 0;
            while (rpm <= this.maxRpm)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    break;
                }                

                calculation.Shaft.Properties.ShaftRPM = rpm;

                if (createForwardPrecession)
                {
                    calculation.Shaft.Properties.Gyros = GyroscopicEffect.forward;
                    await calculation.CalculateCriticalSpeedsAsync();
                    AddToPrecession(calculation.Shaft.Properties.Gyros, rpm, calculation.CriticalSpeeds);
                }
                
                if (createBackwardPrecession)
                {
                    calculation.Shaft.Properties.Gyros = GyroscopicEffect.backward;
                    await calculation.CalculateCriticalSpeedsAsync();
                    AddToPrecession(calculation.Shaft.Properties.Gyros, rpm, calculation.CriticalSpeeds);
                }                

                progress.Report(++i);
                rpm += this.rpmStep;
            }
        }

        private void AddToPrecession(GyroscopicEffect gyroscopicEffect, double shaftRpm, double[] criticalSpeeds)
        {
            List<Precession> list;
            switch (gyroscopicEffect)
            {
                case GyroscopicEffect.forward:
                    list = this.forwardPrecessions;
                    break;
                case GyroscopicEffect.backward:
                    list = this.backwardPrecessions;
                    break;
                default:
                    throw new ArgumentException();
            }

            while (list.Count < criticalSpeeds.Length)
            {
                list.Add(new Precession(gyroscopicEffect, list.Count));
            }

            for (int i = 0; i < criticalSpeeds.Length; i++)
            {
                list[i].Add(new CampbellItem(shaftRpm, criticalSpeeds[i]));
            }
        }

        public PlotModel GetPlotModel()
        {
            if (this.forwardPrecessions.Count == 0 && this.backwardPrecessions.Count == 0)
                return new PlotModel();

            PlotModel model = new PlotModel();
            UpdateModelData(model);

            return model;
        }

        public void UpdateModelData(PlotModel plotModel)
        {
            plotModel.Axes.Clear();
            plotModel.Series.Clear();

            plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Dot,
                Title = strings.OtackyRotoru + " (rpm)",
                Font = "Calibri",
                FontSize = 13,
                Minimum = 0
            });

            plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Dot,
                Title = strings.KritickeOtacky + " (rpm)",
                Font = "Calibri",
                FontSize = 13,
                Minimum = 0
            });

            LineSeries axis = new LineSeries();
            axis.Points.Add(new DataPoint(0, 0));
            axis.Points.Add(new DataPoint(this.maxRpm, this.maxRpm));
            axis.LineStyle = LineStyle.DashDot;
            axis.Color = OxyColors.Gray;
            axis.StrokeThickness = 1.5;
            plotModel.Series.Add(axis);

            foreach (Precession item in this.forwardPrecessions)
            {
                LineSeries line = item.ToLineSeries();
                line.Color = OxyColors.Black;
                line.Title = item.PrecessionType.ToString();
                plotModel.Series.Add(line);
            }
            foreach (Precession item in this.backwardPrecessions)
            {
                LineSeries line = item.ToLineSeries();
                line.Color = this.forwardPrecessions.Count > 0 ? OxyColors.SteelBlue : OxyColors.Black;
                plotModel.Series.Add(line);
            }
            plotModel.InvalidatePlot(true);
        }

        public class Precession
        {
            private readonly List<CampbellItem> items = new List<CampbellItem>();
            public Precession(GyroscopicEffect precessionType, int precessionNumber)
            {
                PrecessionType = precessionType;
                PrecessionNumber = precessionNumber;
            }

            public GyroscopicEffect PrecessionType { get; }
            public int PrecessionNumber { get; }

            public void Add(CampbellItem campbellItem)
            {
                items.Add(campbellItem);
            }

            public LineSeries ToLineSeries()
            {
                LineSeries lineSeries = new LineSeries();
                foreach (CampbellItem item in items)
                {
                    lineSeries.Points.Add(item.ToDataPoint());
                }
                return lineSeries;
            }

            public (double[] rotorSpeeds, double[] criticalSpeeds) GetValues()
            {
                List<double> x = new List<double>(items.Count);
                List<double> y = new List<double>(items.Count);

                foreach (CampbellItem item in items)
                {
                    x.Add(item.ShaftRpm);
                    y.Add(item.CriticalSpeed);
                }
                return (x.ToArray(), y.ToArray());
            }
        }
        
        public struct CampbellItem
        {
            public CampbellItem(double shaftRpm, double criticalSpeed)
            {
                ShaftRpm = shaftRpm;
                CriticalSpeed = criticalSpeed;
            }
            public double ShaftRpm { get; }
            public double CriticalSpeed { get; }
            public DataPoint ToDataPoint()
            {
                return new DataPoint(ShaftRpm, CriticalSpeed);
            }
        }
    }
}

