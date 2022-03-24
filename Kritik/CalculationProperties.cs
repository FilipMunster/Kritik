using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class CalculationProperties
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { 
            get { return author; }
            set { author = value; Properties.Settings.Default.author = value; }
        }
        private string author;
        public string Date { get; set; }
        public string Notes { get; set; }

        public CalculationProperties()
        {
            Author = Properties.Settings.Default.author == "" ? Environment.UserName : Properties.Settings.Default.author;
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
