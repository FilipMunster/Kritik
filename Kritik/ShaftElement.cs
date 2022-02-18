using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    /// <summary>
    /// Prvek hřídele
    /// </summary>
    class ShaftElement
    {
        /// <summary>
        /// Typ prvku hřídele
        /// </summary>
        public ElementType Type { get; set; }
        /// <summary>
        /// Délka prvku
        /// </summary>
        public double L { get { return Type == ElementType.disc || Type == ElementType.support || Type == ElementType.magnet ? 0 : l; } set { l = value; } }
        private double l;
        /// <summary>
        /// Vnější průměr
        /// </summary>
        public double De { get; set; }
        /// <summary>
        /// Vrtání hřídele
        /// </summary>
        public double Di { get; set; }
        /// <summary>
        /// Hmotnost disku
        /// </summary>
        public double M { get; set; }
        /// <summary>
        /// Moment setrvačnosti k ose rotoru
        /// </summary>
        public double Io { get; set; }
        /// <summary>
        /// Moment setrvačnosti kolmo k ose otáčení
        /// </summary>
        public double Id { get; set; }
        /// <summary>
        /// Radiální tuhost podpory
        /// </summary>
        public double K { get; set; }
        /// <summary>
        /// Magneticko elastická konstanta
        /// </summary>
        public double Cm { get; set; }
        /// <summary>
        /// Počet dělení prvku Hřídel+
        /// </summary>
        public double Division { get; set; }
        /// <summary>
        /// Způsob dělení Hřídele+: IdN=0 -> Idi=Id/n*IdNValue, IdN=0 -> Idi=IdNValue
        /// </summary>
        public int IdN { get; set; }
        /// <summary>
        /// Hodnota pro výpočet Idi
        /// </summary>
        public double IdNValue { get; set; }

        public ShaftElement(ElementType elementType = ElementType.beam)
        {
            Type = elementType;
        }
    }
}
