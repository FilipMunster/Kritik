using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kritik
{
    public partial class ShaftScheme : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// Last selected shaft element
        /// </summary>
        private ShaftElementForDataGrid lastSelectedElement;

        /// <summary>
        /// Diameter of rigid element drawn
        /// </summary>
        private double rigidDiameter;
        public ShaftScheme(Shaft shaft, double schemeWidth)
        {
            Shaft = shaft;
            SchemeWidth = schemeWidth;
            Draw(null);
        }

        public delegate void SchemeMouseDownEventHandler(object sender, int elementId);
        public event SchemeMouseDownEventHandler SchemeMouseDown;

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
            ShaftScheme shaftScheme = (ShaftScheme)this.MemberwiseClone();
            shaftScheme.lastSelectedElement = null;
            shaftScheme.SetShaft((Shaft)Shaft.Clone(), null);
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
        /// <summary>
        /// Value of scheme PlotModel PlotView.ActualWidth. Needs to be updated from viewmodel.
        /// </summary>
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

        private List<double> xCoordinates;
        /// <summary>
        /// X-Coordinates of nodes between elements
        /// </summary>
        public List<double> XCoordinates
        {
            get => xCoordinates;
            private set => xCoordinates = value;
        }

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
            Scheme.MouseDown += Scheme_MouseDown;

            if (Shaft?.Elements?.Count == 0)
                return;

            if (selectedElement is null)
                selectedElement = this.lastSelectedElement;
            else
                this.lastSelectedElement = selectedElement;

            double maxD = Shaft.Elements.Max(x => x.De);
            double maxM = Shaft.Elements.Max(x => x.M);
            rigidDiameter = maxD > 0 ? maxD * 0.25 : 500;

            LineSeries selectedItemLine = new LineSeries();

            XCoordinates = new List<double> { 0 };
            foreach (ShaftElement element in Shaft.Elements)
            {
                XCoordinates.Add(XCoordinates.Last() + element.L);
            }

            double xMin = 0;
            double xMax = XCoordinates.Last();
            LineSeries selectedElementLine = new();

            Scheme.Series.Add(Lines.GetAxisLine(new double[] { 0, XCoordinates.Last() }));

            for (int i = 0; i < Shaft.Elements.Count; i++)
            {
                double x = XCoordinates[i];
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
                        line.MouseDown += Line_MouseUp;
                        line.Tag = i;
                        Scheme.Series.Add(Lines.GetDiskBorder(x, maxD, ref xMin, ref xMax));
                        break;
                    case ElementType.support:
                        line = Lines.GetSupport(x, Shaft.Elements, i, rigidDiameter);
                        line.MouseDown += Line_MouseUp;
                        line.Tag = i;
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

        private void Scheme_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (Shaft.Elements.Count == 0)
                return;
            
            OxyPlot.ElementCollection<OxyPlot.Axes.Axis> axisList = Scheme.Axes;
            OxyPlot.Axes.Axis xAxis = axisList.FirstOrDefault(ax => ax.Position == OxyPlot.Axes.AxisPosition.Bottom);
            OxyPlot.Axes.Axis yAxis = axisList.FirstOrDefault(ax => ax.Position == OxyPlot.Axes.AxisPosition.Left);

            DataPoint position = OxyPlot.Axes.Axis.InverseTransform(e.Position, xAxis, yAxis);

            int elementId = -1;
            for (int i = 0; i < (XCoordinates.Count - 1); i++)
            {
                if (position.X > XCoordinates[i] && position.X < XCoordinates[i + 1])
                {
                    elementId = i;
                    break;
                }
            }

            if (elementId < 0)
                return;

            double elementDe = Shaft.Elements[elementId].Type == ElementType.rigid ? rigidDiameter : Shaft.Elements[elementId].De;
            if (Math.Abs(position.Y) <= (elementDe / 2.0))
            {
                SchemeMouseDown?.Invoke(this, elementId);
            }
        }

        private void Line_MouseUp(object sender, OxyMouseEventArgs e)
        {
            if (((Series)sender).Tag is int elementId)
                SchemeMouseDown?.Invoke(this, elementId);
        }
    }
}
