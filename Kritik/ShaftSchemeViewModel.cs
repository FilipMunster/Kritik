using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class ShaftSchemeViewModel : INotifyPropertyChanged
    {
        private Shaft shaft;
        public ShaftSchemeViewModel(Shaft shaft)
        {
            XMin = 0;
            XMax = 1;
            this.shaft = shaft;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public double XMin { get; set; }
        public double XMax { get; set; }
    }
}
