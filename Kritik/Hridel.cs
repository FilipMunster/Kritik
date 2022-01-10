using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OfficeOpenXml;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using OxyPlot;
using OxyPlot.Series;
using System.Windows;

namespace Kritik
{
    /// <summary>
    /// 
    /// </summary>
    public class Hridel : INotifyPropertyChanged
    {
        // NotifyPropertyChanged je nutné zavolat v setteru Vlastnosti, aby se hodnota updatovala v okně
        // viz https://docs.microsoft.com/cs-cz/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-5.0
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            AnyPropertyChanged = true;
        }

        public bool AnyPropertyChanged { get { return anyPropertyChanged; } set { 
                anyPropertyChanged = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextTitulkyOkna"));
            } }
        private bool anyPropertyChanged;
        public string nazevSouboru;
        public string TextTitulkyOkna {
            get {
                string verze = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                verze = verze.Substring(0, verze.Length - 2);
                string kritVer = "Kritik " + verze;
                if (nazevSouboru != null)
                {
                    string title = kritVer + " - [" + nazevSouboru;
                    if (anyPropertyChanged) { title += " *"; }
                    title += "]";
                    return title;
                }
                else { return kritVer; }
            } }

        // klíčová slova ve vstupním souboru
        public readonly string[] nazvySloupcu = { "Typ", "L", "De", "Di", "m", "Io", "Id", "k", "Cm", "Deleni", "Id/N", "Id/Nvalue" };
        public const string vypocetNazevKeyword = "Nazev";
        public const string vypocetPopisKeyword = "Popis";
        public const string vypocetResilKeyword = "Resil";
        public const string vypocetDatumKeyword = "Datum";
        public const string opLevaKeyword = "OP leva";
        public const string opPravaKeyword = "OP prava";
        public const string opVolnyKeyword = "volny";
        public const string opVetknutiKeyword = "vetknuti";
        public const string opKloubKeyword = "kloub";
        public const string gyrosKeyword = "GU";
        public const string gyrosSoubeznaKeyword = "soubezna";
        public const string gyrosProtibeznaKeyword = "protibezna";
        public const string gyrosZanedbaniKeyword = "zanedbani";
        public const string modulPruznostiKeyword = "E";
        public const string rhoKeyword = "rho";
        public const string otackyProvozniKeyword = "n";
        public const string otackyPrubezneKeyword = "nr";
        public const string nKritMaxKeyword = "nKritMax";
        public const string poznamkaKeyword = "Poznamka";
        public const string diskKeyword = "disk";
        public const string rigidKeyword = "tuhy";
        public const string beamKeyword = "hridel";
        public const string beamPlusKeyword = "hridel+";
        public const string springKeyword = "pruzina";
        public const string magnetKeyword = "magnet";
        public const string jednotkaModuluPruznosti = "GPa";
        public const int radJednotkyModuluPruznosti = 9;
        public const string excelListZadani = "KRITIK_zadani";
        public const string excelListVysledky = "KRITIK_vypocet";
        public static Dictionary<string, string> TypDict = new Dictionary<string, string>
        {
            {beamKeyword, "Hřídel" },
            {beamPlusKeyword, "Hřídel+" },
            {rigidKeyword, "Tuhý" },
            {diskKeyword, "Disk" },
            {springKeyword, "Podpora" },
            {magnetKeyword, "Magnet. tah" },
            {"Hřídel", beamKeyword},
            {"Hřídel+", beamPlusKeyword},
            {"Tuhý", rigidKeyword},
            {"Disk", diskKeyword},
            {"Podpora", springKeyword},
            {"Magnet. tah", magnetKeyword}
        };
        public readonly List<string> ListTypuPrvku = new List<string>() {
            TypDict[beamKeyword],
            TypDict[beamPlusKeyword],
            TypDict[rigidKeyword],
            TypDict[diskKeyword],
            TypDict[springKeyword],
            TypDict[magnetKeyword]
        };

        // proměnné s parametry výpočtu
        private string vypocetNazev;
        public string VypocetNazev { get { return vypocetNazev; } set { vypocetNazev = value; NotifyPropertyChanged(); } }
        private string vypocetPopis;
        public string VypocetPopis { get { return vypocetPopis; } set { vypocetPopis = value; NotifyPropertyChanged(); } }
        private string vypocetResil;
        public string VypocetResil { get { return vypocetResil; } set { vypocetResil = value; NotifyPropertyChanged();
                if (MainWindow.novySoubor) { Properties.Settings.Default.resil = value; } } }
        private string vypocetDatum;
        public string VypocetDatum { get { return vypocetDatum; } set { vypocetDatum = value; NotifyPropertyChanged(); } }
        private string opLeva;
        public string OpLeva { get { return opLeva; } set { opLeva = value; NotifyPropertyChanged("OpLevaIndex"); } }
        private string opPrava;
        public string OpPrava { get { return opPrava; } set { opPrava = value; NotifyPropertyChanged("OpPravaIndex"); } }
        private string gyros;
        public string Gyros { get { return gyros; } set { gyros = value; NotifyPropertyChanged("GyrosIndex"); } }
        private double modulPruznosti;
        public double ModulPruznosti { get { return modulPruznosti; } set { modulPruznosti = value; NotifyPropertyChanged(); } }
        private double rho;
        public double Rho { get { return rho; } set { rho = value; NotifyPropertyChanged(); } }
        private double otackyProvozni;
        public double OtackyProvozni { get { return otackyProvozni; } set { otackyProvozni = value; NotifyPropertyChanged(); } }
        private double otackyPrubezne;
        public double OtackyPrubezne { get { return otackyPrubezne; } set { otackyPrubezne = value; NotifyPropertyChanged(); } }
        private double nKritMax;
        public double NKritMax { get { return nKritMax; } set { nKritMax = value; NotifyPropertyChanged(); } }
        private string poznamka;
        public string Poznamka { get { return poznamka; } set { poznamka = value; NotifyPropertyChanged(); } }
        public string HridelPlusDeleni {
            get {
                if (OznacenyRadek != null) {
                    return OznacenyRadek.Deleni.ToString();
                }
                else { return string.Empty; }
            }
            set {
                try { OznacenyRadek.Deleni = Convert.ToDouble(value); }
                catch { MessageBox.Show("Hodnota dělení nesmí být prázdná.", "Špatně zadaná hodnota", MessageBoxButton.OK, MessageBoxImage.Error);
                    MainWindow.AppWindow.deleniHridelPlusTextBox.Focus();
                    return;
                }
                if (OznacenyRadek.Deleni < 1)
                {
                    MessageBox.Show("Hodnota dělení nesmí být menší než 1.", "Špatně zadaná hodnota", MessageBoxButton.OK, MessageBoxImage.Error);
                    MainWindow.AppWindow.deleniHridelPlusTextBox.Focus();
                }
                Historie.Add(); NotifyPropertyChanged("SchemaHridele"); } 
        }

        /// <summary>
        /// Seznam indexů prvků typu Hřídel+
        /// </summary>
        public List<int> IndexyHrideliPlus
        {
            get
            {
                List<int> szn = new List<int>();
                for (int i = 0; i < PrvkyHrideleTab.Count(); i++)
                {
                    if (PrvkyHrideleTab[i].Typ == beamPlusKeyword)
                    {
                        szn.Add(i + 1);
                    }
                }
                return szn;
            }
        }

        /// <summary>
        /// Způsob výpočtu Id Hřídele+. 0 = vynásobení konstantou, 1 = vlastní hodnota
        /// </summary>
        public int HridelPlusZpusobSelectedIndex
        {
            get {
                if (OznacenyRadek != null && OznacenyRadek.Typ == beamPlusKeyword) return OznacenyRadek.IdN;
                else return -1;
            }
            set
            {
                if (OznacenyRadek != null) { OznacenyRadek.IdN = value; Historie.Add(); }
            }
        }
        /// <summary>
        /// Vybrané číslo prvku hřídel+ v comboboxu
        /// </summary>
        public int HridelPlusSelectedValue
        {
            get
            {
                if ((OznacenyRadek != null) && (OznacenyRadek.Typ == beamPlusKeyword))
                {
                    return PrvkyHrideleTab.IndexOf(OznacenyRadek) + 1;
                }
                else return -1;
            }
        }
        /// <summary>
        /// Zadaná / vypočtená hodnota Id u prvku Hridel+
        /// </summary>
        public string HridelPlusHodnota
        {
            get
            {
                if (OznacenyRadek != null) return OznacenyRadek.IdNValue.ToString();
                else return String.Empty;
            }
            set
            {
                if (OznacenyRadek != null) { OznacenyRadek.IdNValue = Convert.ToDouble(value); Historie.Add(); }
            }
        }
        public bool HridelPlusOvlPrvkyEnabled
        {
            get
            {
                if ((OznacenyRadek != null) && (OznacenyRadek.Typ == beamPlusKeyword)) { return true; } else { return false; }
            }
        }

        // Vlastnosti pro provázání ComboBoxů s Vlastnostmi hřídele
        public static Dictionary<int, string> opDict = new Dictionary<int, string> {
            {0,opVolnyKeyword},
            {1,opKloubKeyword},
            {2,opVetknutiKeyword}
        };
        public static Dictionary<int, string> gyrosDict = new Dictionary<int, string> {
            {0,gyrosZanedbaniKeyword},
            {1,gyrosSoubeznaKeyword},
            {2,gyrosProtibeznaKeyword}
        };
        public static Dictionary<string, int> opDictInv = new Dictionary<string, int> {
            {opVolnyKeyword,0},
            {opKloubKeyword,1},
            {opVetknutiKeyword,2}
        };
        public static Dictionary<string, int> gyrosDictInv = new Dictionary<string, int> {
            {gyrosZanedbaniKeyword,0},
            {gyrosSoubeznaKeyword,1},
            {gyrosProtibeznaKeyword,2}
        };
        public int OpLevaIndex {
            get { try { return opDictInv[OpLeva]; } catch { return -1; } }
            set { OpLeva = opDict[value]; }
        }
        public int OpPravaIndex
        {
            get { try { return opDictInv[OpPrava]; } catch { return -1; } }
            set { OpPrava = opDict[value]; }
        }
        public int GyrosIndex
        {
            get { try { return gyrosDictInv[Gyros]; } catch { return -1; } }
            set { Gyros = gyrosDict[value]; }
        }

        // další vlastnosti
        public PrvekTab OznacenyRadek {
            set {
                oznacenyRadek = value;
                NotifyPropertyChanged("HridelPlusDeleni");
                NotifyPropertyChanged("HridelPlusZpusobSelectedIndex");
                NotifyPropertyChanged("HridelPlusSelectedValue");
                NotifyPropertyChanged("HridelPlusHodnota");
                NotifyPropertyChanged("HridelPlusOvlPrvkyEnabled");
                NotifyPropertyChanged("SchemaHridele");

            } get { return oznacenyRadek; } }
        private PrvekTab oznacenyRadek;

        /// <summary>
        /// Vytvoření nové prázdné hřídele
        /// </summary>
        public void HridelNova()
        {
            VypocetNazev = string.Empty;
            VypocetPopis = string.Empty;
            VypocetResil = Properties.Settings.Default.resil == "" ? Environment.UserName : Properties.Settings.Default.resil;
            VypocetDatum = DateTime.Today.ToShortDateString();
            OpLeva = opVolnyKeyword;
            OpPrava = opVolnyKeyword;
            Gyros = gyrosZanedbaniKeyword;
            ModulPruznosti = 210;
            Rho = 7850;
            OtackyProvozni = 0;
            OtackyPrubezne = 0;
            NKritMax = 5000;
            Poznamka = string.Empty;
            PrvkyHrideleTab = new ObservableCollection<PrvekTab>();
            PrvkyHridele = null;
            KritOt = null;
            PrubehDeterminantu = null;
            PrubehRpm = null;
            AnyPropertyChanged = true;
            TvaryKmitu = null;
        }

        /// <summary>
        /// List Prvků hřídele
        /// </summary>
        public List<Prvek> PrvkyHridele { get { return prvkyHridele; } set { prvkyHridele = value; NotifyPropertyChanged(); } }
        private List<Prvek> prvkyHridele;

        /// <summary>
        /// List Prvků hřídele, zobrazené v DataGridu
        /// </summary>
        public ObservableCollection<PrvekTab> PrvkyHrideleTab { get { return prvkyHrideleTab; } set { prvkyHrideleTab = value; NotifyPropertyChanged(); } }
        private ObservableCollection<PrvekTab> prvkyHrideleTab;

        public PlotModel SchemaHridele => Plot.SchemaHridele(PrvkyHrideleTab, OznacenyRadek, MainWindow.AppWindow.schemaHridele.ActualWidth);

        //Vlastnosti s výsledky výpočtu
        /// <summary>
        /// Obsahuje pole hodnot kritických otáček
        /// </summary>
        public double[] KritOt { 
            get { return kritOt; }
            set { kritOt = value; NotifyPropertyChanged("KritOtText"); NotifyPropertyChanged("KritOtOdpovidajiText"); }
        }
        private double[] kritOt;
        /// <summary>
        /// Obsahuje průběh determinantu - pole detUc=f(rpmi)
        /// </summary>
        public double[] PrubehDeterminantu {
            get { return prubehDeterminantu; }
            set { prubehDeterminantu = value; }
        }
        private double[] prubehDeterminantu;
        /// <summary>
        /// Obsahuje pole rpmi - pole detUc=f(rpmi)
        /// </summary>
        public double[] PrubehRpm
        {
            get { return prubehRpm; }
            set { prubehRpm = value; }
        }
        private double[] prubehRpm;
        /// <summary>
        /// Graf determinantu
        /// </summary>
        public PlotModel PlotDeterminant { 
            get { return plotDeterminant; }
            set { plotDeterminant = value; NotifyPropertyChanged(); }
        }
        private PlotModel plotDeterminant;
        /// <summary>
        /// Pole s tvary kmitů
        /// </summary>
        public TvarKmitu[] TvaryKmitu
        {
            get { return tvaryKmitu; }
            set { tvaryKmitu = value; NotifyPropertyChanged(); }
        }
        private TvarKmitu[] tvaryKmitu;

        /// <summary>
        /// Text kritických otáček do TextBoxu
        /// </summary>
        public string KritOtText
        {
            get
            {
                if ((KritOt != null) && (KritOt.Length > 0) && (KritOt[0] == -1))
                {
                    return "Probíhá výpočet...";
                }
                else if ((KritOt != null) && (KritOt.Length>0))
                {
                    string kOText = "";
                    int i = 1;
                    foreach (double otacky in KritOt)
                    {
                        kOText += i + ". kritické otáčky: " + String.Format("{0:0.000}", otacky) + " rpm\n";
                        i++;
                    }
                    return kOText;
                }
                else { return String.Empty; }
            }
        }
        public string KritOtOdpovidajiText
        {
            get
            {
                if ((KritOt != null) && (KritOt.Length > 0) && (KritOt[0] != -1))
                {
                    string kOText = "";
                    if (OtackyProvozni > 0)
                    {
                        kOText += String.Format("{0:0.000}", KritOt[0] / OtackyProvozni * 100) + " % provozních otáček\n";
                    }
                    if (OtackyPrubezne > 0)
                    {
                        kOText += String.Format("{0:0.000}", KritOt[0] / OtackyPrubezne * 100) + " % průběžných otáček";
                    }
                    return kOText;
                }
                else { return String.Empty; }
            }
        }

        // Třídy
        /// <summary>
        /// Třída Prvků hřídele
        /// </summary>
        public class Prvek
        {
            private string typ;
            public string Typ { get { return typ; } set { typ = value; VytvorMatici(); } }
            private double l;
            public double L { 
                get { if (typ == diskKeyword || typ == springKeyword || typ == magnetKeyword) { return 0; } else { return l; } }
                set { l = value; VytvorMatici(); } }
            private double de;
            public double De { get { return de; } set { de = value; VytvorMatici(); } }
            private double di;
            public double Di { get { return di; } set { di = value; VytvorMatici(); } }
            private double m;
            public double M { get { return m; } set { m = value; VytvorMatici(); } }
            private double io;
            public double Io { get { return io; } set { io = value; VytvorMatici(); } }
            private double id;
            public double Id { get { return id; } set { id = value; VytvorMatici(); } }
            private double k;
            public double K { get { return k; } set { k = value; VytvorMatici(); } }
            private double cm;
            public double Cm { get { return cm; } set { cm = value; VytvorMatici(); } }
            private double rpm;
            public double Rpm { get { return rpm; } set { rpm = value; VytvorMatici(); } }
            private string gyros;
            public string Gyros { get { return gyros; } set { gyros = value; VytvorMatici(); } }
            private double rho;
            public double Rho { get { return rho; } set { rho = value; VytvorMatici(); } }
            private double e;
            public double E { get { return e; } set { e = value; VytvorMatici(); } }
            public Matrix<double> Matice { get; private set; }

            /// <summary>
            /// Prvek hřídele
            /// </summary>
            /// <param name="typ">Typ prvku</param>
            /// <param name="l">Délka prvku [m]</param>
            /// <param name="de">Vnější průměr hřídele [m]</param>
            /// <param name="di">Vrtání hřídele [m]</param>
            /// <param name="m">Hmotnost disku [kg]</param>
            /// <param name="io">Moment setrvačnosti k ose rotoru [kg.m2]</param>
            /// <param name="id">Moment setrvačnosti kolmý k ose rotoru [kg.m2]</param>
            /// <param name="k">Tuhost pružiny [N/m]</param>
            /// <param name="cm">Magneticko elastická konstanta [N/m]</param>
            /// <param name="rpm">Otáčky [1/min]</param>
            /// <param name="gyros">Gyroskopické účinky</param>
            /// <param name="rho">Měrná hmotnost materiálu [kg/m3]</param>
            /// <param name="e">Modul pružnosti materiálu [Pa]</param>
            private void VytvorMatici()
            {
                switch (typ)
                {
                    case beamKeyword:
                        {
                            // Prvek hřídele:
                            double S = Math.PI * (Math.Pow(De, 2) - Math.Pow(Di, 2)) / 4.0;
                            double J = Math.PI / 4.0 * (Math.Pow(De / 2.0, 4) - Math.Pow(Di / 2.0, 4));
                            double omega = 2 * Math.PI * Rpm / 60.0;

                            double G = Math.Pow(Rho * S * Math.Pow(omega, 2) / (E * J), 1 / 4.0);
                            double V1 = 1 / 2.0 * (Math.Cosh(G * L) + Math.Cos(G * L));
                            double V2 = 1 / 2.0 * (Math.Sinh(G * L) + Math.Sin(G * L));
                            double V3 = 1 / 2.0 * (Math.Cosh(G * L) - Math.Cos(G * L));
                            double V4 = 1 / 2.0 * (Math.Sinh(G * L) - Math.Sin(G * L));

                            double[,] matrix = {{                    V1,                  V2/G, -V3/(Math.Pow(G,2)*E*J), -V4/(Math.Pow(G,3)*E*J) },
                                                {                  G*V4,                    V1,             -V2/(G*E*J), -V3/(Math.Pow(G,2)*E*J) },
                                                { -Math.Pow(G,2)*E*J*V3,             -G*E*J*V4,                      V1,                    V2/G },
                                                { -Math.Pow(G,3)*E*J*V2, -Math.Pow(G,2)*E*J*V3,                    G*V4,                      V1 }};
                            Matice = Matrix<double>.Build.DenseOfArray(matrix);
                            break;
                        }
                    case rigidKeyword:
                        {
                            // Tuhý prvek:
                            double[,] matrix = {{ 1.0,   L, 0.0, 0.0 },
                                                { 0.0, 1.0, 0.0, 0.0 },
                                                { 0.0, 0.0, 1.0,   L },
                                                { 0.0, 0.0, 0.0, 1.0 }};
                            Matice = Matrix<double>.Build.DenseOfArray(matrix);
                            break;
                        }
                    case diskKeyword:
                        {
                            // Prvek disku:
                            double omega = 2 * Math.PI * Rpm / 60.0;
                            double p = 1.0;
                            double io = Io;
                            double id = Id;

                            switch (Gyros)
                            {
                                case gyrosSoubeznaKeyword:
                                    break;
                                case gyrosProtibeznaKeyword:
                                    p = -1.0;
                                    break;
                                case gyrosZanedbaniKeyword:
                                    io = 0;
                                    id = 0;
                                    break;
                                default:
                                    Debug.WriteLine("Špatně zadané gyroskopické účinky ({0})", Gyros);
                                    break;
                            }

                            double[,] matrix = {{                  1.0,                                         0.0, 0.0, 0.0 },
                                                {                  0.0,                                         1.0, 0.0, 0.0 },
                                                {                  0.0, id*Math.Pow(omega,2)-io*Math.Pow(omega,2)*p, 1.0, 0.0 },
                                                { -M*Math.Pow(omega,2),                                         0.0, 0.0, 1.0 }};
                            Matice = Matrix<double>.Build.DenseOfArray(matrix);
                            break;
                        }
                    case springKeyword:
                        {
                            // Pružná podpora:
                            double[,] matrix = {{ 1.0, 0.0, 0.0, 0.0 },
                                                { 0.0, 1.0, 0.0, 0.0 },
                                                { 0.0, 0.0, 1.0, 0.0 },
                                                {   K, 0.0, 0.0, 1.0 }};
                            Matice = Matrix<double>.Build.DenseOfArray(matrix);
                            break;
                        }
                    case magnetKeyword:
                        {
                            // Magnet:
                            double[,] matrix = {{ 1.0, 0.0, 0.0, 0.0 },
                                                { 0.0, 1.0, 0.0, 0.0 },
                                                { 0.0, 0.0, 1.0, 0.0 },
                                                { -Cm, 0.0, 0.0, 1.0 }};
                            Matice = Matrix<double>.Build.DenseOfArray(matrix);
                            break;
                        }
                    default:
                        //Debug.WriteLine("Špatně zadaný typ prvku ({0})", typ);
                        break;
                }
            }
        }    
        /// <summary>
        /// Třída prvků hřídele v tabulce v Datagridu
        /// </summary>
        public class PrvekTab : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                MainWindow.hridel.AnyPropertyChanged = true;
            }
            private void NotifyPropertyChangedVsechno()
            {
                NotifyPropertyChanged("IsEditableL");
                NotifyPropertyChanged("IsEditableDe");
                NotifyPropertyChanged("IsEditableDi");
                NotifyPropertyChanged("IsEditableM");
                NotifyPropertyChanged("IsEditableIo");
                NotifyPropertyChanged("IsEditableId");
                NotifyPropertyChanged("IsEditableK");
                NotifyPropertyChanged("IsEditableCm");
            }
            public string Typ { get { return typ; } set { typ = value; NotifyPropertyChangedVsechno(); } }
            private string typ;
            public double L { get { return typ == diskKeyword || typ == springKeyword || typ == magnetKeyword ? 0 : l; } set { l = value; } }
            private double l;
            public double De { get { return de; } set { de = value; } }
            private double de;
            public double Di { get { return di; } set { di = value; } }
            private double di;
            public double M { get { return m; } set { m = value; } }
            private double m;
            public double Io { get { return io; } set { io = value; } }
            private double io;
            public double Id { get { return id; } set { id = value; } }
            private double id;
            public double K { get { return k; } set { k = value; } }
            private double k;
            public double Cm { get { return cm; } set { cm = value; } }
            private double cm;
            public double Deleni { get { return deleni; } set { deleni = value; } }
            private double deleni;
            /// <summary>
            /// Způsob dělení Hřídele+: IdN=0 -> Idi=Id/n*IdNValue, IdN=0 -> Idi=IdNValue
            /// </summary>
            public int IdN { get; set; }
            public double IdNValue { get; set; }

            /// <summary>
            /// Text typu prvku tak, jak je zobrazen v tabulce
            /// </summary>
            public string TypZobrazeny { 
                get { if (Typ != null) { 
                        MainWindow.hridel.NotifyPropertyChanged("HridelPlusOvlPrvkyEnabled");
                        MainWindow.hridel.NotifyPropertyChanged("IndexyHrideliPlus");
                        MainWindow.hridel.NotifyPropertyChanged("HridelPlusSelectedValue");
                        return TypDict[Typ]; } else { return String.Empty; } } 
                set { Typ = TypDict[value]; } }
            /// <summary>
            /// Zda je editovatelná buňka na základě typu prvku
            /// </summary>
            public bool IsEditableL => Typ is beamKeyword or beamPlusKeyword or rigidKeyword;
            public bool IsEditableDe => Typ is beamKeyword or beamPlusKeyword;
            public bool IsEditableDi => Typ is beamKeyword or beamPlusKeyword;
            public bool IsEditableM => Typ is diskKeyword or beamPlusKeyword;
            public bool IsEditableIo => Typ is diskKeyword or beamPlusKeyword;
            public bool IsEditableId => Typ is diskKeyword or beamPlusKeyword;
            public bool IsEditableK => Typ is springKeyword or beamPlusKeyword;
            public bool IsEditableCm => Typ is magnetKeyword or beamPlusKeyword;
            /// <summary>
            /// Pole IsEditable v pořadí, jako jsou sloupce v tabulce
            /// </summary>
            public bool[] IsEditableArray { get {
                    bool[] b = { true, IsEditableL, IsEditableDe, IsEditableDi, IsEditableM, IsEditableIo, IsEditableId, IsEditableK, IsEditableCm};
                    return b; } }
        }

        /// <summary>
        /// Z kolekce PrvkyHrideleTab vytvoří List PrvkyHridele, které se použijí pro výpočet.
        /// </summary>
        public void VytvorPrvky()
        {
            List<Prvek> prvkyHridele = new List<Prvek>();

            foreach (PrvekTab p in PrvkyHrideleTab)
            {
                if (p.Typ == beamPlusKeyword)
                {
                    double li = p.L / (p.Deleni + 1);
                    double mi = p.M / p.Deleni;
                    double ioi = p.Io / p.Deleni;
                    double idi = p.IdN == 0 ? p.Id / p.Deleni * p.IdNValue : p.IdNValue;
                    double ki = p.K / p.Deleni;
                    double cmi = p.Cm / p.Deleni;

                    string[] typy = { beamKeyword, diskKeyword, springKeyword, magnetKeyword };

                    for (int i = 0; i < p.Deleni; i++)
                    {
                        foreach (string typ in typy)
                        {
                            prvkyHridele.Add(new Prvek()
                            {
                                L = li / 1000.0,
                                De = p.De / 1000.0,
                                Di = p.Di / 1000.0,
                                M = mi,
                                Io = ioi,
                                Id = idi,
                                K = ki,
                                Cm = cmi,
                                Rpm = 0,
                                Gyros = Gyros,
                                Rho = Rho,
                                E = ModulPruznosti * Math.Pow(10, radJednotkyModuluPruznosti),
                                Typ = typ
                            });
                        }
                    }

                    prvkyHridele.Add(new Prvek()
                    {
                        L = li / 1000.0,
                        De = p.De / 1000.0,
                        Di = p.Di / 1000.0,
                        M = mi,
                        Io = ioi,
                        Id = idi,
                        K = ki,
                        Cm = cmi,
                        Rpm = 0,
                        Gyros = Gyros,
                        Rho = Rho,
                        E = ModulPruznosti * Math.Pow(10, radJednotkyModuluPruznosti),
                        Typ = beamKeyword
                    });

                }
                else
                {
                    prvkyHridele.Add(new Prvek() { 
                        L = p.L / 1000.0,
                        De = p.De / 1000.0,
                        Di = p.Di / 1000.0,
                        M = p.M,
                        Io = p.Io,
                        Id = p.Id,
                        K = p.K,
                        Cm = p.Cm,
                        Rpm = 0,
                        Gyros = Gyros,
                        Rho = Rho,
                        E = ModulPruznosti * Math.Pow(10, radJednotkyModuluPruznosti),
                        Typ = p.Typ
                    });
                }
            }
            PrvkyHridele = prvkyHridele;
        }
    }
}
