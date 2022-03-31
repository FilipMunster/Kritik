using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class CalculationProperties : INotifyPropertyChanged
    {
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

        public CalculationProperties()
        {
            if (Properties.Settings.Default.author is not null) 
            {
                Author = Properties.Settings.Default.author == "" ? Environment.UserName : Properties.Settings.Default.author;
            }
            else
            {
                Author = Environment.UserName;
            }
            
            Date = DateTime.Today.ToShortDateString();
        }

        /// <summary>
        /// Returns deep copy of the object
        /// </summary>
        public CalculationProperties Copy()
        {
            return (CalculationProperties)this.MemberwiseClone();
        }
    }
}
