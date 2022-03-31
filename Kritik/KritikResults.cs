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
        private OscillationShape[] oscillationShapes;
        public OscillationShape[] OscillationShapes
        {
            get => oscillationShapes;
            set
            {
                oscillationShapes = value;
                NotifyPropertyChanged();
            }
        }
    }
}
