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
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            AnyPropertyChanged = true;
        }

        #region Properties for Calculation Data
        public ShaftProperties ShaftProperties { get; set; }
        public CalculationProperties CalculationProperties { get; set; }
        public KritikResults KritikResults { get; set; }
        public Shaft Shaft { get; set; }
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
                if (AnyPropertyChanged) { title += " *"; }
                title += "]";
                return title;
            }
        }
        public bool AnyPropertyChanged
        {
            get { return anyPropertyChanged; }
            set
            {
                anyPropertyChanged = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitle)));
            }
        }
        private bool anyPropertyChanged;
        public Strings Strings { get; set; }
        public string FileName { get; set; }
        public PlotModel ShaftScheme { get; set; }
        #endregion

        #region Commands
        private ICommand newCalculationCommand;
        public ICommand NewCalculationCommand => newCalculationCommand ??= new CommandHandler(() => InitializeNewCalculation(), () => true);




        #endregion

        #region Button actions
        private void InitializeNewCalculation()
        {
            ShaftProperties = new ShaftProperties();
            CalculationProperties = new CalculationProperties();
            KritikResults = new KritikResults();
            Shaft = new Shaft();
            History = new CollectionHistory<ShaftElementForDataGrid>(Shaft.Elements);
            FileName = newCalculationFileName;
        }



        #endregion
    }
}
