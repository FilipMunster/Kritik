using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public partial class ShaftScheme : INotifyPropertyChanged, ICloneable
    {
        private ShaftElementForDataGrid lastSelectedElement;
        public ShaftScheme(Shaft shaft, double schemeWidth)
        {
            Shaft = shaft;
            SchemeWidth = schemeWidth;
            Draw(null);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Draw(null);
        }

        private void OnElementsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Scheme));
        }
        public object Clone()
        {
            ShaftScheme shaftScheme = new ShaftScheme((Shaft)Shaft.Clone(), 0);
            shaftScheme.XMin = XMin;
            shaftScheme.XMax = XMax;
            return shaftScheme;
        }

        private Shaft shaft;
        public Shaft Shaft
        {
            get => shaft;
            private set
            {
                shaft = value;
                shaft.Elements.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnElementsChanged);
            }
        }
        public PlotModel Scheme { get; private set; }

        private double schemeWidth;
        public double SchemeWidth
        {
            get => schemeWidth;
            set
            {
                schemeWidth = value;
                Draw(null);
            }
        }

        public double XMin { get; private set; }
        public double XMax { get; private set; }

        /// <summary>
        /// Set new reference to <see cref="Kritik.Shaft"/> object
        /// </summary>
        /// <param name="shaft"></param>
        public void SetShaft(Shaft shaft, ShaftElementForDataGrid selectedElement)
        {
            Shaft = shaft;
            Draw(selectedElement);
        }
        public void Draw(ShaftElementForDataGrid selectedElement)
        {
            Scheme = new PlotModel();
            Scheme.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false, IsPanEnabled = false });
            Scheme.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false, IsPanEnabled = false });
            Scheme.Padding = new OxyThickness(0);
            Scheme.PlotAreaBorderThickness = new OxyThickness(0);
            Scheme.Background = OxyColors.White;

            if (shaft?.Elements?.Count == 0)
                return;

            if (selectedElement is null)
                selectedElement = this.lastSelectedElement;
            this.lastSelectedElement = selectedElement;

            double maxD = Shaft.Elements.Max(x => x.De);
            double maxM = Shaft.Elements.Max(x => x.M);
            double rigidDiameter = maxD > 0 ? maxD * 0.25 : 500;

            LineSeries selectedItemLine = new LineSeries();

            List<double> xCoordinates = new List<double> { 0 };
            foreach (ShaftElement element in Shaft.Elements) 
            {
                xCoordinates.Add(xCoordinates.Last() + element.L); 
            }

            double xMin = 0;
            double xMax = xCoordinates.Last();
            LineSeries selectedElementLine = new();

            Scheme.Series.Add(Lines.GetAxisLine(new double[] { 0, xCoordinates.Last()} ));

            for (int i = 0; i < Shaft.Elements.Count; i++)
            {
                double x = xCoordinates[i];
                ShaftElementForDataGrid element = Shaft.Elements[i];

                LineSeries line = new LineSeries();
                switch (element.Type)
                {
                    case ElementType.beam:
                        line = Lines.GetBeam(x, element.L, element.De, element.Di);
                        break;
                    case ElementType.beamPlus:
                        {
                            line = Lines.GetBeam(x, element.L, element.De, element.Di);
                            List<LineSeries> beamPlusLines = Lines.GetBeamPlus(x, element, ref xMin, ref xMax, maxM, maxD, rigidDiameter, SchemeWidth);
                            foreach (LineSeries l in beamPlusLines)
                            {
                                Scheme.Series.Add(l);
                            }
                            break;
                        }
                    case ElementType.rigid:
                        line = Lines.GetRigid(x, element.L, rigidDiameter);
                        Scheme.Series.Add(Lines.GetRigidFiller(x, element.L, rigidDiameter));
                        break;
                    case ElementType.disc:
                        line = Lines.GetDisk(x, element.M, maxM, maxD);
                        Scheme.Series.Add(Lines.GetDiskBorder(x, maxD, ref xMin, ref xMax));
                        break;
                    case ElementType.support:
                        line = Lines.GetSupport(x, Shaft.Elements, i, rigidDiameter);
                        Scheme.Series.Add(Lines.GetSupportBorder(x, shaft.Elements, i, rigidDiameter, maxD, ref xMin, ref xMax));
                        break;
                    case ElementType.magnet:
                        line = Lines.GetMagnet(x, Shaft.Elements, i, rigidDiameter);
                        break;
                }

                line.Color = OxyColors.Black;
                line.MarkerStroke = OxyColors.Black;
                line.LineStyle = LineStyle.Solid;

                if (element == selectedElement)
                {
                    selectedElementLine = line;
                    continue;
                }

                Scheme.Series.Add(line);
            }

            selectedElementLine.Color = OxyColors.Red;
            selectedElementLine.MarkerStroke = OxyColors.Red;
            Scheme.Series.Add(selectedElementLine);

            XMin = xMin;
            XMax = xMax;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scheme)));
        }
    }
}
