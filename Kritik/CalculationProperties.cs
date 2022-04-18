using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class CalculationProperties : INotifyPropertyChanged, ICloneable
    {
        private bool newCalculation;
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                NotifyPropertyChanged();
            }
        }
        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                NotifyPropertyChanged();
            }
        }

        private string author;
        public string Author {
            get => author;
            set 
            { 
                author = value;
                if (newCalculation)
                    Properties.Settings.Default.author = value;
                NotifyPropertyChanged();
            }
        }

        private string date;
        public string Date
        {
            get => date;
            set { date = value; NotifyPropertyChanged(); }
        }
        private string notes;
        public string Notes
        {
            get => notes;
            set { notes = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// Create new instance of <see cref="CalculationProperties"/>
        /// </summary>
        /// <param name="newCalculation">Set to true if the instance is for new calculation (e.g. false when loading data from file)</param>
        public CalculationProperties(bool newCalculation = false)
        {
            this.newCalculation = newCalculation;
            Author = Properties.Settings.Default.author == "" ? Environment.UserName : Properties.Settings.Default.author;            
            Date = DateTime.Today.ToShortDateString();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
