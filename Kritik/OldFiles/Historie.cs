using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public static class Historie
    {
        private static List<ObservableCollection<Hridel.PrvekTab>> h = new();
        private static int hIndex;
        private const int delkaHistorie = 33;
        public static bool BackBtnEnabled { get { return hIndex == 0 ? false : true; } private set { } }
        public static bool ForwardBtnEnabled { get { return hIndex == (h.Count() - 1) ? false : true; } private set { } }
        public static void New()
        {
            h.Clear();
            h.Add(new ObservableCollection<Hridel.PrvekTab>(KopieHridele()));
            hIndex = 0;
        }
        public static void Add()
        {
            if (hIndex < (h.Count - 1)) { h.RemoveRange(hIndex + 1, h.Count - hIndex - 1); }
            if (h.Count == delkaHistorie) { h.RemoveAt(0); }

            h.Add(new ObservableCollection<Hridel.PrvekTab>(KopieHridele()));
            hIndex = h.Count() - 1;
        }
        public static void Back()
        {
            if (hIndex > 0)
            {
                hIndex--;
                MainWindow.hridel.PrvkyHrideleTab = new ObservableCollection<Hridel.PrvekTab>(KopieHridele(h[hIndex]));
            }

        }
        public static void Forward()
        {
            if (hIndex < (h.Count - 1))
            {
                hIndex++;
                MainWindow.hridel.PrvkyHrideleTab = new ObservableCollection<Hridel.PrvekTab>(KopieHridele(h[hIndex]));
            }
        }
        private static ObservableCollection<Hridel.PrvekTab> KopieHridele(ObservableCollection<Hridel.PrvekTab> p = default)
        {
            if (p == null) { p = MainWindow.hridel.PrvkyHrideleTab; }
            ObservableCollection<Hridel.PrvekTab> pH = new();
            for (int i = 0; i < p.Count(); i++)
            {
                pH.Add(new Hridel.PrvekTab
                {
                    Typ = p[i].Typ,
                    L = p[i].L,
                    De = p[i].De,
                    Di = p[i].Di,
                    M = p[i].M,
                    Io = p[i].Io,
                    Id = p[i].Id,
                    K = p[i].K,
                    Cm = p[i].Cm,
                    Deleni = p[i].Deleni,
                    IdN = p[i].IdN,
                    IdNValue = p[i].IdNValue
                });
            }
            return pH;
        }
    }
}
