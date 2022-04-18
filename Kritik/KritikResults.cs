using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class KritikResults : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private double[] criticalSpeeds;
        public double[] CriticalSpeeds
        {
            get => criticalSpeeds;
            set
            {
                criticalSpeeds = value;
                NotifyPropertyChanged();
            }
        }
        private OscillationShapes[] oscillationShapes;
        public OscillationShapes[] OscillationShapes
        {
            get => oscillationShapes;
            set
            {
                oscillationShapes = value;
                NotifyPropertyChanged();
            }
        }
        private Shaft shaftUsedForCalculation;
        /// <summary>
        /// Stores Clone of Shaft used for calculation
        /// </summary>
        public Shaft ShaftUsedForCalculation
        {
            get => shaftUsedForCalculation;
            set => shaftUsedForCalculation = (Shaft)value.Clone();
        }
        private CalculationProperties calculationPropertiesUsedForCalculation;
        public CalculationProperties CalculationPropertiesUsedForCalculation
        {
            get => calculationPropertiesUsedForCalculation;
            set => calculationPropertiesUsedForCalculation = (CalculationProperties)value.Clone();
        }
        public string GetCriticalSpeedText()
        {
            return "";
        }
        public string GetFirstCriticalSpeedRatioText()
        {
            return "";
        }
    }
}
