using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    interface IKritikData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string Notes { get; set; }
        public BoundaryCondition BCLeft { get; set; }
        public BoundaryCondition BCRight { get; set; }
        public GyroscopicEffect GyroscopicEffect { get; set; }
        public bool ShaftRotationInfluence { get; set; }
        public double ShaftRPM { get; set; }
        public double YoungModulus { get; set; }
        public double MaterialDensity { get; set; }
        public double ShaftOperatingSpeed { get; set; }
        public double ShaftRunawaySpeed { get; set; }
        public double MaxCriticalSpeed { get; set; }
        public Shaft Shaft { get; set; }
        public List<double> CriticalSpeeds { get; set; }
    }
}
