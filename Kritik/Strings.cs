using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    /// <summary>
    /// Contains properties with strings according to selected 
    /// </summary>
    public class Strings
    {
        public Strings(Language language)
        {
            SelectedLanguageEnum = language;

            List<string> languageNames = new List<string>();
            foreach (var lang in LanguageNameEnumToString)
            {
                languageNames.Add(lang.Value);
            }
            LanguageNames = languageNames.ToArray();
        }
        public enum Language
        {
            cs,
            en
        }
        private Language SelectedLanguageEnum
        {
            get { return selectedLanguage; }
            set
            {
                selectedLanguage = value;
                switch (value)
                {
                    default:
                    case Language.cs:
                        dict = cs;
                        break;
                    case Language.en:
                        dict = en;
                        break;
                }
            }
        }
        public string SelectedLanguage
        {
            get => LanguageNameEnumToString[selectedLanguage];
            set { selectedLanguage = LanguageNameStringToEnum[value]; }
        }
        private Language selectedLanguage;
        public string[] LanguageNames { get; }

        private readonly Dictionary<Language, string> LanguageNameEnumToString = new Dictionary<Language, string>
        {
            {Language.cs, "čeština" },
            {Language.en, "angličtina" }
        };
        private readonly Dictionary<string, Language> LanguageNameStringToEnum = new Dictionary<string, Language>
        {
            {"čeština", Language.cs },
            {"angličtina", Language.en }
        };

        private Dictionary<string, string> dict = cs;
        private string This ([CallerMemberName] string memberName = "") { return memberName; }

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
            {nameof(NebylyVypoctenyZadneKritickeOtacky), "Nebyly vypočteny žádné kritické otáčky." },
            {nameof(VypocetNebylDosudProveden), "Výpočet nebyl dosud proveden." }
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
            {nameof(NebylyVypoctenyZadneKritickeOtacky), "No critical speed was computed." },
            {nameof(VypocetNebylDosudProveden), "The calculation has not been done yet." }
        };

        /// <summary>
        /// Returns string with ordinal number of given int
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string OrdinalNumber(int number)
        {
            if (SelectedLanguageEnum == Language.en)
            {
                // Ordinal numbers are used only for number of critical speed. I assume that >20 will not appear.
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
        public string kritickeOtacky => dict[This()];
        public string PruhybHridele => dict[This()];
        public string NatoceniHridele => dict[This()];
        public string OhybovyMoment => dict[This()];
        public string PosouvajiciSila => dict[This()];
        public string VlivGyrosNeniUvazovan => dict[This()];
        public string SoubeznaPrecese => dict[This()];
        public string ProtibeznaPrecese => dict[This()];
        public string KritickeOtackykrouzivehoKmitani => dict[This()];
        public string NazevDT => dict[This()];
        public string PopisDT => dict[This()];
        public string ResilDT => dict[This()];
        public string DatumDT => dict[This()];
        public string OkrajovePodminkyDT => dict[This()];
        public string LEVYKonecRotoru => dict[This()];
        public string PRAVYKonecRotoru => dict[This()];
        public string VOLNY => dict[This()];
        public string KLOUB => dict[This()];
        public string VETKNUTI => dict[This()];
        public string ModulPruznostiVTahuHrideleDT => dict[This()];
        public string HustotaMaterialuHrideleDT => dict[This()];
        public string VLIVGYROSNENIUVAZOVAN => dict[This()];
        public string SOUBEZNAPRECESE => dict[This()];
        public string PROTIBEZNAPRECESE => dict[This()];
        public string ProvozniOtackyHrideleDT => dict[This()];
        public string PrubezneOtackyHrideleDT => dict[This()];
        public string PoznamkyKVypoctuDT => dict[This()];
        public string VypocteneHodnotyDT => dict[This()];
        public string odpovidajiDT => dict[This()];
        public string provoznichOtacek => dict[This()];
        public string prubeznychOtacek => dict[This()];
        public string GeometrieHrideleDT => dict[This()];
        public string Type(ElementType type) { return dict[type.ToString()]; }
        public string HridelJeRozdelenaNa => dict[This()];
        public string castiODelce => dict[This()];
        public string meziKterymiJsouUmistenyPrvkyDT => dict[This()];
        public string NebylyVypoctenyZadneKritickeOtacky => dict[This()];
        public string VypocetNebylDosudProveden => dict[This()];
    }
}
