using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;

namespace Kritik
{
    internal static class Vypocet
    {
        /// <summary>
        /// Vypočte kritické otáčky hřídele, které uloží do vlastnosti KritOt.
        /// </summary>
        /// <param name="hridel">Instance hřídele třídy Hridel</param>
        /// <param name="rpmMax">Maximální hodnota otáček</param>
        /// <returns>Vrátí pole vypočtených kritických otáček</returns>
        public static (double[] kritOt, double[] rpmi, double[] detUc) KritickeOtacky(Hridel hridel, double rpmMax)
        {            
            List<Hridel.Prvek> prvky = hridel.PrvkyHridele;
            
            double step = 10; // krok otáček
            double[] detUc = new double[Convert.ToInt32(rpmMax / step)];
            double[] rpmi = new double[Convert.ToInt32(rpmMax / step)];
            List<double> kritOtList = new List<double>();
            int i = 0;

            for (double rpm = step; rpm <= rpmMax; rpm = rpm + step)
            {
                Matrix<double> uc = Matrix<double>.Build.DenseIdentity(4); // Jednotková matice 4x4
                
                // Nastavit všem prvkům rpm a vynásobit matice
                foreach (var prvek in prvky)
                {
                    prvek.Rpm = rpm;
                    uc.Multiply(prvek.Matice, uc);
                }

                uc = VytvorMatici2x2(uc, hridel.OpLeva, hridel.OpPrava);

                detUc[i]=uc.Determinant();
                rpmi[i] = rpm;

                if ((i > 0) && (detUc[i] * detUc[i - 1] < 0))
                {
                    kritOtList.Add(NajdiKritOt(hridel, rpmi[i - 1], rpmi[i]));
                }
                i++;        
            }

            double[] kritOt = kritOtList.ToArray();
            return (kritOt, rpmi, detUc);
        }
        /// <summary>
        /// Z celkové přenosové matice 4x4 vytvoří matici 2x2 s vybranými prvky na základě okrajových podmínek.
        /// </summary>
        /// <param name="matice">Matice 4x4</param>
        /// <param name="BCleft">Levá OP</param>
        /// <param name="BCright">Pravá OP</param>
        /// <returns></returns>
        private static Matrix<double> VytvorMatici2x2(Matrix<double> matice, string BCleft, string BCright)
        {
            int col1 = 0;
            int col2 = 0;
            int row1 = 0;
            int row2 = 0;

            switch (BCleft)
            {
                case Hridel.opVolnyKeyword:
                    col1 = 1;
                    col2 = 2;
                    break;
                case Hridel.opKloubKeyword:
                    col1 = 2;
                    col2 = 4;
                    break;
                case Hridel.opVetknutiKeyword:
                    col1 = 3;
                    col2 = 4;
                    break;
                default:
                    Console.WriteLine("Špatně zadaná okrajová podmínka ({0})", BCleft);
                    break;
            }

            switch (BCright)
            {
                case Hridel.opVolnyKeyword:
                    row1 = 3;
                    row2 = 4;
                    break;
                case Hridel.opKloubKeyword:
                    row1 = 1;
                    row2 = 3;
                    break;
                case Hridel.opVetknutiKeyword:
                    row1 = 1;
                    row2 = 2;
                    break;
                default:
                    Console.WriteLine("Špatně zadaná okrajová podmínka ({0})", BCright);
                    break;
            }

            // odčítám jedničku, protože se indexuje od 0 a já jsem indexy opsal z Matlabu
            col1--;
            col2--;
            row1--;
            row2--;

            var c1r1 = matice.Column(col1, row1, 1);
            var c2r1 = matice.Column(col2, row1, 1);
            var c1r2 = matice.Column(col1, row2, 1);
            var c2r2 = matice.Column(col2, row2, 1);

            Matrix<double>[,] maticeArray = { { c1r1.ToRowMatrix(), c2r1.ToRowMatrix() },
                                              { c1r2.ToRowMatrix(), c2r2.ToRowMatrix() } };
            matice = Matrix<double>.Build.DenseOfMatrixArray(maticeArray);
            return matice;
        }
        /// <summary>
        /// Najde přesnou hodnotu kritických otáček pomocí metody půlení intervalů
        /// </summary>
        /// <param name="prvky">Instance hřídele</param>
        /// <param name="rpmL"></param>
        /// <param name="rpmR"></param>
        /// <returns></returns>
        private static double NajdiKritOt(Hridel hridel, double rpmL, double rpmR)
        {
            List<Hridel.Prvek> prvky = hridel.PrvkyHridele;
            Matrix<double> uc;
            double rpmC = (rpmL + rpmR) / 2.0;
            double detUcL;
            double detUcC;

            while (Math.Abs(rpmL-rpmR)>Math.Pow(10,-9))
            {
                // Determinant pro otáčky vlevo
                uc = Matrix<double>.Build.DenseIdentity(4);
                foreach (var prvek in prvky)
                {
                    prvek.Rpm = rpmL;
                    uc.Multiply(prvek.Matice, uc);
                }
                uc = VytvorMatici2x2(uc, hridel.OpLeva, hridel.OpPrava);
                detUcL = uc.Determinant();

                // Determinant pro otáčky uprostřed
                uc = Matrix<double>.Build.DenseIdentity(4);
                foreach (var prvek in prvky)
                {
                    prvek.Rpm = rpmC;
                    uc.Multiply(prvek.Matice, uc);
                }
                uc = VytvorMatici2x2(uc, hridel.OpLeva, hridel.OpPrava);
                detUcC = uc.Determinant();

                if (detUcL*detUcC < 0)
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
