using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    /// <summary>
    /// Shaft Element
    /// </summary>
    public class ShaftElement
    {
        /// <summary>
        /// Shaft Element Type
        /// </summary>
        public ElementType Type { get; set; }
        /// <summary>
        /// Elements Length
        /// </summary>
        public double L { get { return Type == ElementType.disc || Type == ElementType.support || Type == ElementType.magnet ? 0 : l; } set { l = value; } }
        private double l;
        /// <summary>
        /// Element outer diameter
        /// </summary>
        public double De { get; set; }
        /// <summary>
        /// Element inner diameter
        /// </summary>
        public double Di { get; set; }
        /// <summary>
        /// Disc weight
        /// </summary>
        public double M { get; set; }
        /// <summary>
        /// Moment of inertia parallel to the rotor axis
        /// </summary>
        public double Io { get; set; }
        /// <summary>
        /// Moment of inertia perpendicular to the rotor axis
        /// </summary>
        public double Id { get; set; }
        /// <summary>
        /// Radial support stiffness
        /// </summary>
        public double K { get; set; }
        /// <summary>
        /// Magnetic-elastic constant
        /// </summary>
        public double Cm { get; set; }
        /// <summary>
        /// Number od divisions of Beam+ element
        /// </summary>
        public double Division { get; set; }
        /// <summary>
        /// Method of Id/N calculation for Beam+ element: IdN=0 -> Idi=Id/n*IdNValue, IdN=1 -> Idi=IdNValue
        /// </summary>
        public int IdN { get; set; }
        /// <summary>
        /// Value for calculation of Idi
        /// </summary>
        public double IdNValue { get; set; }

        public ShaftElement(ElementType elementType = ElementType.beam)
        {
            Type = elementType;
        }
    }
}
