using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;


namespace Kritik
{
    /// <summary>
    /// 
    /// </summary>
    public class Hridel
    {
        // klíčová slova ve vstupním souboru
        public readonly string[] nazvySloupcu = { "Typ", "L", "De", "Di", "m", "Io", "Id", "k", "Cm", "Deleni", "Id/N", "Id/Nvalue"};
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

        // proměnné s parametry výpočtu
        public string VypocetNazev { get; set; }
        public string VypocetPopis { get; set; }
        public string VypocetResil { get; set; }
        public DateTime VypocetDatum { get; set; }
        public string OpLeva { get; set; }
        public string OpPrava { get; set; }
        public string Gyros { get; set; }
        public double ModulPruznosti { get; set; }
        public double Rho { get; set; }
        public double OtackyProvozni { get; set; }
        public double OtackyPrubezne { get; set; }
        public double NKritMax { get; set; }
        public string Poznamka { get; set; }

        /// <summary>
        /// Nastavení výchozích parametrů
        /// </summary>
        public Hridel()
        {
            VypocetNazev = "";
            VypocetPopis = "";
            VypocetResil = "";
            VypocetDatum = DateTime.Today;
            OpLeva = opVolnyKeyword;
            OpPrava = opVolnyKeyword;
            Gyros = gyrosZanedbaniKeyword;
            ModulPruznosti = 210;
            Rho = 7850;
            OtackyProvozni = 0;
            OtackyPrubezne = 0;
            NKritMax = 5000;
    }

        /// <summary>
        /// Tabulka s daty článků
        /// </summary>
        public DataTable DataClankuTab { get; set; }

        public List<Prvek> PrvkyHridele { get; private set; }

        /// <summary>
        /// Třída Prvků hřídele
        /// </summary>
        public class Prvek
        {
            private string typ;
            public string Typ { get { return typ; } set { typ = value; VytvorMatici(); } }
            private double l;
            public double L { get { return l; } set { l = value; VytvorMatici(); } }
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
            public Prvek(string typ, double l, double de, double di, double m, double io, double id, double k, double cm, double rpm, string gyros, double rho, double e)
            {
                this.typ = typ;
                this.l = l;
                this.de = de;
                this.di = di;
                this.m = m;
                this.io = io;
                this.id = id;
                this.k = k;
                this.cm = cm;
                this.rpm = rpm;
                this.gyros = gyros;
                this.rho = rho;
                this.e = e;
                VytvorMatici();
            }

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
                                    Console.WriteLine("Špatně zadané gyroskopické účinky ({0})", Gyros);
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
                        Console.WriteLine("Špatně zadaný typ prvku ({0})", typ);
                        break;
                }
            }
        }    

        /// <summary>
        /// Načtení vstupních dat hřídele z excelu
        /// </summary>
        /// <param name="fileName">Plná cesta k souboru</param>
        /// <returns>Vrací true, pokud se soubor podařilo načíst</returns>
        public bool NacistData(string fileName)
        {
            // Načtení .xlsx do proměnné excel
            FileInfo fileInfo = new FileInfo(fileName);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet excel = package.Workbook.Worksheets.FirstOrDefault();

                int xRows;

                if ((excel != null) && (excel.Dimension != null))
                {
                    // počet řádků eXcelové tabulky
                    xRows = excel.Dimension.Rows;
                }
                else
                {
                    Console.WriteLine("Chyba: Nepodařilo se načíst soubor se vstupními daty.");
                    DataClankuTab = InicializaceTabulky();
                    return false;
                }

                // Načtení zadaných hodnot
                int dataClankuPrvniRadek = 0;

                for (int i = 1; i <= xRows; i++)
                {
                    if ((excel.Cells[i, 1].Value != null) && (excel.Cells[i, 2].Value != null))
                    {
                        switch (excel.Cells[i, 1].Value.ToString())
                        {
                            case vypocetNazevKeyword:
                                VypocetNazev = excel.Cells[i, 2].Value.ToString();
                                break;
                            case vypocetPopisKeyword:
                                VypocetPopis = excel.Cells[i, 2].Value.ToString();
                                break;
                            case vypocetResilKeyword:
                                VypocetResil = excel.Cells[i, 2].Value.ToString();
                                break;
                            case vypocetDatumKeyword:
                                VypocetDatum = DateTime.Parse(excel.Cells[i, 2].Value.ToString());
                                break;
                            case opLevaKeyword:
                                OpLeva = excel.Cells[i, 2].Value.ToString();
                                break;
                            case opPravaKeyword:
                                OpPrava = excel.Cells[i, 2].Value.ToString();
                                break;
                            case gyrosKeyword:
                                Gyros = excel.Cells[i, 2].Value.ToString();
                                break;
                            case modulPruznostiKeyword:
                                ModulPruznosti = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case rhoKeyword:
                                Rho = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case otackyProvozniKeyword:
                                OtackyProvozni = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case otackyPrubezneKeyword:
                                OtackyPrubezne = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case nKritMaxKeyword:
                                NKritMax = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case poznamkaKeyword:
                                Poznamka = excel.Cells[i, 2].Value.ToString();
                                break;
                            default:
                                if (excel.Cells[i, 1].Value.ToString() == nazvySloupcu[0])
                                {
                                    for (int j = 0; j < nazvySloupcu.Length; j++) // kontrola, jestli názvy všech sloupců sedí
                                    {
                                        if (excel.Cells[i, j + 1].Value.ToString() != nazvySloupcu[j])
                                        {
                                            Console.WriteLine("Chyba ve vstupním souboru - špatně zadané sloupce dat článků.");
                                            return false;
                                        }
                                    }
                                    dataClankuPrvniRadek = i + 1;
                                }
                                break;
                        }

                    }
                }

                // vytvořit a naplnit tabulku DataClankuTab daty článku z excelu
                DataTable dataClanku = InicializaceTabulky();
                DataRow row;
                int cisloRadku = 0;

                if (dataClankuPrvniRadek>0)
                {
                    for (int i = dataClankuPrvniRadek; i <= xRows; i++)
                    {
                        row = dataClanku.NewRow();
                        if (excel.Cells[i, 1].Value != null) // pokud v prvním sloupci nic není, načítání skončí
                        {
                            cisloRadku++;
                            row[0] = cisloRadku.ToString();
                            row[1] = excel.Cells[i, 1].Value.ToString();
                        }
                        else { break; }
                    

                        for (int j = 2; j <= nazvySloupcu.Length; j++)
                        {
                            if (excel.Cells[i, j].Value != null)
                            {
                                row[j] = Convert.ToDouble(excel.Cells[i, j].Value);
                            }
                            else
                            {
                                row[j] = 0.0;
                            }
                        }
                        dataClanku.Rows.Add(row);
                    }
                }


                DataClankuTab = dataClanku;
                Console.WriteLine("Soubor {0} byl načten.", fileName);
                return true;
            }
        }

        /// <summary>
        /// Vytvoří a vrátí prázdnou tabulku s pojmenovanými sloupci parametrů hřídele
        /// </summary>
        public DataTable InicializaceTabulky()
        {
            // Vytvořit tabulku
            DataTable dataClanku = new DataTable("Data článků");
            // Vytvořit proměnnou pro objekt DataColumn
            DataColumn column;

            // Vytvořit jednotlivé sloupce
            column = new DataColumn();
            column.DataType = Type.GetType("System.Int32");
            column.ColumnName = "#";
            column.ReadOnly = false;
            column.Unique = true;
            dataClanku.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = nazvySloupcu[0];
            column.ReadOnly = false;
            column.Unique = false;
            dataClanku.Columns.Add(column);

            foreach (string nazevSloupce in nazvySloupcu.Skip(1))
            {
                column = new DataColumn();
                column.DataType = Type.GetType("System.Double");
                column.ColumnName = nazevSloupce;
                column.ReadOnly = false;
                column.Unique = false;
                dataClanku.Columns.Add(column);
            }
            return dataClanku;
        }

        /// <summary>
        /// Uloží data hřídele do souboru .xlsx
        /// </summary>
        /// <param name="fileName">Úplná cesta k souboru</param>
        /// <returns>Vrací true, pokud se soubor podařilo uložit</returns>
        public bool UlozitData(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            using (ExcelPackage p = new ExcelPackage(fileInfo))
            {
                if (p.Workbook.Worksheets["KRITIK"] != null)
                {
                    p.Workbook.Worksheets.Delete("KRITIK");
                }
                ExcelWorksheet ws = p.Workbook.Worksheets.Add("KRITIK");

                ws.Cells[1, 1].Value = "KRITICKÉ OTÁČKY - DATA";
                ws.Cells[3, 1].Value = vypocetNazevKeyword;
                ws.Cells[3, 2].Value = VypocetNazev;
                ws.Cells[4, 1].Value = vypocetPopisKeyword;
                ws.Cells[4, 2].Value = VypocetPopis;
                ws.Cells[5, 1].Value = vypocetResilKeyword;
                ws.Cells[5, 2].Value = VypocetResil;
                ws.Cells[6, 1].Value = vypocetDatumKeyword;
                ws.Cells[6, 2].Value = VypocetDatum.ToShortDateString();
                ws.Cells[8, 1].Value = "Okrajové podmínky:";
                ws.Cells[8, 3].Value = "("+opVolnyKeyword+" / "+opKloubKeyword+" / "+opVetknutiKeyword+")";
                ws.Cells[9, 1].Value = opLevaKeyword;
                ws.Cells[9, 2].Value = OpLeva;
                ws.Cells[10, 1].Value = opPravaKeyword;
                ws.Cells[10, 2].Value = OpPrava;
                ws.Cells[12, 1].Value = "Gyroskopické účinky:";
                ws.Cells[12, 3].Value = "("+gyrosZanedbaniKeyword+ " / "+gyrosSoubeznaKeyword+ " / "+gyrosProtibeznaKeyword+")";
                ws.Cells[13, 1].Value = gyrosKeyword;
                ws.Cells[13, 2].Value = Gyros;
                ws.Cells[15, 1].Value = "Materiál hřídele:";
                ws.Cells[16, 1].Value = modulPruznostiKeyword;
                ws.Cells[16, 2].Value = ModulPruznosti;
                ws.Cells[16, 3].Value = jednotkaModuluPruznosti;
                ws.Cells[17, 1].Value = rhoKeyword;
                ws.Cells[17, 2].Value = Rho;
                ws.Cells[17, 3].Value = "kg/m3";
                ws.Cells[19, 1].Value = "Otáčky hřídele:";
                ws.Cells[20, 1].Value = otackyProvozniKeyword;
                ws.Cells[20, 2].Value = OtackyProvozni;
                ws.Cells[20, 3].Value = "rpm";
                ws.Cells[21, 1].Value = otackyPrubezneKeyword;
                ws.Cells[21, 2].Value = OtackyPrubezne;
                ws.Cells[21, 3].Value = "rpm";
                ws.Cells[22, 1].Value = nKritMaxKeyword;
                ws.Cells[22, 2].Value = NKritMax;
                ws.Cells[22, 3].Value = "rpm";
                ws.Cells[24, 1].Value = "Poznámky k výpočtu:";
                ws.Cells[25, 1].Value = poznamkaKeyword;
                ws.Cells[25, 2].Value = Poznamka;
                ws.Cells[27, 1].Value = "Data článků:";
                ws.Cells[27, 3].Value = "(L, De, Di - [mm]; m - [kg]; Io, Id - [kg.m2]; k, Cm - [N/m]; Deleni - [-])";
                for (int i = 0; i < nazvySloupcu.Length; i++)
                {
                    ws.Cells[28, i + 1].Value = nazvySloupcu[i];
                }
                int row = 29;
                int col = 1;

                if ((DataClankuTab != null) && (DataClankuTab.Rows != null))
                {
                    foreach (DataRow r in DataClankuTab.Rows)
                    {
                        foreach (var item in r.ItemArray.Skip(1))
                        {
                            ws.Cells[row, col].Value = item;
                            col++;
                        }
                        row++;
                        col = 1;
                    }
                }


                try
                {
                    p.Save();
                }
                catch
                {
                    Console.WriteLine("Chyba při ukládání dat hřídele do souboru: {0}", fileName);
                    return false;
                }
                
            }
            Console.WriteLine("Soubor {0} byl uložen.", fileName);
            return true;
        }
        /// <summary>
        /// Vytvoří List prvků hřídele z tabulky DataClankuTab a uloží je do vlastnosti PrvkyHridele
        /// </summary>
        public void VytvorPrvky()
        {
            List<Prvek> prvkyHridele = new List<Prvek>();

            foreach (DataRow row in DataClankuTab.Rows)
            {
                var i = row.ItemArray;
                string typ = i[1].ToString();
                double l = Convert.ToDouble(i[2]) / 1000;
                double de = Convert.ToDouble(i[3]) / 1000;
                double di = Convert.ToDouble(i[4]) / 1000;
                double m = Convert.ToDouble(i[5]);
                double io = Convert.ToDouble(i[6]);
                double id = Convert.ToDouble(i[7]);
                double k = Convert.ToDouble(i[8]);
                double cm = Convert.ToDouble(i[9]);

                prvkyHridele.Add(new Prvek(typ, l, de, di, m, io, id, k, cm, 0, Gyros, Rho, ModulPruznosti*Math.Pow(10,radJednotkyModuluPruznosti)));

                // JEŠTĚ DODĚLAT ROZDĚLOVÁNÍ PRVKU HRIDEL+ !!!
            }
           
            PrvkyHridele = prvkyHridele;
        }
    }
}
