using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    /// <summary>
    /// Obsahuje vlastnosti s texty
    /// </summary>
    static class Texts
    {
        public enum Languages
        {
            cs,
            en
        }
        public readonly static Dictionary<Languages, string> LanguageName = new Dictionary<Languages, string>
        {
            {Languages.cs, "čeština" },
            {Languages.en, "angličtina" }
        };
        public static Languages SelectedLanguage { get { return language; } 
            set 
            { 
                language = value; 
                switch (value)
                {
                    default:
                    case Languages.cs:
                        dict = cs;
                        break;
                    case Languages.en:
                        dict = en;
                        break;
                }
            } }
        private static Languages language;
        private static Dictionary<string, string> dict = cs;
        private static string This ([CallerMemberName] string memberName = "") { return memberName; }

        private static Dictionary<string, string> cs = new Dictionary<string, string>
        {
            {nameof(kritickeOtacky), "kritické otáčky" },
            {nameof(PruhybHridele), "Průhyb hřídele"},
            {nameof(NatoceniHridele), "Natočení hřídele" },
            {nameof(OhybovyMoment), "Ohybový moment" },
            {nameof(PosouvajiciSila), "Posouvající síla" },
            {nameof(VlivGyrosNeniUvazovan), "Vliv gyroskopických účínků není uvažován." },
            {nameof(SoubeznaPrecese), "Souběžná precese" },
            {nameof(ProtibeznaPrecese), "Protiběžná precese" },
            {nameof(KritickeOtackykrouzivehoKmitani), "KRITICKÉ OTÁČKY KROUŽIVÉHO KMITÁNÍ" },
            {nameof(NazevDT), "Název:" },
            {nameof(PopisDT), "Popis:" },
            {nameof(ResilDT), "Řešil:" },
            {nameof(DatumDT), "Datum:" },
            {nameof(OkrajovePodminkyDT), "Okrajové podmínky:" },
            {nameof(LEVYKonecRotoru), "LEVÝ konec rotoru:" },
            {nameof(PRAVYKonecRotoru), "PRAVÝ konec rotoru:" },
            {nameof(VOLNY), "VOLNÝ" },
            {nameof(KLOUB), "KLOUB" },
            {nameof(VETKNUTI), "VETKNUTÍ" },
            {nameof(ModulPruznostiVTahuHrideleDT), "Modul pružnosti v tahu hřídele:" },
            {nameof(HustotaMaterialuHrideleDT), "Hustota materiálu hřídele:" },
            {nameof(VLIVGYROSNENIUVAZOVAN), "VLIV GYROSKOPICKÝCH ÚČINKŮ NENÍ UVAŽOVÁN" },
            {nameof(SOUBEZNAPRECESE), "SOUBĚŽNÁ PRECESE" },
            {nameof(PROTIBEZNAPRECESE), "PROTIBĚŽNÁ PRECESE" },
            {nameof(ProvozniOtackyHrideleDT), "Provozní otáčky hřídele:" },
            {nameof(PrubezneOtackyHrideleDT), "Průběžné otáčky hřídele:" },
            {nameof(PoznamkyKVypoctuDT), "Poznámky k výpočtu:" },
            {nameof(VypocteneHodnotyDT), "Vypočtené hodnoty:" },
            {nameof(odpovidajiDT), "odpovídají:" },
            {nameof(provoznichOtacek), "provozních otáček" },
            {nameof(prubeznychOtacek), "průběžných otáček" },
            {nameof(GeometrieHrideleDT), "Geometrie hřídele:" },
            {ElementType.beam.ToString(), "Hřídel" },
            {ElementType.beamPlus.ToString(), "Hřídel+" },
            {ElementType.rigid.ToString(), "Tuhý" },
            {ElementType.disc.ToString(), "Disk" },
            {ElementType.support.ToString(), "Podpora" },
            {ElementType.magnet.ToString(), "Magnet. tah" },
            {nameof(HridelJeRozdelenaNa), "Hřídel je rozdělena na" },
            {nameof(castiODelce), "částí o délce" },
            {nameof(meziKterymiJsouUmistenyPrvkyDT), "mezi kterými jsou umístěny prvky:" },
            {nameof(NebylyVypoctenyZadneKritickeOtacky), "Nebyly vypočteny žádné kritické otáčky." }
        };

        private static Dictionary<string, string> en = new Dictionary<string, string>
        {
            {nameof(kritickeOtacky), "critical speed" },
            {nameof(PruhybHridele), "Shaft deflection"},
            {nameof(NatoceniHridele), "Shaft rotation" },
            {nameof(OhybovyMoment), "Bending moment" },
            {nameof(PosouvajiciSila), "Shear force" },
            {nameof(VlivGyrosNeniUvazovan), "The influence of gyroscopic effects is not considered." },
            {nameof(SoubeznaPrecese), "Forward precession" },
            {nameof(ProtibeznaPrecese), "Backward precession" },
            {nameof(KritickeOtackykrouzivehoKmitani), "CRITICAL SPEED OF CIRCULAR OSCILLATION" },
            {nameof(NazevDT), "Title:" },
            {nameof(PopisDT), "Description:" },
            {nameof(ResilDT), "Solved by:" },
            {nameof(DatumDT), "Date:" },
            {nameof(OkrajovePodminkyDT), "Boundary conditions:" },
            {nameof(LEVYKonecRotoru), "LEFT end of rotor:" },
            {nameof(PRAVYKonecRotoru), "RIGHT end of rotor:" },
            {nameof(VOLNY), "FREE" },
            {nameof(KLOUB), "JOINT" },
            {nameof(VETKNUTI), "FIXED" },
            {nameof(ModulPruznostiVTahuHrideleDT), "Young's modulus of the shaft:" },
            {nameof(HustotaMaterialuHrideleDT), "Shaft material density:" },
            {nameof(VLIVGYROSNENIUVAZOVAN), "THE INFLUENCE OF GYROSCOPIC EFFECTS IS NOT CONSIDERED" },
            {nameof(SOUBEZNAPRECESE), "FORWARD PRECESSION" },
            {nameof(PROTIBEZNAPRECESE), "BACKWARD PRECESSION" },
            {nameof(ProvozniOtackyHrideleDT), "Operating shaft speed:" },
            {nameof(PrubezneOtackyHrideleDT), "Runaway shaft speed:" },
            {nameof(PoznamkyKVypoctuDT), "Notes:" },
            {nameof(VypocteneHodnotyDT), "Calculated values:" },
            {nameof(odpovidajiDT), "corresponds to:" },
            {nameof(provoznichOtacek), "of operating speed" },
            {nameof(prubeznychOtacek), "of runaway speed" },
            {nameof(GeometrieHrideleDT), "Shaft geometry:" },
            {ElementType.beam.ToString(), "Beam" },
            {ElementType.beamPlus.ToString(), "Beam+" },
            {ElementType.rigid.ToString(), "Rigid" },
            {ElementType.disc.ToString(), "Disc" },
            {ElementType.support.ToString(),"Support" },
            {ElementType.magnet.ToString(), "Magnetic thrust" },
            {nameof(HridelJeRozdelenaNa), "The shaft is divided into" },
            {nameof(castiODelce), "parts with length of" },
            {nameof(meziKterymiJsouUmistenyPrvkyDT), "between which are placed these elements:" },
            {nameof(NebylyVypoctenyZadneKritickeOtacky), "No critical speed was computed." }
        };

        /// <summary>
        /// Vrací číslovku jako řadovou, např 1st, 1., 2nd, ...
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string OrdinalNumber(int number)
        {
            if (SelectedLanguage == Languages.en)
            {
                // předpokládám, že čísla >20 se neobjeví, takže je neřeším
                switch (number)
                {
                    case 1: return number + "st";
                    case 2: return number + "nd";
                    case 3: return number + "rd";
                    default: return number + "th";
                }
            }
            else { return number + "."; }
        }
        public static string kritickeOtacky => dict[This()];
        public static string PruhybHridele => dict[This()];
        public static string NatoceniHridele => dict[This()];
        public static string OhybovyMoment => dict[This()];
        public static string PosouvajiciSila => dict[This()];
        public static string VlivGyrosNeniUvazovan => dict[This()];
        public static string SoubeznaPrecese => dict[This()];
        public static string ProtibeznaPrecese => dict[This()];
        public static string KritickeOtackykrouzivehoKmitani => dict[This()];
        public static string NazevDT => dict[This()];
        public static string PopisDT => dict[This()];
        public static string ResilDT => dict[This()];
        public static string DatumDT => dict[This()];
        public static string OkrajovePodminkyDT => dict[This()];
        public static string LEVYKonecRotoru => dict[This()];
        public static string PRAVYKonecRotoru => dict[This()];
        public static string VOLNY => dict[This()];
        public static string KLOUB => dict[This()];
        public static string VETKNUTI => dict[This()];
        public static string ModulPruznostiVTahuHrideleDT => dict[This()];
        public static string HustotaMaterialuHrideleDT => dict[This()];
        public static string VLIVGYROSNENIUVAZOVAN => dict[This()];
        public static string SOUBEZNAPRECESE => dict[This()];
        public static string PROTIBEZNAPRECESE => dict[This()];
        public static string ProvozniOtackyHrideleDT => dict[This()];
        public static string PrubezneOtackyHrideleDT => dict[This()];
        public static string PoznamkyKVypoctuDT => dict[This()];
        public static string VypocteneHodnotyDT => dict[This()];
        public static string odpovidajiDT => dict[This()];
        public static string provoznichOtacek => dict[This()];
        public static string prubeznychOtacek => dict[This()];
        public static string GeometrieHrideleDT => dict[This()];
        public static string Type(ElementType type) { return dict[type.ToString()]; }
        public static string HridelJeRozdelenaNa => dict[This()];
        public static string castiODelce => dict[This()];
        public static string meziKterymiJsouUmistenyPrvkyDT => dict[This()];
        public static string NebylyVypoctenyZadneKritickeOtacky => dict[This()];
    }
}
