using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    /// <summary>
    /// Contains properties with strings according to <see cref="SelectedLanguage"/>
    /// </summary>
    public class Strings
    {
        private Dictionary<string, string> dictionary;
        public Strings(Language language)
        {
            SelectedLanguage = language;
        }

        public enum Language
        {
            [Description("čeština")]
            cs,
            [Description("angličtina")]
            en
        }
        
        public string[] LanguageNames => Enums.GetNames<Language>();
        private Language selectedLanguage;
        public Language SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                selectedLanguage = value; 
                Properties.Settings.Default.lang = (int)value;
                switch (value)
                {
                    case Language.cs:
                        dictionary = czech;
                        break;
                    case Language.en:
                        dictionary = english;
                        break;
                }
            }
        }

        #region Auxiliary methods
        /// <summary>
        /// Returns CallerMamberName
        /// </summary>
        /// <returns></returns>
        private string This([CallerMemberName] string memberName = "")
        {
            return memberName; 
        }
        #endregion

        #region Dictionary for Czech
        private static Dictionary<string, string> czech = new Dictionary<string, string>
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
            {nameof(PROZADANEOTACKY), "PRO ZADANÉ OTÁČKY" },
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
        #endregion

        #region Dictionary for English
        private static Dictionary<string, string> english = new Dictionary<string, string>
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
            {nameof(PROZADANEOTACKY), "FOR THE SPECIFIED SPEED OF" },
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
        #endregion

        #region Properties and methods returning strings according to language set in SelectedLanguage
        public string kritickeOtacky => dictionary[This()];
        public string PruhybHridele => dictionary[This()];
        public string NatoceniHridele => dictionary[This()];
        public string OhybovyMoment => dictionary[This()];
        public string PosouvajiciSila => dictionary[This()];
        public string VlivGyrosNeniUvazovan => dictionary[This()];
        public string SoubeznaPrecese => dictionary[This()];
        public string ProtibeznaPrecese => dictionary[This()];
        public string KritickeOtackykrouzivehoKmitani => dictionary[This()];
        public string NazevDT => dictionary[This()];
        public string PopisDT => dictionary[This()];
        public string ResilDT => dictionary[This()];
        public string DatumDT => dictionary[This()];
        public string OkrajovePodminkyDT => dictionary[This()];
        public string LEVYKonecRotoru => dictionary[This()];
        public string PRAVYKonecRotoru => dictionary[This()];
        public string VOLNY => dictionary[This()];
        public string KLOUB => dictionary[This()];
        public string VETKNUTI => dictionary[This()];
        public string ModulPruznostiVTahuHrideleDT => dictionary[This()];
        public string HustotaMaterialuHrideleDT => dictionary[This()];
        public string VLIVGYROSNENIUVAZOVAN => dictionary[This()];
        public string SOUBEZNAPRECESE => dictionary[This()];
        public string PROTIBEZNAPRECESE => dictionary[This()];
        public string PROZADANEOTACKY => dictionary[This()];
        public string ProvozniOtackyHrideleDT => dictionary[This()];
        public string PrubezneOtackyHrideleDT => dictionary[This()];
        public string PoznamkyKVypoctuDT => dictionary[This()];
        public string VypocteneHodnotyDT => dictionary[This()];
        public string odpovidajiDT => dictionary[This()];
        public string provoznichOtacek => dictionary[This()];
        public string prubeznychOtacek => dictionary[This()];
        public string GeometrieHrideleDT => dictionary[This()];
        public string HridelJeRozdelenaNa => dictionary[This()];
        public string castiODelce => dictionary[This()];
        public string meziKterymiJsouUmistenyPrvkyDT => dictionary[This()];
        public string NebylyVypoctenyZadneKritickeOtacky => dictionary[This()];
        public string VypocetNebylDosudProveden => dictionary[This()];

        /// <summary>
        /// Converts <see cref="ElementType"/> enum to string according to language set in <see cref="SelectedLanguage"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string Type(ElementType type) { return dictionary[type.ToString()]; }

        /// <summary>
        /// Returns string with ordinal number of given int according to language set in <see cref="SelectedLanguage"/>
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string OrdinalNumber(int number)
        {
            int num = Math.Abs(number) % 100;

            switch (selectedLanguage)
            {
                case Language.en:
                    {
                        if (num > 10 && num < 20)
                            return number + "th";

                        switch (num % 10)
                        {
                            case 1: return number + "st";
                            case 2: return number + "nd";
                            case 3: return number + "rd";
                            default: return number + "th";
                        }
                    }
                case Language.cs:
                default:
                    {
                        return number + ".";
                    }
            }
        }
        #endregion
    }
}
