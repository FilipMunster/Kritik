using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace Kritik
{
    public partial class OscillationShapesViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Array of computed OscillationShapes
        /// </summary>
        private readonly OscillationShapes[] oscillationShapes;

        private readonly CalculationProperties calculationProperties;
        private readonly Shaft shaft;

        /// <summary>
        /// Oscillation shape plotted in MainPlot
        /// </summary>
        private OscillationShapeType mainPlotShape = OscillationShapeType.w;

        private ShaftScheme shaftScheme;

        private readonly Strings strings;

        /// <summary>
        /// Array of line colors used by thumbnail plots
        /// </summary>
        private readonly OxyColor[] colorsArray = { OxyColors.Blue, OxyColors.Red, OxyColors.Tan, OxyColors.Plum };

        public OscillationShapesViewModel(KritikCalculation kritikCalculation,
            ShaftScheme shaftScheme, Strings strings)
        {
            if (kritikCalculation.OscillationShapes is null || kritikCalculation.OscillationShapes.Length == 0)
                throw new ArgumentNullException();

            this.oscillationShapes = kritikCalculation.OscillationShapes;
            this.calculationProperties = kritikCalculation.CalculationProperties;
            this.shaft = kritikCalculation.Shaft;
            this.shaftScheme = shaftScheme;
            this.strings = strings;

            ShapeNumber = 1;
            ShowNodesIsChecked = Properties.Settings.Default.drawNodes;
            ShowGridIsChecked = Properties.Settings.Default.drawGrid;
            SaveSchemeIsChecked = Properties.Settings.Default.drawScheme;
            SaveShapeIsChecked = Properties.Settings.Default.drawShape;
            SaveDescriptionIsChecked = Properties.Settings.Default.drawDescription;
            ThumbnailPlotBorderUpdate();
        }
        public enum OscillationShapeType
        {
            w,
            phi,
            m,
            t
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PlotModel MainPlot => GetMainPlot();
        public PlotModel ThumbnailPlotW => GetThumbnailPlot(OscillationShapeType.w);
        public PlotModel ThumbnailPlotPhi => GetThumbnailPlot(OscillationShapeType.phi);
        public PlotModel ThumbnailPlotM => GetThumbnailPlot(OscillationShapeType.m);
        public PlotModel ThumbnailPlotT => GetThumbnailPlot(OscillationShapeType.t);
        public Brush ThumbnailPlotWBrush { get; private set; }
        public Brush ThumbnailPlotPhiBrush { get; private set; }
        public Brush ThumbnailPlotMBrush { get; private set; }
        public Brush ThumbnailPlotTBrush { get; private set; }

        private int shapeNumber;
        /// <summary>
        /// Number of shape to plot (1-indexed)
        /// </summary>
        public int ShapeNumber
        {
            get => shapeNumber;
            private set
            {
                shapeNumber = value;
                NotifyPropertyChanged(nameof(MainPlot));
            }
        }

        private bool showNodesIsChecked;
        /// <summary>
        /// Indicates if Nodes are plotted
        /// </summary>
        public bool ShowNodesIsChecked
        {
            get => showNodesIsChecked;
            set
            {
                showNodesIsChecked = value;
                Properties.Settings.Default.drawNodes = value;
                NotifyPropertyChanged(nameof(MainPlot));
            }
        }

        private bool showGridIsChecked;
        /// <summary>
        /// Indicates if Grid are plotted
        /// </summary>
        public bool ShowGridIsChecked
        {
            get => showGridIsChecked;
            set
            {
                showGridIsChecked = value;
                Properties.Settings.Default.drawGrid = value;
                NotifyPropertyChanged(nameof(MainPlot));
            }
        }

        private bool saveSchemeIsChecked;
        public bool SaveSchemeIsChecked
        {
            get => saveSchemeIsChecked;
            set
            {
                saveSchemeIsChecked = value;
                Properties.Settings.Default.drawScheme = value;
            }
        }
        private bool saveShapeIsChecked;
        public bool SaveShapeIsChecked
        {
            get => saveShapeIsChecked;
            set
            {
                saveShapeIsChecked = value;
                Properties.Settings.Default.drawShape = value;
            }
        }
        private bool saveDescriptionIsChecked;
        public bool SaveDescriptionIsChecked
        {
            get => saveDescriptionIsChecked;
            set
            {
                saveDescriptionIsChecked = value;
                Properties.Settings.Default.drawDescription = value;
            }
        }

        /// <summary>
        /// Oscillation shape description
        /// </summary>
        public string Description
        {
            get
            {
                string description = "";
                string speed = String.Format("{0:0.000}", oscillationShapes[ShapeNumber - 1].Rpm);
                string gyros = "";
                switch (shaft.Properties.Gyros)
                {
                    case GyroscopicEffect.none:
                        gyros = strings.VlivGyrosNeniUvazovan;
                        break;
                    case GyroscopicEffect.forward:
                        gyros = strings.SoubeznaPrecese;
                        break;
                    case GyroscopicEffect.backward:
                        gyros = strings.ProtibeznaPrecese;
                        break;
                }
                string drawnShape = "";
                switch (mainPlotShape)
                {
                    case OscillationShapeType.w:
                        drawnShape = strings.PruhybHridele + " w";
                        break;
                    case OscillationShapeType.phi:
                        drawnShape = strings.NatoceniHridele + " φ";
                        break;
                    case OscillationShapeType.m:
                        drawnShape = strings.OhybovyMoment + " M";
                        break;
                    case OscillationShapeType.t:
                        drawnShape = strings.PosouvajiciSila + " T";
                        break;
                }
                description += strings.OrdinalNumber(ShapeNumber) + " " + strings.kritickeOtacky + " = " + speed + " min⁻¹\n";
                description += drawnShape + "\n";
                description += gyros + "\n";
                description += calculationProperties.Title + "\n";
                description += calculationProperties.Description;
                return description;
            }
        }

        #region Commands
        private ICommand increaseShapeNumberCommand;
        public ICommand IncreaseShapeNumberCommand => increaseShapeNumberCommand ??= new CommandHandler(
            () => ChangeShapeNumber(1),
            () => ShapeNumber < oscillationShapes.Length);
        private ICommand decreaseShapeNumberCommand;
        public ICommand DecreaseShapeNumberCommand => decreaseShapeNumberCommand ??= new CommandHandler(
            () => ChangeShapeNumber(-1),
            () => ShapeNumber > 1);
        private ICommand changeMainPlotCommand;
        public ICommand ChangeMainPlotCommand => changeMainPlotCommand ??= new RelayCommand<string>(
            (i) => ChangeMainPlot((OscillationShapeType)int.Parse(i)),
            (i) => true);
        private ICommand saveToPNGCommand;
        public ICommand SaveToPNGCommand => saveToPNGCommand ??= new CommandHandler(
            () => SaveToPNG(),
            () => SaveShapeIsChecked || SaveSchemeIsChecked);
        #endregion

        private PlotModel GetMainPlot()
        {
            PlotModel model = Plotter.GetMainPlotModel();
            OscillationShapes.Shape shape = GetShapeByType(mainPlotShape);

            // Adding transparent line covering full x-range of Shaft Scheme, so the two plots are horizontally aligned
            model.Series.Add(Plotter.NewLine(new double[] { shaftScheme.XMin, shaftScheme.XMax }, new double[] { 0, 0 }, OxyColors.Transparent));

            model.Series.Add(Plotter.NewAxisLine(shape.X));
            if (ShowNodesIsChecked)
                model.Series.Add(Plotter.NewCircleLine(shape.XNodes, shape.YNodes));

            model.Series.Add(Plotter.NewLine(shape.X, shape.Y));
            
            if (ShowGridIsChecked)
            {
                model.Axes[0].ExtraGridlines = shaftScheme.XCoordinates.ToArray();
                model.Axes[0].ExtraGridlineStyle = LineStyle.Dot;
                model.Axes[0].ExtraGridlineColor = OxyColors.Gray;
            }

            return model;
        }
        private PlotModel GetThumbnailPlot(OscillationShapeType shapeType)
        {
            PlotModel model = Plotter.GetThumbnailPlotModel();
            OscillationShapes.Shape shape = GetShapeByType(shapeType);
            OxyColor color = colorsArray[(int)shapeType];

            model.Series.Add(Plotter.NewAxisLine(shape.X));
            model.Series.Add(Plotter.NewLine(shape.X, shape.Y, color));

            return model;
        }
        private PlotModel GetDescriptionPlot()
        {
            return Plotter.ModelFromString(Description, 4, 0, 15);
        }
        private void ThumbnailPlotBorderUpdate()
        {
            ThumbnailPlotWBrush = Brushes.Transparent;
            ThumbnailPlotPhiBrush = Brushes.Transparent;
            ThumbnailPlotMBrush = Brushes.Transparent;
            ThumbnailPlotTBrush = Brushes.Transparent;

            Brush colorBrush = Brushes.SteelBlue;

            switch (mainPlotShape)
            {
                case OscillationShapeType.w:
                    ThumbnailPlotWBrush = colorBrush;
                    break;
                case OscillationShapeType.phi:
                    ThumbnailPlotPhiBrush = colorBrush;
                    break;
                case OscillationShapeType.m:
                    ThumbnailPlotMBrush = colorBrush;
                    break;
                case OscillationShapeType.t:
                    ThumbnailPlotTBrush = colorBrush;
                    break;
            }
            NotifyPropertyChanged(nameof(ThumbnailPlotWBrush));
            NotifyPropertyChanged(nameof(ThumbnailPlotPhiBrush));
            NotifyPropertyChanged(nameof(ThumbnailPlotMBrush));
            NotifyPropertyChanged(nameof(ThumbnailPlotTBrush));
        }

        private OscillationShapes.Shape GetShapeByType(OscillationShapeType shapeType)
        {
            OscillationShapes.Shape shape;
            int i = ShapeNumber - 1;
            switch (shapeType)
            {
                case OscillationShapeType.w:
                default:
                    shape = oscillationShapes[i].W;
                    break;
                case OscillationShapeType.phi:
                    shape = oscillationShapes[i].Phi;
                    break;
                case OscillationShapeType.m:
                    shape = oscillationShapes[i].M;
                    break;
                case OscillationShapeType.t:
                    shape = oscillationShapes[i].T;
                    break;
            }

            return shape;
        }
        /// <summary>
        /// Changes displayed shape number by value (+1/-1)
        /// </summary>
        /// <param name="value">value added to current shape number (+1/-1)</param>
        private void ChangeShapeNumber(int value)
        {
            int newShapeNum = ShapeNumber + value;
            if (newShapeNum <= oscillationShapes.Length && newShapeNum >= 1)
            {
                ShapeNumber = newShapeNum;
                NotifyPropertyChanged(nameof(ShapeNumber));
                NotifyPropertyChanged(nameof(Description));
                NotifyPropertyChanged(nameof(MainPlot));
                NotifyPropertyChanged(nameof(ThumbnailPlotW));
                NotifyPropertyChanged(nameof(ThumbnailPlotPhi));
                NotifyPropertyChanged(nameof(ThumbnailPlotM));
                NotifyPropertyChanged(nameof(ThumbnailPlotT));
            }
        }
        /// <summary>
        /// Changes main plot to given oscillation shape
        /// </summary>
        /// <param name="shapeType">new main plot's oscillation shape</param>
        private void ChangeMainPlot(OscillationShapeType shapeType)
        {
            mainPlotShape = shapeType;
            ThumbnailPlotBorderUpdate();
            NotifyPropertyChanged(nameof(Description));
            NotifyPropertyChanged(nameof(MainPlot));
        }
        private void SaveToPNG()
        {
            string path = System.IO.Path.GetDirectoryName(calculationProperties.FileName);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(calculationProperties.FileName);
            string shape = mainPlotShape.ToString();
            if (mainPlotShape == OscillationShapeType.m || mainPlotShape == OscillationShapeType.t)
                shape = shape.ToUpper();

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Obrázek PNG (*.png)|*.png",
                FileName = System.IO.Path.GetFileName(path + "\\" + fileName + "_" + shape + ".png")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                int resolution = 200;
                int imageWidth = Convert.ToInt32(Math.Round(resolution * 9.6));
                int schemeHeight = Convert.ToInt32(Math.Round(0.22 * imageWidth));
                int shapeHeight = Convert.ToInt32(Math.Round(0.4 * imageWidth));
                int descriptionHeight = Convert.ToInt32(Math.Round(0.125 * imageWidth));

                OxyModelToPng modelToPng = new OxyModelToPng(resolution, imageWidth, imageWidth / 100);

                if (SaveSchemeIsChecked)
                    modelToPng.AddModel(shaftScheme.Scheme, schemeHeight);
                if (SaveShapeIsChecked)
                    modelToPng.AddModel(MainPlot, shapeHeight);
                if (SaveDescriptionIsChecked)
                    modelToPng.AddModel(GetDescriptionPlot(), descriptionHeight);

                modelToPng.SaveToFile(saveFileDialog.FileName);
            }
        }
    }
}
