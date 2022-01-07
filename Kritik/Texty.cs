using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    internal static class Texty
    {
        public enum Jazyky
        {
            cs,
            en
        }
        public static Dictionary<string, string> jazykNazev = new Dictionary<string, string>
        {
            {"cs", "čeština" },
            {"en", "angličtina" }
        };
        public static Jazyky Jazyk { get { return jazyk; } 
            set 
            { 
                jazyk = value; 
                switch (value)
                {
                    default:
                    case Jazyky.cs:
                        dict = cs;
                        break;
                    case Jazyky.en:
                        dict = en;
                        break;
                }
            } }
        private static Jazyky jazyk;
        private static Dictionary<string, string> dict = new();
        private static string This ([CallerMemberName] string memberName = "") { return memberName; }

        private static Dictionary<string, string> cs = new Dictionary<string, string>
        {
            {"kritickeOtacky", "kritcké otáčky" },
            {"PruhybHridele" , "Průhyb hřídele"},
            {"NatoceniHridele", "Natočení hřídele" },
            {"OhybovyMoment", "Ohybový moment" },
            {"PosouvajiciSila", "Posouvající síla" },
            {"VlivGyrosNeniUvazovan", "Vliv gyroskopických účínků není uvažován." },
            {"SoubeznaPrecese", "Souběžná precese" },
            {"ProtibeznaPrecese", "Protiběžná precese" }
        };

        private static Dictionary<string, string> en = new Dictionary<string, string>
        {
            {"kritickeOtacky", "critical speed" },
            {"PruhybHridele" , "Shaft deflection"},
            {"NatoceniHridele", "Shaft rotation" },
            {"OhybovyMoment", "Bending moment" },
            {"PosouvajiciSila", "Shear force" },
            {"VlivGyrosNeniUvazovan", "The influence of gyroscopic effects is not considered." },
            {"SoubeznaPrecese", "Forward precession" },
            {"ProtibeznaPrecese", "Backward precession" }
        };

        /// <summary>
        /// Vrací číslovku jako řadovou, např 1st, 1., 2nd, ...
        /// </summary>
        /// <param name="cislo"></param>
        /// <returns></returns>
        public static string RadovaCislovka(int cislo)
        {
            if (Jazyk == Jazyky.en)
            {
                // předpokládám, že čísla >20 se neobjeví, takže je neřeším
                switch (cislo)
                {
                    case 1: return cislo + "st";
                    case 2: return cislo + "nd";
                    case 3: return cislo + "rd";
                    default: return cislo + "th";
                }
            }
            else { return cislo + "."; }
        }
        public static string kritickeOtacky => dict[This()];
        public static string PruhybHridele => dict[This()];
        public static string NatoceniHridele => dict[This()];
        public static string OhybovyMoment => dict[This()];
        public static string PosouvajiciSila => dict[This()];
        public static string VlivGyrosNeniUvazovan => dict[This()];
        public static string SoubeznaPrecese => dict[This()];
        public static string ProtibeznaPrecese => dict[This()];
    }
}
