using System.Collections.Generic;

namespace Kritik
{
    public class OscillationShapes
    {
        public double Rpm { get; }
        public Shape W { get; set; }
        public Shape Phi { get; set; }
        public Shape M { get; set; }
        public Shape T { get; set; }
        public OscillationShapes(double rpm)
        {
            Rpm = rpm;
        }

        public class Shape
        {
            public double[] X { get; }
            public double[] Y { get; }
            public double[] XNodes { get; }
            public double[] YNodes { get; }
            public Shape(double[] x, double[] y, double[] xNodes, double[] yNodes)
            {
                X = x;
                Y = y;
                XNodes = xNodes;
                YNodes = yNodes;
            }
            public Shape(List<double> x, List<double> y, List<double> xNodes, List<double> yNodes)
            {
                X = x.ToArray();
                Y = y.ToArray();
                XNodes = xNodes.ToArray();
                YNodes = yNodes.ToArray();
            }
        }
    }
}
