using System;
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

namespace Kritik
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string newCalculationFileName = "Nový výpočet.xlsx";
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

        #region Application Properties
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

        #endregion

        #region Button actions
        private void InitializeNewCalculation()
        {
            CalculationProperties = new CalculationProperties();
            Shaft = new Shaft();
            KritikResults = new KritikResults();
            History = new CollectionHistory<ShaftElementForDataGrid>(Shaft.Elements);
            FileName = newCalculationFileName;
            AnyPropertyChanged = false;
        }
        /// <summary>
        /// Loads Kritik input .xlsx file and stores its data in <see cref="ShaftProperties"/>, <see cref="CalculationProperties"/> and <see cref="Shaft.Elements"/>
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
            AnyPropertyChanged = false;
        }

        /// <summary>
        /// Saves <see cref="ShaftProperties"/>, <see cref="CalculationProperties"/>, <see cref="Shaft.Elements"/> and <see cref="KritikResults"/> into .xlsx file
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

        #region Properties and Dictionaries for Combobox items To Enums conversion
        private Dictionary<string, BoundaryCondition> boundaryConditionsStringToEnum = new Dictionary<string, BoundaryCondition>()
        {
            {boundaryConditionsItems[0], BoundaryCondition.free },
            {boundaryConditionsItems[1], BoundaryCondition.joint },
            {boundaryConditionsItems[2], BoundaryCondition.fix }
        };
        private Dictionary<BoundaryCondition, string> boundaryConditionsEnumToString = new Dictionary<BoundaryCondition, string>()
        {
            {BoundaryCondition.free, boundaryConditionsItems[0] },
            {BoundaryCondition.joint, boundaryConditionsItems[1] },
            {BoundaryCondition.fix, boundaryConditionsItems[2] }
        };
        private static string[] boundaryConditionsItems = { "volný", "kloub", "vetknutí" };
        public string[] BoundaryConditionsItems => boundaryConditionsItems;
        public string BoundaryConditionLeftComboBoxValue
        {
            get => boundaryConditionsEnumToString[Shaft.Properties.BCLeft];
            set => Shaft.Properties.BCLeft = boundaryConditionsStringToEnum[value];
        }
        public string BoundaryConditionRightComboBoxValue
        {
            get => boundaryConditionsEnumToString[Shaft.Properties.BCRight];
            set => Shaft.Properties.BCRight = boundaryConditionsStringToEnum[value];
        }

        private Dictionary<string, GyroscopicEffect> gyroscopicEffectStringToEnum = new Dictionary<string, GyroscopicEffect>()
        {
            {gyroscopicEffectsItems[0], GyroscopicEffect.none },
            {gyroscopicEffectsItems[1], GyroscopicEffect.forward },
            {gyroscopicEffectsItems[2], GyroscopicEffect.backward }
        };
        private Dictionary<GyroscopicEffect, string> gyroscopicEffectEnumToString = new Dictionary<GyroscopicEffect, string>()
        {
            {GyroscopicEffect.none, gyroscopicEffectsItems[0] },
            {GyroscopicEffect.forward, gyroscopicEffectsItems[1] },
            {GyroscopicEffect.backward, gyroscopicEffectsItems[2] }
        };
        private static string[] gyroscopicEffectsItems = { "zanedbání", "souběžná precese", "protiběžná precese" };
        public string[] GyroscopicEffectsItems => gyroscopicEffectsItems;
        public string GyroscopicEffectComboBoxValue
        {
            get => gyroscopicEffectEnumToString[Shaft.Properties.Gyros];
            set => Shaft.Properties.Gyros = gyroscopicEffectStringToEnum[value];
        }
        

        #endregion
    }
}
