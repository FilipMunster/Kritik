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
            {"kritickeOtacky", "kritické otáčky" },
            {"PruhybHridele" , "Průhyb hřídele"},
            {"NatoceniHridele", "Natočení hřídele" },
            {"OhybovyMoment", "Ohybový moment" },
            {"PosouvajiciSila", "Posouvající síla" },
            {"VlivGyrosNeniUvazovan", "Vliv gyroskopických účínků není uvažován." },
            {"SoubeznaPrecese", "Souběžná precese" },
            {"ProtibeznaPrecese", "Protiběžná precese" },
            {"KritickeOtackykrouzivehoKmitani", "KRITICKÉ OTÁČKY KROUŽIVÉHO KMITÁNÍ" },
            {"NazevDT", "Název:" },
            {"PopisDT", "Popis:" },
            {"ResilDT", "Řešil:" },
            {"DatumDT", "Datum:" },
            {"OkrajovePodminkyDT", "Okrajové podmínky:" },
            {"LEVYKonecRotoru", "LEVÝ konec rotoru:" },
            {"PRAVYKonecRotoru", "PRAVÝ konec rotoru:" },
            {"VOLNY", "VOLNÝ" },
            {"KLOUB", "KLOUB" },
            {"VETKNUTI", "VETKNUTÍ" },
            {"ModulPruznostiVTahuHrideleDT", "Modul pružnosti v tahu hřídele:" },
            {"HustotaMaterialuHrideleDT", "Hustota materiálu hřídele:" },
            {"VLIVGYROSNENIUVAZOVAN", "VLIV GYROSKOPICKÝCH ÚČINKŮ NENÍ UVAŽOVÁN" },
            {"SOUBEZNAPRECESE", "SOUBĚŽNÁ PRECESE" },
            {"PROTIBEZNAPRECESE", "PROTIBĚŽNÁ PRECESE" },
            {"ProvozniOtackyHrideleDT", "Provozní otáčky hřídele:" },
            {"PrubezneOtackyHrideleDT", "Průběžné otáčky hřídele:" },
            {"PoznamkyKVypoctuDT", "Poznámky k výpočtu:" },
            {"VypocteneHodnotyDT", "Vypočtené hodnoty:" },
            {"odpovidajiDT", "odpovídají:" },
            {"provoznichOtacek", "provozních otáček" },
            {"prubeznychOtacek", "průběžných otáček" },
            {"GeometrieHrideleDT", "Geometrie hřídele:" },
            {Kritik.Hridel.beamKeyword, Kritik.Hridel.TypDict[Kritik.Hridel.beamKeyword] },
            {Kritik.Hridel.beamPlusKeyword, Kritik.Hridel.TypDict[Kritik.Hridel.beamPlusKeyword] },
            {Kritik.Hridel.rigidKeyword, Kritik.Hridel.TypDict[Kritik.Hridel.rigidKeyword] },
            {Kritik.Hridel.diskKeyword, Kritik.Hridel.TypDict[Kritik.Hridel.diskKeyword] },
            {Kritik.Hridel.springKeyword, Kritik.Hridel.TypDict[Kritik.Hridel.springKeyword] },
            {Kritik.Hridel.magnetKeyword, Kritik.Hridel.TypDict[Kritik.Hridel.magnetKeyword] },
            {"HridelJeRozdelenaNa", "Hřídel je rozdělena na" },
            {"castiODelce", "částí o délce" },
            {"meziKterymiJsouUmistenyPrvkyDT", "mezi kterými jsou umístěny prvky:" },
            {"NebylyVypoctenyZadneKritickeOtacky", "Nebyly vypočteny žádné kritické otáčky." }
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
            {"ProtibeznaPrecese", "Backward precession" },
            {"KritickeOtackykrouzivehoKmitani", "CRITICAL SPEED OF CIRCULAR OSCILLATION" },
            {"NazevDT", "Title:" },
            {"PopisDT", "Description:" },
            {"ResilDT", "Solved by:" },
            {"DatumDT", "Date:" },
            {"OkrajovePodminkyDT", "Boundary conditions:" },
            {"LEVYKonecRotoru", "LEFT end of rotor:" },
            {"PRAVYKonecRotoru", "RIGHT end of rotor:" },
            {"VOLNY", "FREE" },
            {"KLOUB", "JOINT" },
            {"VETKNUTI", "FIXED" },
            {"ModulPruznostiVTahuHrideleDT", "Young's modulus of the shaft:" },
            {"HustotaMaterialuHrideleDT", "Shaft material density:" },
            {"VLIVGYROSNENIUVAZOVAN", "THE INFLUENCE OF GYROSCOPIC EFFECTS IS NOT CONSIDERED" },
            {"SOUBEZNAPRECESE", "FORWARD PRECESSION" },
            {"PROTIBEZNAPRECESE", "BACKWARD PRECESSION" },
            {"ProvozniOtackyHrideleDT", "Operating shaft speed:" },
            {"PrubezneOtackyHrideleDT", "Runaway shaft speed:" },
            {"PoznamkyKVypoctuDT", "Notes:" },
            {"VypocteneHodnotyDT", "Calculated values:" },
            {"odpovidajiDT", "corresponds to:" },
            {"provoznichOtacek", "of operating speed" },
            {"prubeznychOtacek", "of runaway speed" },
            {"GeometrieHrideleDT", "Shaft geometry:" },
            {Kritik.Hridel.beamKeyword, "Beam" },
            {Kritik.Hridel.beamPlusKeyword, "Beam+" },
            {Kritik.Hridel.rigidKeyword, "Rigid" },
            {Kritik.Hridel.diskKeyword, "Disc" },
            {Kritik.Hridel.springKeyword, "Support" },
            {Kritik.Hridel.magnetKeyword, "Magnetic thrust" },
            {"HridelJeRozdelenaNa", "The shaft is divided into" },
            {"castiODelce", "parts with length of" },
            {"meziKterymiJsouUmistenyPrvkyDT", "between which are placed these elements:" },
            {"NebylyVypoctenyZadneKritickeOtacky", "No critical speed was computed." }
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
        public static string Typ(string typKeyword) { return dict[typKeyword]; }
        public static string HridelJeRozdelenaNa => dict[This()];
        public static string castiODelce => dict[This()];
        public static string meziKterymiJsouUmistenyPrvkyDT => dict[This()];
        public static string NebylyVypoctenyZadneKritickeOtacky => dict[This()];
    }
}
