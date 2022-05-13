using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Kritik
{
    public class CampbellViewModel : INotifyPropertyChanged
    {
        private KritikCalculation kritikCalculation;
        private CampbellDiagram campbellDiagram;
        private readonly Strings strings;
        public CampbellViewModel(KritikCalculation kritikCalculation, Strings strings)
        {
            Shaft shaftClone = (Shaft)kritikCalculation.Shaft.Clone();
            CalculationProperties propertiesClone = (CalculationProperties)kritikCalculation.CalculationProperties.Clone();
            this.kritikCalculation = new KritikCalculation(shaftClone, propertiesClone);

            this.strings = strings;
            double runawaySpeed = kritikCalculation.Shaft.Properties.RunawaySpeed;
            
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
                NotifyPropertyChanged(nameof(NotInProgress));
            }
        }
        public bool NotInProgress => !InProgress;
        public string ProgressBarVisibility => InProgress ? "Visible" : "Collapsed";
        public bool CampbellTabIsSelected { get; set; }

        public PlotModel CampbellDiagramPlotModel { get; private set; } = new PlotModel();

        private ICommand createDiagramCommand;
        public ICommand CreateDiagramCommand => createDiagramCommand ??= new CommandHandler(
            async () => await CreateDiagramAsync(),
            () => MaxShaftRpm > 0 && RpmDivision >= 10 && !InProgress && CampbellTabIsSelected);
        private ICommand saveDiagramCommand;
        public ICommand SaveDiagramCommand => saveDiagramCommand ??= new CommandHandler(
            () => SaveToPng(),
            () => MaxShaftRpm > 0 && RpmDivision >= 10 && !InProgress);


        private async Task CreateDiagramAsync()
        {
            InProgress = true;
            NotifyPropertyChanged(nameof(ProgressBarVisibility));
            Progress<int> progress = new Progress<int>(
                (val) => { 
                    ProgressPercentage = Convert.ToInt32(Math.Round((double)val / RpmDivision * 100));
                    NotifyPropertyChanged(nameof(ProgressPercentage));
                });

            this.campbellDiagram = new CampbellDiagram(kritikCalculation, MaxShaftRpm, RpmDivision, this.strings);
            await this.campbellDiagram.CreateDiagramAsync(progress);
            CampbellDiagramPlotModel = this.campbellDiagram.GetPlotModel();
            NotifyPropertyChanged(nameof(CampbellDiagramPlotModel));
            
            InProgress = false;
            NotifyPropertyChanged(nameof(ProgressBarVisibility));
            CommandManager.InvalidateRequerySuggested();
        }

        private void SaveToPng()
        {
            string fullFilePath = this.kritikCalculation.CalculationProperties.FileName;
            string path = System.IO.Path.GetDirectoryName(fullFilePath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(fullFilePath);

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = Application.Current.Resources.MergedDictionaries[^1]["PNG_image"] + " (*.png)|*.png",
                FileName = System.IO.Path.GetFileName(path + "\\" + fileName + "_Campbell.png")
            };

            if (saveFileDialog.ShowDialog() == false)
                return;

            OxyModelToPng oxyModelToPng = new OxyModelToPng(300, 1920, 0);
            oxyModelToPng.AddModel(CampbellDiagramPlotModel, 1485);
            oxyModelToPng.SaveToFile(saveFileDialog.FileName);
        }

    }
}
