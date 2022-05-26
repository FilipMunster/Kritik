using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OxyPlot;
using OxyPlot.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Kritik
{
    public class CampbellViewModel : INotifyPropertyChanged
    {
        private KritikCalculation kritikCalculation;
        private CampbellDiagram campbellDiagram;
        private MovableAnnotations<LineAnnotation> speedAnnotations = new MovableAnnotations<LineAnnotation>();
        private MovableAnnotations<TextAnnotation> precessionAnnotations = new MovableAnnotations<TextAnnotation>();
        private readonly Strings strings;

        private CancellationTokenSource cancellationTokenSource;
        public CampbellViewModel(KritikCalculation kritikCalculation, Strings strings)
        {
            Shaft shaftClone = (Shaft)kritikCalculation.Shaft.Clone();
            CalculationProperties propertiesClone = (CalculationProperties)kritikCalculation.CalculationProperties.Clone();
            this.kritikCalculation = new KritikCalculation(shaftClone, propertiesClone);
            this.strings = strings;
                                  
            MaxShaftRpm = Convert.ToInt32(Math.Ceiling(kritikCalculation.CriticalSpeeds.Max() / 100) * 100);
            NotifyPropertyChanged(nameof(ProgressBarVisibility));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int maxShaftRpm;
        public int MaxShaftRpm
        {
            get => maxShaftRpm;
            set
            {
                maxShaftRpm = value;
                NotifyPropertyChanged(nameof(RpmStep));
            }
        }

        public double MaxCriticalSpeed
        {
            get => this.kritikCalculation.Shaft.Properties.MaxCriticalSpeed;
            set => this.kritikCalculation.Shaft.Properties.MaxCriticalSpeed = value;
        }

        private int rpmDivision = 10;
        public int RpmDivision
        {
            get => rpmDivision;
            set
            {
                rpmDivision = value;
                NotifyPropertyChanged(nameof(RpmStep));
            }
        }
        public string RpmStep => String.Format("{0:0.000}", (double)MaxShaftRpm / RpmDivision);
        public int ProgressPercentage { get; private set; }
        private bool inProgress;
        public bool InProgress
        {
            get => inProgress;
            set
            {
                inProgress = value;
                NotifyPropertyChanged(nameof(ProgressBarVisibility));
                NotifyPropertyChanged(nameof(NotInProgress));
            }
        }
        public bool NotInProgress => !InProgress;
        public string ProgressBarVisibility => InProgress ? "Visible" : "Collapsed";
        public bool CampbellTabIsSelected { get; set; }

        public bool ForwardPrecessionIsChecked { get; set; } = true;
        public bool BackwardPrecessionIsChecked { get; set; } = true;

        private bool operatingSpeedIsChecked;
        public bool OperatingSpeedIsChecked
        {
            get => operatingSpeedIsChecked;
            set
            {
                operatingSpeedIsChecked = value;
                UpdateSpeedAnnotations();
            }
        }
        private bool runawaySpeedIsChecked;
        public bool RunawaySpeedIsChecked
        {
            get => runawaySpeedIsChecked;
            set
            {
                runawaySpeedIsChecked = value;
                UpdateSpeedAnnotations();
            }
        }

        private bool criticalSpeedLowerLimitIsChecked;
        public bool CriticalSpeedLowerLimitIsChecked
        {
            get => criticalSpeedLowerLimitIsChecked;
            set
            {
                criticalSpeedLowerLimitIsChecked = value;
                UpdateSpeedAnnotations();
            }
        }

        private bool operatingSpeedRangeIsChecked;
        public bool OperatingSpeedRangeIsChecked
        {
            get => operatingSpeedRangeIsChecked;
            set
            {
                operatingSpeedRangeIsChecked = value;
                UpdateSpeedAnnotations();
            }
        }
        private (int Min, int Max) operatingSpeedRange;
        public int OperatingSpeedRangeMin
        {
            get => operatingSpeedRange.Min;
            set
            {
                operatingSpeedRange.Min = value;
                UpdateSpeedAnnotations();
            }
        }
        public int OperatingSpeedRangeMax
        {
            get => operatingSpeedRange.Max;
            set
            {
                operatingSpeedRange.Max = value;
                UpdateSpeedAnnotations();
            }
        }
        private double criticalSpeedLowerLimitFactor;
        public double CriticalSpeedLowerLimitFactor
        {
            get => criticalSpeedLowerLimitFactor;
            set
            {
                criticalSpeedLowerLimitFactor = value;
                UpdateSpeedAnnotations();
            }
        }
        public bool showLabelsIsChecked;
        public bool ShowLabelsIsChecked
        {
            get => showLabelsIsChecked;
            set
            {
                showLabelsIsChecked = value;
                UpdatePrecessionAnnotations();
            }
        }
        public bool labelsBackgroundIsChecked = true;
        public bool LabelsBackgroundIsChecked
        {
            get => labelsBackgroundIsChecked;
            set
            {
                labelsBackgroundIsChecked = value;
                UpdatePrecessionAnnotations();
            }
        }

        private (int Width, int Height, int DPI) image = 
            (Properties.Settings.Default.CampbellImgW, Properties.Settings.Default.CampbellImgH, Properties.Settings.Default.CampbellImgDPI);
        public int ImageWidth
        {
            get => image.Width;
            set
            {
                image.Width = value;
                Properties.Settings.Default.CampbellImgW = value;
            }
        }
        public int ImageHeight
        {
            get => image.Height;
            set
            {
                image.Height = value;
                Properties.Settings.Default.CampbellImgH = value;
            }
        }
        public int ImageDPI
        {
            get => image.DPI;
            set
            {
                image.DPI = value;
                Properties.Settings.Default.CampbellImgDPI = value;
            }
        }

        public PlotModel CampbellDiagramPlotModel { get; private set; } = new PlotModel();

        private ICommand createDiagramCommand;
        public ICommand CreateDiagramCommand => createDiagramCommand ??= new CommandHandler(
            async () => await CreateDiagramAsync(),
            () => MaxShaftRpm > 0
            && RpmDivision >= 10 
            && !InProgress 
            && CampbellTabIsSelected 
            && (ForwardPrecessionIsChecked || BackwardPrecessionIsChecked));
        
        private ICommand saveDiagramCommand;
        public ICommand SaveDiagramCommand => saveDiagramCommand ??= new CommandHandler(
            () => SaveDiagram(),
            () => MaxShaftRpm > 0 && RpmDivision >= 10 && !InProgress && CampbellDiagramPlotModel.Axes.Count > 0);

        private ICommand resetImagePropertiesCommand;
        public ICommand ResetImagePropertiesCommand => resetImagePropertiesCommand ??= new CommandHandler(
            () =>
            {
                ImageWidth = 1920;
                ImageHeight = 1440;
                ImageDPI = 250;
                NotifyPropertyChanged(nameof(ImageHeight));
                NotifyPropertyChanged(nameof(ImageWidth));
                NotifyPropertyChanged(nameof(ImageDPI));
            },
            () => true);

        private ICommand cancelCommand;
        public ICommand CancelCommand => cancelCommand ??= new CommandHandler(
            () => cancellationTokenSource?.Cancel(),
            () => true);
        private ICommand resetLabelsCommand;
        public ICommand ResetLabelsCommand => resetLabelsCommand ??= new CommandHandler(
            () => { UpdateSpeedAnnotations(true); UpdatePrecessionAnnotations(true); },
            () => true);

        /// <summary>
        /// Finds max critical speed and updates <see cref="MaxShaftRpm"/>
        /// </summary>
        /// <returns></returns>
        public async Task FindMaxCriticalSpeedAsync()
        {
            ShaftProperties shaftProperties = kritikCalculation.Shaft.Properties;
            shaftProperties.ShaftRotationInfluence = false;

            shaftProperties.Gyros = GyroscopicEffect.forward;
            await kritikCalculation.CalculateCriticalSpeedsAsync();
            double maxCriticalSpeed = kritikCalculation.CriticalSpeeds.Max();

            shaftProperties.Gyros = GyroscopicEffect.backward;
            await kritikCalculation.CalculateCriticalSpeedsAsync();
            if (kritikCalculation.CriticalSpeeds.Max() > maxCriticalSpeed)
                maxCriticalSpeed = kritikCalculation.CriticalSpeeds.Max();

            MaxShaftRpm = Convert.ToInt32(Math.Ceiling(maxCriticalSpeed / 100) * 100);
            NotifyPropertyChanged(nameof(MaxShaftRpm));
        }

        private async Task CreateDiagramAsync()
        {
            InProgress = true;
            Progress<int> progress = new Progress<int>(
                (val) => { 
                    ProgressPercentage = Convert.ToInt32(Math.Round((double)val / RpmDivision * 100));
                    NotifyPropertyChanged(nameof(ProgressPercentage));
                });

            this.campbellDiagram = new CampbellDiagram(kritikCalculation, MaxShaftRpm, RpmDivision, this.strings);

            this.cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            await this.campbellDiagram.CreateDiagramAsync(progress, token, ForwardPrecessionIsChecked, BackwardPrecessionIsChecked);
            this.cancellationTokenSource.Dispose();

            if (CampbellDiagramPlotModel.Axes.Count == 0)
            {
                // If it's first time creating the Diagram:
                CampbellDiagramPlotModel = this.campbellDiagram.GetPlotModel();
                CampbellDiagramPlotModel.MouseMove += CampbellDiagramPlotModel_MouseMove;
                
                UpdateSpeedAnnotations(true);
                UpdatePrecessionAnnotations(true);
            }
            else
            {
                // Else just update the axes and series of plot model and the annotations
                this.campbellDiagram.UpdateModelData(CampbellDiagramPlotModel);
                UpdateSpeedAnnotations();
                UpdatePrecessionAnnotations();
            }

            NotifyPropertyChanged(nameof(CampbellDiagramPlotModel));
            
            InProgress = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void SaveDiagram()
        {
            string fullFilePath = this.kritikCalculation.CalculationProperties.FileName;
            string path = Path.GetDirectoryName(fullFilePath);
            string fileName = Path.GetFileNameWithoutExtension(fullFilePath);

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = Application.Current.Resources.MergedDictionaries[^1]["PNG_image"] + " (*.png)|*.png" +
                "|" + Application.Current.Resources.MergedDictionaries[^1]["Excel_sheet"] + " (*.xlsx)|*.xlsx",
                FileName = Path.GetFileName(path + "\\" + fileName + "_Campbell")
            };

            if (saveFileDialog.ShowDialog() == false)
                return;

            switch (saveFileDialog.FilterIndex)
            {
                case 1:
                    {
                        OxyModelToPng oxyModelToPng = new OxyModelToPng(ImageDPI, ImageWidth, 0);
                        oxyModelToPng.AddModel(CampbellDiagramPlotModel, ImageHeight);
                        oxyModelToPng.SaveToFile(saveFileDialog.FileName);
                        break;
                    }
                case 2:
                    {
                        SaveDiagramToExcel(saveFileDialog.FileName);
                        break;
                    }
            }

            CampbellDiagramPlotModel.InvalidatePlot(false);
        }

        private void UpdateSpeedAnnotations(bool setInitialPosition = false)
        {
            if (CampbellDiagramPlotModel.Axes.Count == 0)
                return;

            bool[] checkedValues = new bool[]
            {
                OperatingSpeedIsChecked,
                RunawaySpeedIsChecked,
                CriticalSpeedLowerLimitIsChecked,
                OperatingSpeedRangeIsChecked,
                OperatingSpeedRangeIsChecked
            };
            double[] rpmValues = new double[]
            {
                this.kritikCalculation.Shaft.Properties.OperatingSpeed,
                this.kritikCalculation.Shaft.Properties.RunawaySpeed,
                this.kritikCalculation.Shaft.Properties.RunawaySpeed * CriticalSpeedLowerLimitFactor,
                OperatingSpeedRangeMin,
                OperatingSpeedRangeMax
            };
            string[] textValues = new string[]
            {
                this.strings.provozniOtacky,
                this.strings.prubezneOtacky,
                this.strings.prubezneOtacky,
                "min. " + this.strings.provozniOtacky,
                "max. " + this.strings.provozniOtacky,
            };

            if (this.speedAnnotations.Count == 0)
            {
                // Create 5 new Line annotations and add reference for plot model
                for (int i = 0; i < checkedValues.Length; i++)
                {
                    this.speedAnnotations.Add(new LineAnnotation());
                    CampbellDiagramPlotModel.Annotations.Add(speedAnnotations[i]);
                }
            }

            for (int i = 0; i < checkedValues.Length; i++)
            {
                LineAnnotation line = this.speedAnnotations[i];

                line.StrokeThickness = 1;
                line.Color = OxyColors.Black;
                line.TextColor = OxyColors.Black;
                line.FontSize = 13;
                line.Font = "Calibri";
                line.Type = LineAnnotationType.Vertical;
                line.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;

                line.Text = textValues[i] + " " + rpmValues[i] + " rpm";
                if (i == 2)
                    line.Text = textValues[i] + " × " + criticalSpeedLowerLimitFactor;
                line.X = rpmValues[i];
                if (setInitialPosition)
                    line.TextPosition = new DataPoint(rpmValues[i], this.campbellDiagram.MaxCriticalSpeed * 0.01);
                else
                    line.TextPosition = new DataPoint(rpmValues[i], line.TextPosition.Y);
                line.TextRotation = 270;

                if (!checkedValues[i])
                {
                    line.Color = OxyColors.Transparent;
                    line.TextColor = OxyColors.Transparent;
                }
            }                     
            CampbellDiagramPlotModel.InvalidatePlot(false);
        }

        private void UpdatePrecessionAnnotations(bool setInitialPosition = false)
        {
            if (campbellDiagram is null)
                return;

            CampbellDiagram.Precession[] forwardPrecessions = this.campbellDiagram.ForwardPrecessions;
            CampbellDiagram.Precession[] backwardPrecessions = this.campbellDiagram.BackwardPrecessions;

            List<TextAnnotation> forwardAnnotations = new List<TextAnnotation>(
                this.precessionAnnotations.Where((a) => ((CampbellDiagram.Precession)a.Tag).PrecessionType == GyroscopicEffect.forward));
            List<TextAnnotation> backwardAnnotations = new List<TextAnnotation>(
                this.precessionAnnotations.Where((a) => ((CampbellDiagram.Precession)a.Tag).PrecessionType == GyroscopicEffect.backward));

            bool resetPosition = setInitialPosition 
                || forwardPrecessions.Length != forwardAnnotations.Count 
                || backwardPrecessions.Length != backwardAnnotations.Count;

            // If theres no change in precessions, just update the annotations text and appearance
            if (!resetPosition)
            {
                foreach (TextAnnotation annotation in this.precessionAnnotations)
                {
                    string precessionNumber = this.strings.OrdinalNumber(((CampbellDiagram.Precession)annotation.Tag).PrecessionNumber + 1, true).ToUpper();
                    string precessionName = ((CampbellDiagram.Precession)annotation.Tag).PrecessionType == GyroscopicEffect.forward ?
                        this.strings.SOUBEZNAPRECESE : this.strings.PROTIBEZNAPRECESE;
                    annotation.Text = precessionNumber + " " + precessionName;

                    annotation.TextColor = ShowLabelsIsChecked ? OxyColors.Black : OxyColors.Transparent;
                    annotation.Background = (ShowLabelsIsChecked && LabelsBackgroundIsChecked) ? OxyColors.White : OxyColors.Transparent;
                    annotation.Stroke = (ShowLabelsIsChecked && LabelsBackgroundIsChecked) ? OxyColors.Black : OxyColors.Transparent;
                }
                CampbellDiagramPlotModel.InvalidatePlot(false);
                return;
            }                

            double maxCriticalSpeed = this.campbellDiagram.MaxCriticalSpeed;
            while (this.precessionAnnotations.Count > 0)
            {
                CampbellDiagramPlotModel.Annotations.Remove(this.precessionAnnotations[0]);
                this.precessionAnnotations.RemoveAt(0);
            }

            foreach (CampbellDiagram.Precession precession in forwardPrecessions.Concat(backwardPrecessions))
            {
                this.precessionAnnotations.Add(GetAnnotation(precession));
                CampbellDiagramPlotModel.Annotations.Add(this.precessionAnnotations[^1]);
            }
            CampbellDiagramPlotModel.InvalidatePlot(false);

            TextAnnotation GetAnnotation(CampbellDiagram.Precession precession)
            {
                string precessionNumber = this.strings.OrdinalNumber(precession.PrecessionNumber + 1, true).ToUpper();
                string precessionName = precession.PrecessionType == GyroscopicEffect.forward ? this.strings.SOUBEZNAPRECESE : this.strings.PROTIBEZNAPRECESE;

                TextAnnotation annotation = new TextAnnotation();
                annotation.StrokeThickness = 1;
                annotation.Padding = new OxyThickness(3, 2, 3, 2);
                annotation.TextColor = ShowLabelsIsChecked ? OxyColors.Black : OxyColors.Transparent;
                annotation.Font = "Calibri";
                annotation.FontSize = 13;
                annotation.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center;
                annotation.Background = (ShowLabelsIsChecked && LabelsBackgroundIsChecked) ? OxyColors.White : OxyColors.Transparent;
                annotation.Stroke = (ShowLabelsIsChecked && LabelsBackgroundIsChecked) ? OxyColors.Black : OxyColors.Transparent;
                annotation.Tag = precession;

                annotation.Text = precessionNumber + " " + precessionName;

                (double[] rotorSpeeds, double[] criticalSpeeds) = precession.GetValues();
                double textXPosition = rotorSpeeds[rotorSpeeds.Length * 2 / 3];
                double textYPosition = criticalSpeeds[criticalSpeeds.Length / 2] - maxCriticalSpeed * 0.03;
                annotation.TextPosition = new DataPoint(textXPosition, textYPosition);

                return annotation;
            }
        }

        private void CampbellDiagramPlotModel_MouseMove(object sender, OxyMouseEventArgs e)
        {
            this.speedAnnotations.MoveAnnotation(e, CampbellDiagramPlotModel, false, true);
            this.precessionAnnotations.MoveAnnotation(e, CampbellDiagramPlotModel, true, true);
        }

        public void OnLanguageChanged()
        {
            UpdateSpeedAnnotations();
            UpdatePrecessionAnnotations();
            CampbellDiagramPlotModel.Axes[0].Title = this.strings.OtackyRotoru + " (rpm)";
            CampbellDiagramPlotModel.Axes[1].Title = this.strings.KritickeOtacky + " (rpm)";
            CampbellDiagramPlotModel.InvalidatePlot(false);
        }

        private void SaveDiagramToExcel(string fileName)
        {
            string sheetName = "KRITIK_Campbell";
            ResourceDictionary dictionary = Application.Current.Resources.MergedDictionaries[^1];

            if (File.Exists(fileName))
            {
                CustomSheetDialogWindow dialogWindow = new CustomSheetDialogWindow();
                dialogWindow.CustomSheetName = sheetName;
                dialogWindow.Owner = Application.Current.MainWindow;
                dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (dialogWindow.ShowDialog() == false) 
                    return;
                sheetName = dialogWindow.CustomSheetName;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileInfo fileInfo = new FileInfo(fileName);
            try
            {
                using (ExcelPackage p = new ExcelPackage(fileInfo))
                {
                    if (p.Workbook.Worksheets[sheetName] is not null)
                    {
                        p.Workbook.Worksheets.Delete(sheetName);
                    }
                    ExcelWorksheet ws = p.Workbook.Worksheets.Add(sheetName);

                    ExcelChart chart = ws.Drawings.AddChart("Campbell diagram", eChartType.XYScatterLinesNoMarkers);
                    chart.Title.Text = this.kritikCalculation.CalculationProperties.Title + " - " + this.strings.CampbellDiagram;
                    chart.RoundedCorners = false;
                    chart.SetSize(800, 600);

                    ws.Cells.Style.Font.Bold = false;
                    ws.Cells.Style.Font.Color.SetColor(Color.Black);
                    ws.Cells.Style.Font.Italic = false;
                    ws.Cells.Style.Font.UnderLine = false;
                    ws.Cells.Style.Font.Size = 11;
                    ws.Cells.Style.Font.Name = "Calibri";

                    ws.Cells[1, 1].Value = this.kritikCalculation.CalculationProperties.Title + " - " + this.strings.CampbellDiagram + " " + this.strings.data;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[2, 1].Value = this.strings.OtackyRotoru.ToLower() + " (rpm)";
                    ws.Cells[2, 1].AutoFitColumns();
                    ws.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[2, 2].Value = this.strings.KritickeOtacky.ToLower() + " (rpm)";
                    ws.Cells[2, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    int col = 2;
                    foreach (CampbellDiagram.Precession precession in campbellDiagram.ForwardPrecessions.Concat(campbellDiagram.BackwardPrecessions))
                    {
                        // Fill table header
                        string number = strings.OrdinalNumber(precession.PrecessionNumber + 1);
                        string name = precession.PrecessionType == GyroscopicEffect.forward ? strings.SoubeznaPrecese.ToLower() : strings.ProtibeznaPrecese.ToLower();
                        ws.Cells[3, col].Value = number + " " + name;

                        // Fill Rotor speed column
                        if (col == 2)
                        {
                            (double[] rotorSpeeds, _) = precession.GetValues();
                            for (int i = 0; i < rotorSpeeds.Length; i++)
                            {
                                ws.Cells[4 + i, 1].Value = rotorSpeeds[i];
                            }
                        }

                        // Fill data
                        (_, double[] criticalSpeeds) = precession.GetValues();
                        for (int i = 0; i < criticalSpeeds.Length; i++)
                        {
                            ws.Cells[4 + i, col].Value = criticalSpeeds[i];
                        }

                        // Add series to chart
                        chart.Series.Add(ws.Cells[4, col, 3 + criticalSpeeds.Length, col], ws.Cells[4, 1, 3 + criticalSpeeds.Length, 1]);
                        chart.Series[^1].Header = (string)ws.Cells[3, col].Value;

                        col++;
                    }
                    ws.Cells[2, 2, 2, col - 1].Merge = true;
                    chart.SetPosition(ws.Dimension.End.Row + 1, 0, 1, 0);

                    p.Save();
                }
            }
            catch
            {
                MessageBox.Show(dictionary["File"] +
                    " \"" + fileName + "\" " + dictionary["failed_to_save"],
                    (string)dictionary["Error_saving_file"],
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return;
        }
    }
}
