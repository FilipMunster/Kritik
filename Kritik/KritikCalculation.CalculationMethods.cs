using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public partial class KritikCalculation
    {
        /// <summary>
        /// Contains methods for calculations associated with Shaft critical speed
        /// </summary>
        public class CalculationMethods
        {
            /// <summary>
            /// Calculates Shaft critical speed
            /// </summary>
            /// <returns>Array of critical speed values</returns>
            public double[] CriticalSpeed(List<ShaftElementWithMatrix> shaftElements, BoundaryCondition bCLeft, BoundaryCondition bCRight, double rpmMax)
            {
                double rpmStep = 10;
                double[] detUc = new double[Convert.ToInt32(rpmMax / rpmStep)];
                double[] rpmi = new double[Convert.ToInt32(rpmMax / rpmStep)];
                List<double> critSpeedList = new List<double>();
                int i = 0;

                for (double rpm = rpmStep; rpm <= rpmMax; rpm = rpm + rpmStep)
                {
                    Matrix<double> uc = Matrix<double>.Build.DenseIdentity(4); // Indentity matrix 4x4

                    // Set RPM to all elements and multiply matrices
                    foreach (ShaftElementWithMatrix element in shaftElements)
                    {
                        element.Rpm = rpm;
                        uc.Multiply(element.Matrix, uc);
                    }

                    uc = GetMatrix2x2(uc, bCLeft, bCRight);

                    detUc[i] = uc.Determinant();
                    rpmi[i] = rpm;

                    if ((i > 0) && (detUc[i] * detUc[i - 1] < 0))
                    {
                        critSpeedList.Add(FindCriticalSpeed(shaftElements, bCLeft, bCRight, rpmi[i - 1], rpmi[i]));
                    }
                    i++;
                }
                return critSpeedList.ToArray();
            }
            /// <summary>
            /// Calculates Shaft oscillation shapes for given critical speeds
            /// </summary>
            /// <returns>Array of oscillation shapes</returns>
            public OscillationShapes[] OscillationShapes(List<ShaftElementWithMatrix> shaftElements,
                BoundaryCondition bCLeft, BoundaryCondition bCRight, double[] criticalSpeeds)
            {
                OscillationShapes[] shapes = new OscillationShapes[criticalSpeeds.Length];

                const int shaftDivision = 1000;
                double shaftLength = 0;

                for (int i = 0; i < shaftElements.Count; i++)
                {
                    shaftLength += shaftElements[i].L;
                }
                double dx = shaftLength / shaftDivision;

                for (int i = 0; i < criticalSpeeds.Length; i++)
                {
                    shapes[i] = new OscillationShapes(criticalSpeeds[i]);

                    Matrix<double> uc = Matrix<double>.Build.DenseIdentity(4); // Identity matrix 4x4
                    
                    // Set RPM to all elements and multiply the matrices
                    foreach (var element in shaftElements)
                    {
                        element.Rpm = shapes[i].Rpm;
                        element.Matrix.Multiply(uc, uc);
                    }
                    uc = GetMatrix2x2(uc, bCLeft, bCRight); // celková přenosová matice pro dané kritické otáčky

                    // Solution according to Pilkey: FORMULAS FOR STRESS, STRAIN, AND STRUCTURAL MATRICES, pages 1426 - 1428
                    double ucRatio = -uc[1, 0] / uc[1, 1]; // ratio between two unknown values at the left shaft end

                    // Oscillation shapes in nodes
                    List<double> wNodes = new(shaftElements.Count + 1);
                    List<double> phiNodes = new(shaftElements.Count + 1);
                    List<double> mNodes = new(shaftElements.Count + 1);
                    List<double> tNodes = new(shaftElements.Count + 1);
                    List<double> xNodes = new(shaftElements.Count + 1);

                    // Oscillation shapes through whole shaft length
                    List<double> w = new(shaftElements.Count + shaftDivision);
                    List<double> phi = new(shaftElements.Count + shaftDivision);
                    List<double> m = new(shaftElements.Count + shaftDivision);
                    List<double> t = new(shaftElements.Count + shaftDivision);
                    List<double> x = new(shaftElements.Count + shaftDivision);

                    switch (bCLeft)
                    {
                        case BoundaryCondition.free:
                            wNodes.Add(1);
                            phiNodes.Add(ucRatio);
                            mNodes.Add(0);
                            tNodes.Add(0);
                            break;
                        case BoundaryCondition.joint:
                            wNodes.Add(0);
                            phiNodes.Add(1);
                            mNodes.Add(0);
                            tNodes.Add(ucRatio);
                            break;
                        case BoundaryCondition.fix:
                            wNodes.Add(0);
                            phiNodes.Add(0);
                            mNodes.Add(1);
                            tNodes.Add(ucRatio);
                            break;
                        default:
                            break;
                    }
                    xNodes.Add(0);

                    w.Add(wNodes[0]);
                    phi.Add(phiNodes[0]);
                    m.Add(mNodes[0]);
                    t.Add(tNodes[0]);
                    x.Add(xNodes[0]);

                    foreach (var element in shaftElements)
                    {
                        Vector<double> v = Vector<double>.Build.DenseOfArray(new[] { wNodes.Last(), phiNodes.Last(), mNodes.Last(), tNodes.Last() }); // vektor levého konce prvku

                        if (element.L > 0)
                        {
                            double lengthOriginal = element.L; // uložím si původní délku prvku, abych nemusel vytvářet prvek nový. L budu měnit, na konci mu původní délku vrátím.
                            double xRel = dx;

                            while (xRel < lengthOriginal)
                            {
                                element.L = xRel;
                                Vector<double> vX = element.Matrix.Multiply(v);
                                w.Add(vX[0]);
                                phi.Add(vX[1]);
                                m.Add(vX[2]);
                                t.Add(vX[3]);
                                x.Add(x.Last() + dx);
                                xRel += dx;
                            }
                            element.L = lengthOriginal;
                        }

                        // shape at the end of the element (in node)
                        Vector<double> vNext = element.Matrix.Multiply(v);

                        wNodes.Add(vNext[0]);
                        phiNodes.Add(vNext[1]);
                        mNodes.Add(vNext[2]);
                        tNodes.Add(vNext[3]);
                        xNodes.Add(xNodes.Last() + element.L);

                        w.Add(vNext[0]);
                        phi.Add(vNext[1]);
                        m.Add(vNext[2]);
                        t.Add(vNext[3]);
                        x.Add(xNodes.Last());
                    }

                    shapes[i].W = new OscillationShapes.Shape(x, w, xNodes, wNodes);
                    shapes[i].Phi = new OscillationShapes.Shape(x, phi, xNodes, phiNodes);
                    shapes[i].M = new OscillationShapes.Shape(x, m, xNodes, mNodes);
                    shapes[i].T = new OscillationShapes.Shape(x, t, xNodes, tNodes);
                }
                return shapes;
            }


            /// <summary>
            /// Z celkové přenosové matice 4x4 vytvoří matici 2x2 s vybranými prvky na základě okrajových podmínek.
            /// </summary>
            /// <param name="matrix">Matice 4x4</param>
            /// <returns>Vrátí matici 2x2 na základě okrajových podmínek</returns>
            private Matrix<double> GetMatrix2x2(Matrix<double> matrix, BoundaryCondition bCLeft, BoundaryCondition bCRight)
            {
                int col1;
                int col2;
                int row1;
                int row2;

                switch (bCLeft)
                {
                    case BoundaryCondition.free:
                        col1 = 0;
                        col2 = 1;
                        break;
                    case BoundaryCondition.joint:
                        col1 = 1;
                        col2 = 3;
                        break;
                    case BoundaryCondition.fix:
                        col1 = 2;
                        col2 = 3;
                        break;
                    default:
                        throw new ArgumentNullException(nameof(bCLeft));
                }

                switch (bCRight)
                {
                    case BoundaryCondition.free:
                        row1 = 2;
                        row2 = 3;
                        break;
                    case BoundaryCondition.joint:
                        row1 = 0;
                        row2 = 2;
                        break;
                    case BoundaryCondition.fix:
                        row1 = 0;
                        row2 = 1;
                        break;
                    default:
                        throw new ArgumentNullException(nameof(bCRight));
                }

                Vector<double> c1r1 = matrix.Column(col1, row1, 1);
                Vector<double> c2r1 = matrix.Column(col2, row1, 1);
                Vector<double> c1r2 = matrix.Column(col1, row2, 1);
                Vector<double> c2r2 = matrix.Column(col2, row2, 1);

                Matrix<double>[,] matrixArray = { { c1r1.ToRowMatrix(), c2r1.ToRowMatrix() },
                                              { c1r2.ToRowMatrix(), c2r2.ToRowMatrix() } };
                return Matrix<double>.Build.DenseOfMatrixArray(matrixArray);
            }

            /// <summary>
            /// Najde přesnou hodnotu kritických otáček pomocí metody půlení intervalů
            /// </summary>
            /// <param name="rpmL">Levá strana intervalu otáček</param>
            /// <param name="rpmR">Pravá strana intervalu otáček</param>
            /// <returns></returns>
            private double FindCriticalSpeed(List<ShaftElementWithMatrix> shaftElements, BoundaryCondition bCLeft, BoundaryCondition bCRight, double rpmL, double rpmR)
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
                    uc = GetMatrix2x2(uc, bCLeft, bCRight);
                    detUcL = uc.Determinant();

                    // Determinant pro otáčky uprostřed
                    uc = Matrix<double>.Build.DenseIdentity(4);
                    foreach (var prvek in shaftElements)
                    {
                        prvek.Rpm = rpmC;
                        uc.Multiply(prvek.Matrix, uc);
                    }
                    uc = GetMatrix2x2(uc, bCLeft, bCRight);
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
}
