using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Kritik
{
    public partial class KritikCalculation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public KritikCalculation() { }
        /// <summary>
        /// Creates <see cref="KritikCalculation"/> object using Clones of <see cref="Shaft"/> and <see cref="CalculationProperties"/>
        /// </summary>
        /// <param name="shaft">Instance of <see cref="Shaft"/> to be used for calculation</param>
        /// <param name="calculationProperties">Instance of <see cref="CalculationProperties"/> to be used for calculation</param>
        public KritikCalculation(Shaft shaft, CalculationProperties calculationProperties)
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
                text += "Otáčky hřídele = " + Shaft.Properties.ShaftRPM + " rpm:\n";

            if (CriticalSpeeds is null || CriticalSpeeds.Count() == 0)
            {
                text += "Pro zadaný rotor nebyly\nvypočteny žádné kritické otáčky.";
                return text;
            }

            for (int i = 0; i < CriticalSpeeds.Count(); i++)
            {
                text += (i + 1) + ". kritické otáčky: " + String.Format("{0:0.000}", CriticalSpeeds[i]) + " rpm\n";
            }
            return text;
        }
        private string GetFirstCriticalSpeedRatioText()
        {
            string text = "";
            if (CriticalSpeeds is null || CriticalSpeeds.Count() == 0)
                return text;

            if (Shaft.Properties.OperatingSpeed > 0)
                text += String.Format("{0:0.000}", CriticalSpeeds[0] / Shaft.Properties.OperatingSpeed * 100) + " % provozních otáček\n";
            if (Shaft.Properties.RunawaySpeed > 0)
                text += String.Format("{0:0.000}", CriticalSpeeds[0] / Shaft.Properties.RunawaySpeed * 100) + " % provozních otáček";
            return text;
        }

        public async Task CalculateCriticalSpeedsAsync()
        {
            if (Shaft is null || CalculationProperties is null)
                throw new ArgumentNullException();

            CriticalSpeedsText = "Probíhá výpočet...";
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
    }
}
