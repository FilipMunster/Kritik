using MathNet.Numerics.LinearAlgebra;
using System;
using System.Reflection;

namespace Kritik
{
    /// <summary>
    /// Prvek hřídele k použití pro výpočty
    /// </summary>
    public class ShaftElementWithMatrix : ShaftElement
    {
        public new ElementType Type { get { return type; } set { type = value == ElementType.beamPlus ? throw new ArgumentException("Element type cannot be the beamPlus") : value; } }
        private ElementType type;
        public double Rpm { get; set; }
        public double ShaftRPM { get; set; }
        public bool ShaftRotationInfluence { get; set; }
        public GyroscopicEffect Gyros { get; set; }
        public double Rho { get; set; }
        public double E { get; set; }
        public Matrix<double> Matrix => GetMatrix();

        private Matrix<double> GetMatrix()
        {
            switch (Type)
            {
                case ElementType.beam:
                    {
                        // Prvek hřídele:
                        double S = Math.PI * (Math.Pow(De, 2) - Math.Pow(Di, 2)) / 4.0;
                        double J = Math.PI / 4.0 * (Math.Pow(De / 2.0, 4) - Math.Pow(Di / 2.0, 4));
                        double omega = 2 * Math.PI * Rpm / 60.0;

                        double G = Math.Pow(Rho * S * Math.Pow(omega, 2) / (E * J), 1 / 4.0);
                        double V1 = 1 / 2.0 * (Math.Cosh(G * L) + Math.Cos(G * L));
                        double V2 = 1 / 2.0 * (Math.Sinh(G * L) + Math.Sin(G * L));
                        double V3 = 1 / 2.0 * (Math.Cosh(G * L) - Math.Cos(G * L));
                        double V4 = 1 / 2.0 * (Math.Sinh(G * L) - Math.Sin(G * L));

                        double[,] matrix = {{                    V1,                  V2/G, -V3/(Math.Pow(G,2)*E*J), -V4/(Math.Pow(G,3)*E*J) },
                                            {                  G*V4,                    V1,             -V2/(G*E*J), -V3/(Math.Pow(G,2)*E*J) },
                                            { -Math.Pow(G,2)*E*J*V3,             -G*E*J*V4,                      V1,                    V2/G },
                                            { -Math.Pow(G,3)*E*J*V2, -Math.Pow(G,2)*E*J*V3,                    G*V4,                      V1 }};
                        return Matrix<double>.Build.DenseOfArray(matrix);
                    }
                case ElementType.rigid:
                    {
                        // Tuhý prvek:
                        double[,] matrix = {{ 1.0,   L, 0.0, 0.0 },
                                            { 0.0, 1.0, 0.0, 0.0 },
                                            { 0.0, 0.0, 1.0,   L },
                                            { 0.0, 0.0, 0.0, 1.0 }};
                        return Matrix<double>.Build.DenseOfArray(matrix);
                    }
                case ElementType.disc:
                    {
                        // Prvek disku:
                        double omega = 2 * Math.PI * Rpm / 60.0; // úhlová rychlost vlastní frekvence
                        double omegaHridele = 2 * Math.PI * (ShaftRotationInfluence ? ShaftRPM : Rpm) / 60.0; // úhlová rychlost otáčení hřídele
                        double sign = 1.0; //znaménko u otáček hřídele;
                        double io = Io;
                        double id = Id;

                        switch (Gyros)
                        {
                            case GyroscopicEffect.forward:
                                break;
                            case GyroscopicEffect.backward:
                                sign = -1.0;
                                break;
                            case GyroscopicEffect.none:
                                io = 0;
                                id = 0;
                                break;
                            default:
                                throw new ArgumentNullException("Gyroscopic effect was not set.");
                        }

                        double[,] matrix = {{                  1.0,                                             0.0, 0.0, 0.0 },
                                            {                  0.0,                                             1.0, 0.0, 0.0 },
                                            {                  0.0, id*Math.Pow(omega,2)-io*omega*omegaHridele*sign, 1.0, 0.0 },
                                            { -M*Math.Pow(omega,2),                                             0.0, 0.0, 1.0 }};
                        return Matrix<double>.Build.DenseOfArray(matrix);
                    }
                case ElementType.support:
                    {
                        // Pružná podpora:
                        double[,] matrix = {{ 1.0, 0.0, 0.0, 0.0 },
                                            { 0.0, 1.0, 0.0, 0.0 },
                                            { 0.0, 0.0, 1.0, 0.0 },
                                            {   K, 0.0, 0.0, 1.0 }};
                        return Matrix<double>.Build.DenseOfArray(matrix);
                    }
                case ElementType.magnet:
                    {
                        // Magnet:
                        double[,] matrix = {{ 1.0, 0.0, 0.0, 0.0 },
                                            { 0.0, 1.0, 0.0, 0.0 },
                                            { 0.0, 0.0, 1.0, 0.0 },
                                            { -Cm, 0.0, 0.0, 1.0 }};
                        return Matrix<double>.Build.DenseOfArray(matrix);
                    }
                case ElementType.beamPlus:
                    throw new ArgumentNullException("Element of type \"BeamPlus\" cannot be computed.");
                default:
                    throw new ArgumentNullException("Element type was not set.");
            }
        }
        public ShaftElementWithMatrix(ElementType elementType = ElementType.beam) : base(elementType)
        {
            Type = elementType;
        }
        /// <summary>
        /// Create ShaftElementWithMatrix from ShaftElement
        /// </summary>
        /// <param name="shaftElement">Source ShaftElement object</param>
        public ShaftElementWithMatrix(ShaftElement shaftElement) : base(shaftElement.Type)
        {
            PropertyInfo[] parentProperties = typeof(ShaftElement).GetProperties();
            PropertyInfo[] thisProperties = this.GetType().GetProperties();
            foreach (var parentProperty in parentProperties)
            {
                foreach (var thisProperty in thisProperties)
                {
                    if (thisProperty.Name == parentProperty.Name && thisProperty.PropertyType == parentProperty.PropertyType)
                    {
                        thisProperty.SetValue(this, parentProperty.GetValue(shaftElement));
                        break;
                    }
                }
            }
        }
    }
}
