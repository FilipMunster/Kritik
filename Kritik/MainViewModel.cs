using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kritik
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string newCalculationFileName = "Nový výpočet.xlsx";
        private bool shaftRotationInfluenceControlsAreVisible;
        public MainViewModel()
        {
            InitializeNewCalculation();
            Strings = new Strings((Strings.Language)Properties.Settings.Default.lang);
        }

        public delegate void SelectedElementChangedEventHandler(object sender, int elementId);
        public event SelectedElementChangedEventHandler SelectedElementChanged;

        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            AnyPropertyChanged = true;
        }

        private bool anyPropertyChanged;
        /// <summary>
        /// Reflects if any property has changed since file was saved
        /// </summary>
        public bool AnyPropertyChanged
        {
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

        #region Model Properties

        private CalculationProperties calculationProperties;
        public CalculationProperties CalculationProperties
        {
            get => calculationProperties;
            set
            {
                calculationProperties = value;
                NotifyPropertyChanged();
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
                BeamPlusControlsUpdate();
            }
        }
        private ShaftElementForDataGrid shaftElementSelected;
        public ShaftElementForDataGrid ShaftElementSelected
        {
            get => shaftElementSelected;
            set
            {
                shaftElementSelected = value;
                BeamPlusControlsUpdate();
                ShaftScheme.Draw(value);
            }
        }

        private KritikCalculation kritikCalculation;
        public KritikCalculation KritikCalculation
        {
            get => kritikCalculation;
            set
            {
                kritikCalculation = value;
                NotifyPropertyChanged();
            }
        }
        public CollectionHistory<ShaftElementForDataGrid> History { get; set; }
        public Strings Strings { get; set; }
        private OscillationShapesViewModel oscillationShapesViewModel;
        public OscillationShapesViewModel OscillationShapesViewModel
        {
            get => oscillationShapesViewModel;
            set
            {
                oscillationShapesViewModel = value;
                NotifyPropertyChanged();
            }
        }
        private ShaftScheme shaftScheme;
        public ShaftScheme ShaftScheme
        {
            get => shaftScheme;
            set
            {
                shaftScheme = value;
                NotifyPropertyChanged();
            }
        }
        public ShaftScheme ShaftScheme2 => (ShaftScheme)ShaftScheme.Clone();
        #endregion

        #region View Properties
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
        public int TabControlSelectedIndex { get; set; }
        private string[] boundaryConditionsItems;
        public string[] BoundaryConditionsItems => boundaryConditionsItems ??= Enums.GetNames<BoundaryCondition>();
        private string[] gyroscopicEffectsItems;
        public string[] GyroscopicEffectsItems => gyroscopicEffectsItems ??= Enums.GetNames<GyroscopicEffect>();
        private double shaftScheme1Width;
        public double ShaftScheme1Width
        {
            get => shaftScheme1Width;
            set
            {
                shaftScheme1Width = value;
                if (ShaftScheme is not null)
                    ShaftScheme.SchemeWidth = value;
            }
        }
        private double shaftScheme2Width;
        public double ShaftScheme2Width
        {
            get => shaftScheme2Width;
            set
            {
                shaftScheme2Width = value;
                if (ShaftScheme2 is not null)
                    ShaftScheme2.SchemeWidth = value;
            }
        }

        #region Shaft rotation influence controls
        public string ShaftRotationInfluenceButtonSource => shaftRotationInfluenceControlsAreVisible ? "/icons/StepBackArrow_16x.png" : "/icons/StepOverArrow_16x.png";
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
        #endregion
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
        public bool OscillationShapesTabIsEnabled => KritikCalculation.CriticalSpeeds?.Length > 0;

        #region Beam+ controls
        /// <summary>
        /// True if <see cref="ShaftElementSelected"/> in datagrid is <see cref="ElementType.beamPlus"/>
        /// </summary>
        public bool BeamPlusElementIsSelected => ShaftElementSelected?.Type == ElementType.beamPlus;
        public bool BeamPlusComboBoxIsNotEmpty => BeamPlusComboBoxItems.Count > 0;
        /// <summary>
        /// Array of beamPlus elements numbers - 1-based indexing
        /// </summary>
        public List<int> BeamPlusComboBoxItems
        {
            get
            {
                List<int> numbers = new List<int>();
                for (int i = 0; i < Shaft.Elements.Count; i++)
                {
                    if (Shaft.Elements[i].Type == ElementType.beamPlus)
                        numbers.Add(i + 1);
                }
                return numbers;
            }
        }
        public int BeamPlusComboBoxSelectedItem
        {
            get
            {
                if (ShaftElementSelected is null || ShaftElementSelected.Type != ElementType.beamPlus)
                    return -1;

                return Shaft.Elements.IndexOf(ShaftElementSelected) + 1;
            }
            set
            {
                ShaftElementSelected = Shaft.Elements[value - 1];
                NotifyPropertyChanged(nameof(ShaftElementSelected));
            }
        }
        public int BeamPlusDivision
        {
            get => ShaftElementSelected?.Type == ElementType.beamPlus ? ShaftElementSelected.Division : 0;
            set { ShaftElementSelected.Division = value; NotifyPropertyChanged(nameof(BeamPlusText)); }
        }
        public int BeamPlusIdN
        {
            get => ShaftElementSelected?.Type == ElementType.beamPlus ? ShaftElementSelected.IdN : 0;
            set { ShaftElementSelected.IdN = value; NotifyPropertyChanged(nameof(BeamPlusText)); }
        }
        public double BeamPlusIdNValue
        {
            get => ShaftElementSelected?.Type == ElementType.beamPlus ? ShaftElementSelected.IdNValue : 0;
            set { ShaftElementSelected.IdNValue = value; NotifyPropertyChanged(nameof(BeamPlusText)); }
        }
        public string BeamPlusText
        {
            get
            {
                ShaftElementForDataGrid element = ShaftElementSelected;
                if (element is null || element.Type != ElementType.beamPlus)
                    return "(Jdᵢ =  . . . kg.m²)";
                double value = 0;
                switch (element.IdN)
                {
                    case 0:
                        value = element.Id / element.Division * element.IdNValue;
                        break;
                    case 1:
                        value = element.IdNValue;
                        break;
                }
                return "(Jdᵢ = " + value.ToString("0.###") + " kg.m²)";
            }
        }
        #endregion

        #endregion

        #region Commands
        private ICommand newCalculationCommand;
        public ICommand NewCalculationCommand => newCalculationCommand ??= new CommandHandler(
            () => InitializeNewCalculation(),
            () => TabControlSelectedIndex == 0);
        private ICommand openFileCommand;
        public ICommand OpenFileCommand => openFileCommand ??= new CommandHandler(
            () => OpenFile(),
            () => TabControlSelectedIndex == 0);
        private ICommand saveFileCommand;
        public ICommand SaveFileCommand => saveFileCommand ??= new CommandHandler(
            () => SaveFile(false),
            () => AnyPropertyChanged && TabControlSelectedIndex == 0);
        private ICommand saveFileAsCommand;
        public ICommand SaveFileAsCommand => saveFileAsCommand ??= new CommandHandler(
            () => SaveFile(true),
            () => TabControlSelectedIndex == 0);
        private ICommand fillTodayCommand;
        public ICommand FillTodayCommand => fillTodayCommand ??=
            new CommandHandler(() => CalculationProperties.Date = DateTime.Today.ToShortDateString(), () => true);

        #region Rotation Influence Commands
        private ICommand shaftRotationInfluenceCheckBoxCommand;
        public ICommand ShaftRotationInfluenceCheckBoxCommand => shaftRotationInfluenceCheckBoxCommand ??=
            new CommandHandler(
                () => ShaftRPMUpdate(),
                () => Shaft.Properties.Gyros != GyroscopicEffect.none);
        private ICommand shaftRotationOptionChangedCommand;
        public ICommand ShaftRotationOptionChangedCommand => shaftRotationOptionChangedCommand ??=
            new CommandHandler(
                () => ShaftRPMUpdate(),
                () => Shaft.Properties.ShaftRotationInfluence);

        private ICommand shaftRotationInfluenceVisibilityCommand;
        public ICommand ShaftRotationInfluenceVisibilityCommand => shaftRotationInfluenceVisibilityCommand ??=
            new CommandHandler(
                () =>
                {
                    shaftRotationInfluenceControlsAreVisible = !shaftRotationInfluenceControlsAreVisible;
                    NotifyPropertyChanged(nameof(ShaftRotationInfluenceVisibility)); 
                    NotifyPropertyChanged(nameof(ShaftRotationInfluenceButtonSource));
                },
                () => true);
        #endregion

        #region DataGrid Controls Commands
        private ICommand addItemCommand;
        public ICommand AddItemCommand => addItemCommand ??= new RelayCommand<object>(
            (obj) => { ShaftElementSelected = Shaft.AddElement(ShaftElementSelected); CommonBtnActions(); },
            (obj) => TabControlSelectedIndex == 0);
        private ICommand removeItemCommand;
        public ICommand RemoveItemCommand => removeItemCommand ??= new RelayCommand<object>(
            (obj) => { ShaftElementSelected = Shaft.RemoveSelectedElement(ShaftElementSelected); CommonBtnActions(); },
            (obj) => Shaft.Elements.Count > 0 && TabControlSelectedIndex == 0);
        private ICommand moveItemUpCommand;
        public ICommand MoveItemUpCommand => moveItemUpCommand ??= new RelayCommand<object>(
            (obj) => { Shaft.MoveElementUp(ShaftElementSelected); CommonBtnActions(); },
            (obj) => Shaft.Elements.IndexOf(ShaftElementSelected) > 0 && TabControlSelectedIndex == 0);
        private ICommand moveItemDownCommand;
        public ICommand MoveItemDownCommand => moveItemDownCommand ??= new RelayCommand<object>(
            (obj) => { Shaft.MoveElementDown(ShaftElementSelected); CommonBtnActions(); },
            (obj) =>
            {
                int i = Shaft.Elements.IndexOf(ShaftElementSelected);
                return i < Shaft.Elements.Count - 1 && i >= 0 && TabControlSelectedIndex == 0;
            });
        private ICommand mirrorShaftCommand;
        public ICommand MirrorShaftCommand => mirrorShaftCommand ??= new RelayCommand<object>(
            (obj) => { if (History.Count == 0) { History.Add(); } ShaftElementSelected = Shaft.Mirror(); CommonBtnActions(); },
            (obj) => Shaft.Elements.Count > 0 && TabControlSelectedIndex == 0);
        private ICommand removeAllItemsCommand;
        public ICommand RemoveAllItemsCommand => removeAllItemsCommand ??= new CommandHandler(
            () => { Shaft.RemoveAllElements(); History.Add(); BeamPlusControlsUpdate(); },
            () => Shaft.Elements.Count > 0 && TabControlSelectedIndex == 0);
        private ICommand historyBackCommand;
        public ICommand HistoryBackCommand => historyBackCommand ??= new RelayCommand<object>(
            (obj) =>
            {
                Shaft.Elements = History.Back(); History.SetCollection(Shaft.Elements);
                ShaftElementSelected = Shaft.Elements.FirstOrDefault();
                ShaftScheme.SetShaft(Shaft, ShaftElementSelected);
                CommonBtnActions(false);
            },
            (obj) => History.CanGoBack() && TabControlSelectedIndex == 0);
        private ICommand historyForwardCommand;
        public ICommand HistoryForwardCommand => historyForwardCommand ??= new RelayCommand<object>(
            (obj) =>
            {
                Shaft.Elements = History.Forward(); History.SetCollection(Shaft.Elements);
                ShaftElementSelected = Shaft.Elements.FirstOrDefault();
                ShaftScheme.SetShaft(Shaft, ShaftElementSelected);
                CommonBtnActions(false);
            },
            (obj) => History.CanGoForward() && TabControlSelectedIndex == 0);
        #endregion

        private ICommand cellEditEndingCommand;
        public ICommand CellEditEndingCommand => cellEditEndingCommand ??= new CommandHandler(
            () => CellEditEnding(),
            () => true);
        private ICommand languageDropDownClosedCommand;
        public ICommand LanguageDropDownClosedCommand => languageDropDownClosedCommand ??= new CommandHandler(
            () => NotifyPropertyChanged(nameof(OscillationShapesViewModel)),
            () => true);
        private ICommand historyAddCommand;
        public ICommand HistoryAddCommand => historyAddCommand ??= new CommandHandler(
            () => { History.Add(); ShaftScheme?.NotifyPropertyChanged(nameof(ShaftScheme.Scheme)); },
            () => true);
        private ICommand kritikCalculateCommand;
        public ICommand KritikCalculateCommand => kritikCalculateCommand ??= new CommandHandler(
                async () => await RunCalculationAsync(),
                () => !KritikCalculation.IsCalculationInProgress && TabControlSelectedIndex == 0);
        #endregion

        #region Button actions methods
        private void InitializeNewCalculation()
        {
            CalculationProperties = new CalculationProperties(true);
            Shaft = new Shaft();
            KritikCalculation = new KritikCalculation();
            History = new CollectionHistory<ShaftElementForDataGrid>(Shaft.Elements);
            ShaftScheme = new ShaftScheme(Shaft, ShaftScheme1Width);
            ShaftScheme.SchemeMouseDown += (s, i) => OnShaftSchemeMouseDown(s, i);
            OscillationShapesViewModel = null;
            FileName = newCalculationFileName;
            NotifyPropertyChanged(nameof(ShaftOperatingSpeed));
            NotifyPropertyChanged(nameof(ShaftRunawaySpeed));
            NotifyPropertyChanged(nameof(OscillationShapesTabIsEnabled));
            AnyPropertyChanged = false;
            ShaftRotationInfluenceSelectedOption = ShaftRotationInfluenceOption.operatingSpeed;
        }

        /// <summary>
        /// Loads Kritik input .xlsx file and stores its data in <see cref="this.ShaftProperties"/>, <see cref="this.CalculationProperties"/> and <see cref="this.Shaft.Elements"/>
        /// </summary>
        internal void OpenFile(string fileName = null)
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
            KritikCalculation = new KritikCalculation();
            History = new CollectionHistory<ShaftElementForDataGrid>(Shaft.Elements);
            ShaftScheme = new ShaftScheme(Shaft, ShaftScheme1Width);
            ShaftScheme.SchemeMouseDown += (s, i) => OnShaftSchemeMouseDown(s, i);
            OscillationShapesViewModel = null;
            FileName = fileName;
            NotifyPropertyChanged(nameof(ShaftOperatingSpeed));
            NotifyPropertyChanged(nameof(ShaftRunawaySpeed));
            NotifyPropertyChanged(nameof(OscillationShapesTabIsEnabled));
            ShaftRotationInfluenceLoading();
            AnyPropertyChanged = false;
        }

        /// <summary>
        /// Saves data from <see cref="ShaftProperties"/>, <see cref="CalculationProperties"/>, <see cref="Shaft.Elements"/> and <see cref="KritikCalculation"/> into .xlsx file
        /// </summary>
        /// <param name="saveAs">opens save file dialog when true</param>
        private void SaveFile(bool saveAs)
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
            // Saving results, shaft and calculation properties stored in KritikCalculation
            bool saveResultsSuccess = DataLoadSave.SaveResults(FileName, KritikCalculation, Strings);
            // Saving data stored in Shaft and CalculationProperties
            bool saveDataSuccess = DataLoadSave.SaveData(FileName, Shaft, CalculationProperties);

            if (saveResultsSuccess && saveDataSuccess)
                AnyPropertyChanged = false;
            else
                MessageBox.Show("Soubor \"" + FileName + "\" se nepodařilo uložit.", "Chyba ukládání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private async Task RunCalculationAsync()
        {
            KritikCalculation = new KritikCalculation(Shaft, CalculationProperties);
            await KritikCalculation.CalculateCriticalSpeedsAsync();
            await KritikCalculation.CalculateOscillationShapesAsync();
            if (KritikCalculation.CriticalSpeeds.Length > 0)
                OscillationShapesViewModel = new OscillationShapesViewModel(KritikCalculation, ShaftScheme2, Strings);

            NotifyPropertyChanged(nameof(OscillationShapesTabIsEnabled));
            NotifyPropertyChanged(nameof(ShaftScheme2));
        }
        #endregion

        #region Other Methods
        /// <summary>
        /// Updates <see cref="Shaft.Properties.ShaftRPM"/> value according to <see cref="ShaftRotationInfluenceSelectedOption"/>
        /// </summary>
        private void ShaftRPMUpdate()
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
        private void ShaftRotationInfluenceLoading()
        {
            if (Shaft.Properties.ShaftRPM == Shaft.Properties.OperatingSpeed)
                ShaftRotationInfluenceSelectedOption = ShaftRotationInfluenceOption.operatingSpeed;
            else if (Shaft.Properties.ShaftRPM == Shaft.Properties.RunawaySpeed)
                ShaftRotationInfluenceSelectedOption = ShaftRotationInfluenceOption.runawaySpeed;
            else
            {
                ShaftRotationInfluenceSelectedOption = ShaftRotationInfluenceOption.custom;
                ShaftRotationInfluenceCustomValue = Shaft.Properties.ShaftRPM;
            }

            shaftRotationInfluenceControlsAreVisible = Shaft.Properties.ShaftRotationInfluence;

            NotifyPropertyChanged(nameof(ShaftRotationInfluenceVisibility));
            NotifyPropertyChanged(nameof(ShaftRotationInfluenceButtonSource));
            NotifyPropertyChanged(nameof(ShaftRotationInfluenceSelectedOption));
            NotifyPropertyChanged(nameof(ShaftRotationInfluenceCustomValue));
        }
        private void CellEditEnding()
        {
            BeamPlusControlsUpdate();
            ShaftScheme?.NotifyPropertyChanged(nameof(ShaftScheme.Scheme));
        }
        private void BeamPlusControlsUpdate([CallerMemberName] string caller = "")
        {
            if (caller != nameof(ShaftElementSelected))
                NotifyPropertyChanged(nameof(BeamPlusComboBoxItems));

            NotifyPropertyChanged(nameof(BeamPlusComboBoxSelectedItem));
            NotifyPropertyChanged(nameof(BeamPlusComboBoxIsNotEmpty));
            NotifyPropertyChanged(nameof(BeamPlusElementIsSelected));
            NotifyPropertyChanged(nameof(BeamPlusComboBoxSelectedItem));
            NotifyPropertyChanged(nameof(BeamPlusDivision));
            NotifyPropertyChanged(nameof(BeamPlusIdN));
            NotifyPropertyChanged(nameof(BeamPlusIdNValue));
            NotifyPropertyChanged(nameof(BeamPlusText));
        }
        /// <summary>
        /// Batch call of: <see cref="History"/>.Add(), invoking <see cref="SelectedElementChanged"/> and <see cref="BeamPlusControlsUpdate(string)"/>
        /// </summary>
        private void CommonBtnActions(bool addToHistory = true)
        {
            if (addToHistory)
                History.Add();

            int selectedElementIndex = Shaft.Elements.IndexOf(ShaftElementSelected);
            if (selectedElementIndex < 0)
                selectedElementIndex = Shaft.Elements.Count - 1;
            if (selectedElementIndex < 0)
                selectedElementIndex = 0;

            SelectedElementChanged?.Invoke(this, selectedElementIndex);
            BeamPlusControlsUpdate();
        }

        private void OnShaftSchemeMouseDown(object sender, int elementId)
        {
            ShaftElementSelected = Shaft.Elements[elementId];
            SelectedElementChanged?.Invoke(this, elementId);
        }

        #endregion
    }
}
