using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class ShaftProperties
    {
        public BoundaryCondition BCLeft { get; set; }
        public BoundaryCondition BCRight { get; set; }
        public GyroscopicEffect Gyros { get; set; }
        public bool ShaftRotationInfluence { get; set; }
        public double ShaftRPM { get; set; }
        public double YoungModulus { get; set; }
        public double MaterialDensity { get; set; }
        public double OperatingSpeed { get; set; }
        public double RunawaySpeed { get; set; }
        public double MaxCriticalSpeed { get; set; }

        public ShaftProperties()
        {
            BCLeft = BoundaryCondition.free;
            BCRight = BoundaryCondition.free;
            Gyros = GyroscopicEffect.none;
            ShaftRotationInfluence = false;
            ShaftRPM = 0;
            YoungModulus = 210e9;
            MaterialDensity = 7850;
            OperatingSpeed = 0;
            RunawaySpeed = 0;
            MaxCriticalSpeed = 5000;
        }
        /// <summary>
        /// Returns deep copy of the object
        /// </summary>
        public ShaftProperties Copy()
        {
            return (ShaftProperties)this.MemberwiseClone();
        }
    }
}
