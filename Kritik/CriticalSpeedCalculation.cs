using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    /// <summary>
    /// Třída pro výpočet kritických otáček
    /// </summary>
    internal class CriticalSpeedCalculation
    {
        private readonly List<ShaftElementWithMatrix> shaftElements;
        private readonly double rpmMax;
        private readonly BoundaryCondition BCLeft;
        private readonly BoundaryCondition BCRight;
        public CriticalSpeedCalculation(List<ShaftElementWithMatrix> shaftElements, double rpmMax, BoundaryCondition BCLeft, BoundaryCondition BCRight)
        {
            this.shaftElements = new List<ShaftElementWithMatrix>(shaftElements);
            this.rpmMax = rpmMax;
            this.BCLeft = BCLeft;
            this.BCRight = BCRight;
        }
        /// <summary>
        /// Vypočítá kritické otáčky hřídele
        /// </summary>
        /// <returns>Vrátí pole kritických otáček</returns>
        public double[] Compute()
        {
            double step = 10; // krok otáček
            double[] detUc = new double[Convert.ToInt32(rpmMax / step)];
            double[] rpmi = new double[Convert.ToInt32(rpmMax / step)];
            List<double> critSpeedList = new List<double>();
            int i = 0;

            for (double rpm = step; rpm <= rpmMax; rpm = rpm + step)
            {
                Matrix<double> uc = Matrix<double>.Build.DenseIdentity(4); // Jednotková matice 4x4

                // Nastavit všem prvkům rpm a vynásobit matice
                foreach (ShaftElementWithMatrix element in shaftElements)
                {
                    element.Rpm = rpm;
                    uc.Multiply(element.Matrix, uc);
                }

                uc = GetMatrix2x2(uc);

                detUc[i] = uc.Determinant();
                rpmi[i] = rpm;

                if ((i > 0) && (detUc[i] * detUc[i - 1] < 0))
                {
                    critSpeedList.Add(FindCriticalSpeed(rpmi[i - 1], rpmi[i]));
                }
                i++;
            }

            return critSpeedList.ToArray();
        }
            
        /// <summary>
        /// Z celkové přenosové matice 4x4 vytvoří matici 2x2 s vybranými prvky na základě okrajových podmínek.
        /// </summary>
        /// <param name="matrix">Matice 4x4</param>
        /// <returns>Vrátí matici 2x2 na základě okrajových podmínek</returns>
        private Matrix<double> GetMatrix2x2(Matrix<double> matrix)
        {
            int col1;
            int col2;
            int row1;
            int row2;

            switch (BCLeft)
            {
                case BoundaryCondition.free:
                    col1 = 1;
                    col2 = 2;
                    break;
                case BoundaryCondition.joint:
                    col1 = 2;
                    col2 = 4;
                    break;
                case BoundaryCondition.fix:
                    col1 = 3;
                    col2 = 4;
                    break;
                default:
                    throw new ArgumentNullException(nameof(BCLeft));
            }

            switch (BCRight)
            {
                case BoundaryCondition.free:
                    row1 = 3;
                    row2 = 4;
                    break;
                case BoundaryCondition.joint:
                    row1 = 1;
                    row2 = 3;
                    break;
                case BoundaryCondition.fix:
                    row1 = 1;
                    row2 = 2;
                    break;
                default:
                    throw new ArgumentNullException(nameof(BCRight));
            }

            // odčítám jedničku, protože se indexuje od 0 a já jsem indexoval od 1.
            col1--;
            col2--;
            row1--;
            row2--;

            Vector<double> c1r1 = matrix.Column(col1, row1, 1);
            Vector<double> c2r1 = matrix.Column(col2, row1, 1);
            Vector<double> c1r2 = matrix.Column(col1, row2, 1);
            Vector<double> c2r2 = matrix.Column(col2, row2, 1);

            Matrix<double>[,] maticeArray = { { c1r1.ToRowMatrix(), c2r1.ToRowMatrix() },
                                              { c1r2.ToRowMatrix(), c2r2.ToRowMatrix() } };
            return Matrix<double>.Build.DenseOfMatrixArray(maticeArray);
        }

        /// <summary>
        /// Najde přesnou hodnotu kritických otáček pomocí metody půlení intervalů
        /// </summary>
        /// <param name="rpmL">Levá strana intervalu otáček</param>
        /// <param name="rpmR">Pravá strana intervalu otáček</param>
        /// <returns></returns>
        private double FindCriticalSpeed(double rpmL, double rpmR)
        {
            Matrix<double> uc;
            double rpmC = (rpmL + rpmR) / 2.0;
            double detUcL;
            double detUcC;
            double tol = Math.Pow(10, -9);

            while (Math.Abs(rpmL - rpmR) > tol)
            {
                // Determinant pro otáčky vlevo
                uc = Matrix<double>.Build.DenseIdentity(4);
                foreach (var element in shaftElements)
                {
                    element.Rpm = rpmL;
                    uc.Multiply(element.Matrix, uc);
                }
                uc = GetMatrix2x2(uc);
                detUcL = uc.Determinant();

                // Determinant pro otáčky uprostřed
                uc = Matrix<double>.Build.DenseIdentity(4);
                foreach (var prvek in shaftElements)
                {
                    prvek.Rpm = rpmC;
                    uc.Multiply(prvek.Matrix, uc);
                }
                uc = GetMatrix2x2(uc);
                detUcC = uc.Determinant();

                if (detUcL * detUcC < 0)
                {
                    rpmR = rpmC;
                }
                else
                {
                    rpmL = rpmC;
                }
                rpmC = (rpmL + rpmR) / 2;
            }
            return rpmC;
        }
    }
}
