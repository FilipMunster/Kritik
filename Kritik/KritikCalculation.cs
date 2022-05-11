using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace Kritik
{
    public partial class KritikCalculation : INotifyPropertyChanged
    {
        /// <summary>
        /// Resource dictionary containing currently selected language
        /// </summary>
        public readonly ResourceDictionary resourceDictionary;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public KritikCalculation() 
        {
            this.resourceDictionary = Application.Current.Resources.MergedDictionaries.Last();
        }
        /// <summary>
        /// Creates <see cref="KritikCalculation"/> object using Clones of <see cref="Shaft"/> and <see cref="CalculationProperties"/>
        /// </summary>
        /// <param name="shaft">Instance of <see cref="Shaft"/> to be used for calculation</param>
        /// <param name="calculationProperties">Instance of <see cref="CalculationProperties"/> to be used for calculation</param>
        public KritikCalculation(Shaft shaft, CalculationProperties calculationProperties) : this()
        {
            Shaft = (Shaft)shaft.Clone();
            CalculationProperties = (CalculationProperties)calculationProperties.Clone();
        }
        /// <summary>
        /// Shaft used for calculation
        /// </summary>
        public Shaft Shaft { get; private set; }
        /// <summary>
        /// CalculationProperties for calculation
        /// </summary>
        public CalculationProperties CalculationProperties { get; private set; }
        private string criticalSpeedsText;
        public string CriticalSpeedsText
        {
            get => criticalSpeedsText;
            private set
            {
                criticalSpeedsText = value;
                NotifyPropertyChanged();
            }
        }
        private string firstCriticalSpeedRatioText;
        public string FirstCriticalSpeedRatioText
        {
            get => firstCriticalSpeedRatioText;
            private set
            {
                firstCriticalSpeedRatioText = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsCalculationInProgress { get; private set; }
        public double[] CriticalSpeeds { get; private set; }
        public OscillationShapes[] OscillationShapes { get; private set; }
        /// <summary>
        /// True if all necessary properties are set
        /// </summary>
        public bool IsReady => Shaft is not null && CalculationProperties is not null;

        private string GetCriticalSpeedText()
        {
            string text = "";

            if (Shaft.Properties.ShaftRotationInfluence)
                text += this.resourceDictionary["Shaft_speed"] + " = " + Shaft.Properties.ShaftRPM + " rpm:\n";

            if (CriticalSpeeds is null || CriticalSpeeds.Count() == 0)
            {
                text += (string)this.resourceDictionary["No_critical_speeds_were_calculated"];
                return text;
            }

            for (int i = 0; i < CriticalSpeeds.Count(); i++)
            {
                text += OrdinalNumber(i + 1) + " " + this.resourceDictionary["critical_speed"] + ": " 
                    + String.Format("{0:0.000}", CriticalSpeeds[i]) + " rpm\n";
            }
            return text;
        }
        private string GetFirstCriticalSpeedRatioText()
        {
            string text = "";
            if (CriticalSpeeds is null || CriticalSpeeds.Count() == 0)
                return text;

            if (Shaft.Properties.OperatingSpeed > 0)
                text += String.Format("{0:0.000}", CriticalSpeeds[0] / Shaft.Properties.OperatingSpeed * 100) + 
                    " % " + this.resourceDictionary["of_operating_speed"] + "\n";
            if (Shaft.Properties.RunawaySpeed > 0)
                text += String.Format("{0:0.000}", CriticalSpeeds[0] / Shaft.Properties.RunawaySpeed * 100) + 
                    " % " +this.resourceDictionary["of_runaway_speed"];
            return text;
        }

        public async Task CalculateCriticalSpeedsAsync()
        {
            if (Shaft is null || CalculationProperties is null)
                throw new ArgumentNullException();

            CriticalSpeedsText = (string)this.resourceDictionary["Calculation_in_progress"];
            IsCalculationInProgress = true;

            CalculationMethods calculation = new CalculationMethods();

            CriticalSpeeds = await Task.Run(() => calculation.CriticalSpeed(Shaft.GetElementsWithMatrix(),
                Shaft.Properties.BCLeft, Shaft.Properties.BCRight, Shaft.Properties.MaxCriticalSpeed));

            CriticalSpeedsText = GetCriticalSpeedText();
            FirstCriticalSpeedRatioText = GetFirstCriticalSpeedRatioText();
            IsCalculationInProgress = false;
        }
        public async Task CalculateOscillationShapesAsync()
        {
            if (Shaft is null || CalculationProperties is null)
                throw new ArgumentNullException();

            if (CriticalSpeeds is null)
                await Task.Run(() => CalculateCriticalSpeedsAsync());

            IsCalculationInProgress = true;
            CalculationMethods calculation = new CalculationMethods();
            OscillationShapes = await Task.Run(() => calculation.OscillationShapes(Shaft.GetElementsWithMatrix(),
                Shaft.Properties.BCLeft, Shaft.Properties.BCRight, CriticalSpeeds));
            IsCalculationInProgress = false;
        }

        /// <summary>
        /// Returns string with ordinal number of given int according to language set in <see cref="SelectedLanguage"/>
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string OrdinalNumber(int number)
        {
            if ((string)this.resourceDictionary["@languageName"] != "english")
                return number + ".";

            int num = Math.Abs(number) % 100;
            if (num > 10 && num < 20)
                return number + "th";

            switch (num % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }
    }
}
