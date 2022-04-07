﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OxyPlot;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Kritik
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string newCalculationFileName = "Nový výpočet.xlsx";
        private bool shaftRotationInfluenceControlsAreVisible;
        public MainViewModel()
        {
            InitializeNewCalculation();
            Strings = new Strings(Strings.Language.cs);
        }

        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            AnyPropertyChanged = true;
        }
        private void NotifySenderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(sender.GetType().Name);
        }
        private bool anyPropertyChanged;
        /// <summary>
        /// Reflects if any property has changed since file was saved
        /// </summary>
        public bool AnyPropertyChanged {
            get => anyPropertyChanged;
            set
            {
                if (value != anyPropertyChanged)
                {
                    anyPropertyChanged = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitle)));
                }
            }
        }
        #endregion

        #region Properties for Calculation Data

        private CalculationProperties calculationProperties;
        public CalculationProperties CalculationProperties
        {
            get => calculationProperties;
            set
            {
                calculationProperties = value;
                NotifyPropertyChanged();
                calculationProperties.PropertyChanged += new PropertyChangedEventHandler(NotifySenderPropertyChanged);
            }
        }

        private Shaft shaft;
        public Shaft Shaft
        {
            get => shaft;
            set 
            {
                shaft = value; 
                NotifyPropertyChanged();
                shaft.PropertyChanged += new PropertyChangedEventHandler(NotifySenderPropertyChanged);
            }
        }

        private KritikResults kritikResults;
        public KritikResults KritikResults
        {
            get => kritikResults;
            set 
            {
                kritikResults = value; 
                NotifyPropertyChanged();
                kritikResults.PropertyChanged += new PropertyChangedEventHandler(NotifySenderPropertyChanged);
            }
        }
        public PlotModel ShaftScheme { get; set; }
        public CollectionHistory<ShaftElementForDataGrid> History { get; set; }
        #endregion

        #region Application View Properties
        public string WindowTitle
        {
            get
            {
                string version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                version = version.Substring(0, version.Length - 2);
                string kritVer = "Kritik " + version;

                if (FileName is null)
                    return kritVer;

                string title = kritVer + " - [" + FileName;
                if (AnyPropertyChanged)
                    title += " *";

                title += "]";
                return title;
            }
        }
        public Strings Strings { get; set; }
        private string fileName;
        public string FileName
        {
            get => fileName;
            set
            {
                NotifyPropertyChanged(nameof(WindowTitle));
                fileName = value;
            }
        }
        public string[] BoundaryConditionsItems => Enums.GetNames<BoundaryCondition>();
        public string[] GyroscopicEffectsItems => Enums.GetNames<GyroscopicEffect>();

        public string ShaftRotationInfluenceVisibility => shaftRotationInfluenceControlsAreVisible ? "visible" : "collapsed";
        public ShaftRotationInfluenceOption ShaftRotationInfluenceSelectedOption { get; set; }
        private double shaftRotationInfluenceCustomValue;
        public double ShaftRotationInfluenceCustomValue
        {
            get => shaftRotationInfluenceCustomValue;
            set
            {
                shaftRotationInfluenceCustomValue = value;
                ShaftRPMUpdate();
            }
        }
        public double ShaftOperatingSpeed
        {
            get => Shaft.Properties.OperatingSpeed;
            set
            {
                Shaft.Properties.OperatingSpeed = value;
                ShaftRPMUpdate();
            }
        }
        public double ShaftRunawaySpeed
        {
            get => Shaft.Properties.RunawaySpeed;
            set
            {
                Shaft.Properties.RunawaySpeed = value;
                ShaftRPMUpdate();
            }
        }
        #endregion

        #region Commands
        private ICommand newCalculationCommand;
        public ICommand NewCalculationCommand => newCalculationCommand ??= new CommandHandler(() => InitializeNewCalculation(), () => true);
        private ICommand openFileCommand;
        public ICommand OpenFileCommand => openFileCommand ??= new CommandHandler(() => OpenFile(), () => true);
        private ICommand saveFileCommand;
        public ICommand SaveFileCommand => saveFileCommand ??= new CommandHandler(() => SaveFile(false), () => AnyPropertyChanged);
        private ICommand saveFileAsCommand;
        public ICommand SaveFileAsCommand => saveFileAsCommand ??= new CommandHandler(() => SaveFile(true), () => true);
        private ICommand fillTodayCommand;
        public ICommand FillTodayCommand => fillTodayCommand ??=
            new CommandHandler(() => CalculationProperties.Date = DateTime.Today.ToShortDateString(), () => true);
        private ICommand shaftRotationOptionCommand;
        public ICommand ShaftRotationOptionCommand => shaftRotationOptionCommand ??=
            new RelayCommand<ShaftRotationInfluenceOption>(
                (e) => { ShaftRotationInfluenceSelectedOption = e; ShaftRPMUpdate(); },
                (e) => Shaft.Properties.ShaftRotationInfluence);
        private ICommand shaftRotationInfluenceVisibilityCommand;
        public ICommand ShaftRotationInfluenceVisibilityCommand => shaftRotationInfluenceVisibilityCommand ??=
            new CommandHandler(
                () => { shaftRotationInfluenceControlsAreVisible = !shaftRotationInfluenceControlsAreVisible; NotifyPropertyChanged(nameof(ShaftRotationInfluenceVisibility)); }, 
                () => true);
        private ICommand historyAddCommand;
        public ICommand HistoryAddCommand => historyAddCommand ??= new CommandHandler(() => History.Add(), () => true);
        private ICommand updateShaftSchemeCommand;
        public ICommand UpdateShaftSchemeCommand => updateShaftSchemeCommand ??= new CommandHandler(() => { }, () => true);

        #endregion

        #region Button actions
        private void InitializeNewCalculation()
        {
            CalculationProperties = new CalculationProperties();
            Shaft = new Shaft();
            KritikResults = new KritikResults();
            History = new CollectionHistory<ShaftElementForDataGrid>(Shaft.Elements);
            FileName = newCalculationFileName;
            NotifyPropertyChanged(nameof(ShaftOperatingSpeed));
            NotifyPropertyChanged(nameof(ShaftRunawaySpeed));
            AnyPropertyChanged = false;
        }
        /// <summary>
        /// Loads Kritik input .xlsx file and stores its data in <see cref="this.ShaftProperties"/>, <see cref="this.CalculationProperties"/> and <see cref="this.Shaft.Elements"/>
        /// </summary>
        public void OpenFile(string fileName = null)
        {
            if (fileName is null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
                if (openFileDialog.ShowDialog() == false)
                    return;
                fileName = openFileDialog.FileName;
            }

            DataLoadSave.LoadResult? loadResult = DataLoadSave.Load(fileName);

            if (loadResult is null)
            {
                MessageBox.Show("Soubor \"" + fileName + "\" se nepodařilo načíst.", "Chyba načítání souboru", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CalculationProperties = loadResult?.CalculationProperties;
            Shaft = new Shaft(loadResult?.ShaftElements);
            Shaft.Properties = loadResult?.ShaftProperties;
            KritikResults = new KritikResults();
            History = new CollectionHistory<ShaftElementForDataGrid>(Shaft.Elements);
            FileName = fileName;
            NotifyPropertyChanged(nameof(ShaftOperatingSpeed));
            NotifyPropertyChanged(nameof(ShaftRunawaySpeed));
            AnyPropertyChanged = false;
        }

        /// <summary>
        /// Saves data from <see cref="ShaftProperties"/>, <see cref="CalculationProperties"/>, <see cref="Shaft.Elements"/> and <see cref="KritikResults"/> into .xlsx file
        /// </summary>
        /// <param name="saveAs">opens save file dialog when true</param>
        public void SaveFile(bool saveAs)
        {
            if (saveAs || FileName == newCalculationFileName)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
                saveFileDialog.FileName = System.IO.Path.GetFileName(FileName);
                if (saveFileDialog.ShowDialog() == false)
                    return;
                FileName = saveFileDialog.FileName;
            }

            List<ShaftElement> shaftElements = new List<ShaftElement>(Shaft.Elements);
            bool saveResultsSuccess = DataLoadSave.SaveResults(FileName, Shaft.Properties, CalculationProperties, shaftElements, KritikResults, Strings);
            bool saveDataSuccess = DataLoadSave.SaveData(FileName, Shaft.Properties, CalculationProperties, shaftElements);

            if (saveResultsSuccess && saveDataSuccess)
                AnyPropertyChanged = false;
            else
                MessageBox.Show("Soubor \"" + FileName + "\" se nepodařilo uložit.", "Chyba ukládání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        #region Other Methods
        /// <summary>
        /// Updates <see cref="Shaft.Properties.ShaftRPM"/> value according to <see cref="ShaftRotationInfluenceSelectedOption"/>
        /// </summary>
        public void ShaftRPMUpdate()
        {
            switch (ShaftRotationInfluenceSelectedOption)
            {
                case ShaftRotationInfluenceOption.custom:
                    Shaft.Properties.ShaftRPM = ShaftRotationInfluenceCustomValue;
                    break;
                case ShaftRotationInfluenceOption.operatingSpeed:
                    Shaft.Properties.ShaftRPM = Shaft.Properties.OperatingSpeed;
                    break;
                case ShaftRotationInfluenceOption.runawaySpeed:
                    Shaft.Properties.ShaftRPM = Shaft.Properties.RunawaySpeed;
                    break;
            }
        }
        #endregion
    }
}
