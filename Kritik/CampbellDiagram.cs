using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class CampbellDiagram
    {
        private readonly KritikCalculation calculation;
        private readonly double maxRpm;
        private readonly double rpmStep;

        private List<Precession> forwardPrecessions = new List<Precession>();
        private List<Precession> backwardPrecessions = new List<Precession>();

        public CampbellDiagram(KritikCalculation kritikCalculation, double maxRpm, int rpmDivision)
        {
            this.calculation = kritikCalculation;
            this.maxRpm = maxRpm;
            this.rpmStep = maxRpm / rpmDivision;
        }

        public async Task CreateDiagramAsync(IProgress<int> progress)
        {
            calculation.Shaft.Properties.ShaftRotationInfluence = true;

            double rpm = 0;
            int i = 0;
            while (rpm <= this.maxRpm)
            {
                calculation.Shaft.Properties.ShaftRPM = rpm;

                calculation.Shaft.Properties.Gyros = GyroscopicEffect.forward;
                await calculation.CalculateCriticalSpeedsAsync();
                AddToPrecession(calculation.Shaft.Properties.Gyros, rpm, calculation.CriticalSpeeds);

                calculation.Shaft.Properties.Gyros = GyroscopicEffect.backward;
                await calculation.CalculateCriticalSpeedsAsync();
                AddToPrecession(calculation.Shaft.Properties.Gyros, rpm, calculation.CriticalSpeeds);

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
                return null;

            PlotModel model = new PlotModel();
            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Dot,
                Title = "Shaft speed (RPM)",
                Minimum = 0
            });

            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Dot,
                Title = "Critical speed (RPM)",
                Minimum = 0
            });            

            LineSeries axis = new LineSeries();
            axis.Points.Add(new DataPoint(0, 0));
            axis.Points.Add(new DataPoint(this.maxRpm, this.maxRpm));
            axis.LineStyle = LineStyle.DashDot;
            axis.Color = OxyColors.Gray;
            model.Series.Add(axis);

            foreach (Precession item in this.forwardPrecessions)
            {
                LineSeries line = item.ToLineSeries();
                line.Color = OxyColors.Black;
                line.Title = item.PrecessionType.ToString();
                model.Series.Add(line);
            }
            foreach (Precession item in this.backwardPrecessions)
            {
                LineSeries line = item.ToLineSeries();
                line.Color = OxyColors.SteelBlue;
                model.Series.Add(line);
            }
            return model;
        }

        private class Precession
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
        }
        
        private struct CampbellItem
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

