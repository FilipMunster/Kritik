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
    }
}
