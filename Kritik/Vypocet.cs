using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;
using System.Diagnostics;

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
                    Debug.WriteLine("Špatně zadaná okrajová podmínka ({0})", BCleft);
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
                    Debug.WriteLine("Špatně zadaná okrajová podmínka ({0})", BCright);
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
        /// <param name="hridel">Instance hřídele</param>
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

        public static TvarKmitu[] TvaryKmitu(Hridel hridel, double[] kritOt = default)
        {
            if (kritOt == null)
            {
                if (hridel.KritOt != null)
                {
                    kritOt = hridel.KritOt;
                }
                else { return null; }                
            }
            TvarKmitu[] tvary = new TvarKmitu[kritOt.Length];
            List<Hridel.Prvek> prvky = hridel.PrvkyHridele;

            const int deleniHridele = 1000;
            double delkaHridele = 0;
            foreach (Hridel.Prvek p in prvky)
            {
                delkaHridele = delkaHridele + p.L;
            }
            double dx = delkaHridele / deleniHridele;

            for (int i = 0; i < kritOt.Length; i++)
            {
                tvary[i] = new TvarKmitu();
                tvary[i].Rpm = kritOt[i];

                Matrix<double> uc = Matrix<double>.Build.DenseIdentity(4); // Jednotková matice 4x4
                // Nastavit všem prvkům rpm a vynásobit matice
                foreach (var prvek in prvky)
                {
                    prvek.Rpm = tvary[i].Rpm;
                    prvek.Matice.Multiply(uc, uc);
                }
                uc = VytvorMatici2x2(uc, hridel.OpLeva, hridel.OpPrava); // celková přenosová matice pro dané kritické otáčky

                // Řešení dle Pilkey: FORMULAS FOR STRESS, STRAIN, AND STRUCTURAL MATRICES, str. 1426 - 1428
                double ucRatio = -uc[1, 0] / uc[1, 1]; // poměr hodnot mezi dvěma neznámými hodnotami na levém konci

                // Tvary kmitů v uzlech
                List<double> wUzly = new List<double>(prvky.Count + 1);
                List<double> phiUzly = new List<double>(prvky.Count + 1);
                List<double> mUzly = new List<double>(prvky.Count + 1);
                List<double> tUzly = new List<double>(prvky.Count + 1);
                List<double> xUzly = new List<double>(prvky.Count + 1);

                // Tvary kmitů po celé délce hřídele
                List<double> w = new List<double>(prvky.Count + deleniHridele);
                List<double> phi = new List<double>(prvky.Count + deleniHridele);
                List<double> m = new List<double>(prvky.Count + deleniHridele);
                List<double> t = new List<double>(prvky.Count + deleniHridele);
                List<double> x = new List<double>(prvky.Count + deleniHridele);

                switch (hridel.OpLeva)
                {
                    case Hridel.opVolnyKeyword:
                        wUzly.Add(1);
                        phiUzly.Add(ucRatio);
                        mUzly.Add(0);
                        tUzly.Add(0);
                        break;
                    case Hridel.opKloubKeyword:
                        wUzly.Add(0);
                        phiUzly.Add(1);
                        mUzly.Add(0);
                        tUzly.Add(ucRatio);
                        break;
                    case Hridel.opVetknutiKeyword:
                        wUzly.Add(0);
                        phiUzly.Add(0);
                        mUzly.Add(1);
                        tUzly.Add(ucRatio);
                        break;
                    default:
                        break;
                }
                xUzly.Add(0);

                w.Add(wUzly[0]);
                phi.Add(phiUzly[0]);
                m.Add(mUzly[0]);
                t.Add(tUzly[0]);
                x.Add(xUzly[0]);

                foreach (Hridel.Prvek p in prvky)
                {
                    Vector<double> v = Vector<double>.Build.DenseOfArray(new[] { wUzly.Last(), phiUzly.Last(), mUzly.Last(), tUzly.Last() }); // vektor levého konce prvku

                    if (p.L > 0)
                    {
                        double lPuvodni = p.L; // uložím si původní délku prvku, abych nemusel vytvářet prvek nový. L budu měnit, na konci mu původní délku vrátím.
                        double xRel = dx;

                        while (xRel < lPuvodni)
                        {
                            p.L = xRel;
                            Vector<double> vX = p.Matice.Multiply(v);
                            w.Add(vX[0]);
                            phi.Add(vX[1]);
                            m.Add(vX[2]);
                            t.Add(vX[3]);
                            x.Add(x.Last() + dx);
                            xRel = xRel + dx;
                        }
                        p.L = lPuvodni;
                    }

                    // tvar na konci prvku (v uzlu)
                    Vector<double> vNext = p.Matice.Multiply(v);
                    wUzly.Add(vNext[0]);
                    phiUzly.Add(vNext[1]);
                    mUzly.Add(vNext[2]);
                    tUzly.Add(vNext[3]);
                    xUzly.Add(xUzly.Last() + p.L);

                    w.Add(vNext[0]);
                    phi.Add(vNext[1]);
                    m.Add(vNext[2]);
                    t.Add(vNext[3]);
                    x.Add(xUzly.Last());
                }

                tvary[i].wUzly = wUzly.ToArray();
                tvary[i].phiUzly = phiUzly.ToArray();
                tvary[i].mUzly = mUzly.ToArray();
                tvary[i].tUzly = tUzly.ToArray();
                tvary[i].xUzly = xUzly.ToArray();

                tvary[i].w = w.ToArray();
                tvary[i].phi = phi.ToArray();
                tvary[i].m = m.ToArray();
                tvary[i].t = t.ToArray();
                tvary[i].x = x.ToArray();
            }
            return tvary;
        }



    }
}
