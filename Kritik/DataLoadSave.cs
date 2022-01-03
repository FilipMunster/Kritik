using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Kritik
{
    public static class DataLoadSave
    {
        /// <summary>
        /// Načtení vstupních dat hřídele z excelu do kolekce PrvkyHrideleTab
        /// </summary>
        /// <param name="fileName">Plná cesta ke vstupnímu souboru</param>
        /// <param name="hridel">Objekt hřídele, kam budou data uložena</param>
        /// <returns>Vrací true, pokud se soubor podařilo načíst</returns>
        public static bool NacistData(string fileName, Hridel hridel)
        {
            // Načtení .xlsx do proměnné excel
            FileInfo fileInfo = new FileInfo(fileName);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet excel;
                excel = package.Workbook.Worksheets[Hridel.excelListZadani];
                if (excel == null) { excel = package.Workbook.Worksheets.FirstOrDefault(); }

                int xRows;

                if ((excel != null) && (excel.Dimension != null))
                {
                    // počet řádků eXcelové tabulky
                    xRows = excel.Dimension.Rows;
                }
                else
                {
                    Debug.WriteLine("Chyba: Nepodařilo se načíst soubor se vstupními daty.");
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
                            case Hridel.vypocetNazevKeyword:
                                hridel.VypocetNazev = excel.Cells[i, 2].Value.ToString();
                                break;
                            case Hridel.vypocetPopisKeyword:
                                hridel.VypocetPopis = excel.Cells[i, 2].Value.ToString();
                                break;
                            case Hridel.vypocetResilKeyword:
                                hridel.VypocetResil = excel.Cells[i, 2].Value.ToString();
                                break;
                            case Hridel.vypocetDatumKeyword:
                                try
                                {
                                    DateTime datum = DateTime.Parse(excel.Cells[i, 2].Value.ToString());
                                    hridel.VypocetDatum = datum.ToShortDateString();
                                }
                                catch
                                {
                                    hridel.VypocetDatum = String.Empty;
                                }

                                break;
                            case Hridel.opLevaKeyword:
                                hridel.OpLeva = excel.Cells[i, 2].Value.ToString();
                                break;
                            case Hridel.opPravaKeyword:
                                hridel.OpPrava = excel.Cells[i, 2].Value.ToString();
                                break;
                            case Hridel.gyrosKeyword:
                                hridel.Gyros = excel.Cells[i, 2].Value.ToString();
                                break;
                            case Hridel.modulPruznostiKeyword:
                                hridel.ModulPruznosti = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case Hridel.rhoKeyword:
                                hridel.Rho = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case Hridel.otackyProvozniKeyword:
                                hridel.OtackyProvozni = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case Hridel.otackyPrubezneKeyword:
                                hridel.OtackyPrubezne = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case Hridel.nKritMaxKeyword:
                                hridel.NKritMax = Convert.ToDouble(excel.Cells[i, 2].Value);
                                break;
                            case Hridel.poznamkaKeyword:
                                hridel.Poznamka = excel.Cells[i, 2].Value.ToString();
                                break;
                            default:
                                if (excel.Cells[i, 1].Value.ToString() == hridel.nazvySloupcu[0])
                                {
                                    for (int j = 0; j < hridel.nazvySloupcu.Length; j++) // kontrola, jestli názvy všech sloupců sedí
                                    {
                                        if (excel.Cells[i, j + 1].Value.ToString() != hridel.nazvySloupcu[j])
                                        {
                                            Debug.WriteLine("Chyba ve vstupním souboru - špatně zadané sloupce dat článků.");
                                            return false;
                                        }
                                    }
                                    dataClankuPrvniRadek = i + 1;
                                }
                                break;
                        }
                    }
                }

                // Naplnit kolekci PrvkyHrideleTab daty ze vstupního souboru
                if (dataClankuPrvniRadek > 0)
                {
                    hridel.PrvkyHrideleTab = new ObservableCollection<Hridel.PrvekTab>();
                    for (int i = dataClankuPrvniRadek; i <= xRows; i++)
                    {
                        hridel.PrvkyHrideleTab.Add(new Hridel.PrvekTab
                        {
                            Typ = excel.Cells[i, 1].Value.ToString(),
                            L = Convert.ToDouble(excel.Cells[i, 2].Value),
                            De = Convert.ToDouble(excel.Cells[i, 3].Value),
                            Di = Convert.ToDouble(excel.Cells[i, 4].Value),
                            M = Convert.ToDouble(excel.Cells[i, 5].Value),
                            Io = Convert.ToDouble(excel.Cells[i, 6].Value),
                            Id = Convert.ToDouble(excel.Cells[i, 7].Value),
                            K = Convert.ToDouble(excel.Cells[i, 8].Value),
                            Cm = Convert.ToDouble(excel.Cells[i, 9].Value),
                            Deleni = Convert.ToDouble(excel.Cells[i, 10].Value),
                            IdN = Convert.ToInt32(excel.Cells[i, 11].Value),
                            IdNValue = Convert.ToDouble(excel.Cells[i, 12].Value)
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("Chyba ve vstupním souboru - nebylo nalezeno zadání hřídele");
                    return false;
                }
                Debug.WriteLine("Soubor {0} byl načten.", fileName);
                return true;
            }
        }

        /// <summary>
        /// Uloží data hřídele do souboru .xlsx
        /// </summary>
        /// <param name="fileName">Úplná cesta k výstupnímu souboru</param>
        /// <param name="hridel">Objekt hřídele s daty</param>
        /// <returns>Vrací true, pokud se soubor podařilo uložit</returns>
        public static bool UlozitData(string fileName, Hridel hridel)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileInfo fileInfo = new FileInfo(fileName);
            using (ExcelPackage p = new ExcelPackage(fileInfo))
            {
                if (p.Workbook.Worksheets[Hridel.excelListZadani] != null)
                {
                    p.Workbook.Worksheets.Delete(Hridel.excelListZadani);
                }
                ExcelWorksheet ws = p.Workbook.Worksheets.Add(Hridel.excelListZadani);
                
                ws.Cells.Style.Font.Bold = false;
                ws.Cells.Style.Font.Color.SetColor(Color.Black);
                ws.Cells.Style.Font.Italic = false;
                ws.Cells.Style.Font.UnderLine = false;
                ws.Cells.Style.Font.Size = 11;
                ws.Cells.Style.Font.Name = "Calibri";

                ws.Cells[1, 1].Value = "KRITICKÉ OTÁČKY - VSTUPNÍ DATA";
                ws.Cells[3, 1].Value = Hridel.vypocetNazevKeyword;
                ws.Cells[3, 2].Value = hridel.VypocetNazev;
                ws.Cells[4, 1].Value = Hridel.vypocetPopisKeyword;
                ws.Cells[4, 2].Value = hridel.VypocetPopis;
                ws.Cells[5, 1].Value = Hridel.vypocetResilKeyword;
                ws.Cells[5, 2].Value = hridel.VypocetResil;
                ws.Cells[6, 1].Value = Hridel.vypocetDatumKeyword;
                ws.Cells[6, 2].Value = hridel.VypocetDatum;
                ws.Cells[8, 1].Value = "Okrajové podmínky:";
                ws.Cells[8, 3].Value = "(" + Hridel.opVolnyKeyword + " / " + Hridel.opKloubKeyword + " / " + Hridel.opVetknutiKeyword + ")";
                ws.Cells[9, 1].Value = Hridel.opLevaKeyword;
                ws.Cells[9, 2].Value = hridel.OpLeva;
                ws.Cells[10, 1].Value = Hridel.opPravaKeyword;
                ws.Cells[10, 2].Value = hridel.OpPrava;
                ws.Cells[12, 1].Value = "Gyroskopické účinky:";
                ws.Cells[12, 3].Value = "(" + Hridel.gyrosZanedbaniKeyword + " / " + Hridel.gyrosSoubeznaKeyword + " / " + Hridel.gyrosProtibeznaKeyword + ")";
                ws.Cells[13, 1].Value = Hridel.gyrosKeyword;
                ws.Cells[13, 2].Value = hridel.Gyros;
                ws.Cells[15, 1].Value = "Materiál hřídele:";
                ws.Cells[16, 1].Value = Hridel.modulPruznostiKeyword;
                ws.Cells[16, 2].Value = hridel.ModulPruznosti;
                ws.Cells[16, 3].Value = Hridel.jednotkaModuluPruznosti;
                ws.Cells[17, 1].Value = Hridel.rhoKeyword;
                ws.Cells[17, 2].Value = hridel.Rho;
                ws.Cells[17, 3].Value = "kg/m3";
                ws.Cells[19, 1].Value = "Otáčky hřídele:";
                ws.Cells[20, 1].Value = Hridel.otackyProvozniKeyword;
                ws.Cells[20, 2].Value = hridel.OtackyProvozni;
                ws.Cells[20, 3].Value = "rpm";
                ws.Cells[21, 1].Value = Hridel.otackyPrubezneKeyword;
                ws.Cells[21, 2].Value = hridel.OtackyPrubezne;
                ws.Cells[21, 3].Value = "rpm";
                ws.Cells[22, 1].Value = Hridel.nKritMaxKeyword;
                ws.Cells[22, 2].Value = hridel.NKritMax;
                ws.Cells[22, 3].Value = "rpm";
                ws.Cells[24, 1].Value = "Poznámky k výpočtu:";
                ws.Cells[25, 1].Value = Hridel.poznamkaKeyword;
                ws.Cells[25, 2].Value = hridel.Poznamka;
                ws.Cells[27, 1].Value = "Data článků:";
                ws.Cells[27, 3].Value = "(L, De, Di - [mm]; m - [kg]; Io, Id - [kg.m2]; k, Cm - [N/m]; Deleni - [-])";
                for (int i = 0; i < hridel.nazvySloupcu.Length; i++)
                {
                    ws.Cells[28, i + 1].Value = hridel.nazvySloupcu[i];
                }

                int row = 29; // první řádek dat prvků
                if (hridel.PrvkyHrideleTab != null)
                {
                    foreach (Hridel.PrvekTab a in hridel.PrvkyHrideleTab)
                    {
                        ws.Cells[row, 1].Value = a.Typ;
                        ws.Cells[row, 2].Value = a.L;
                        ws.Cells[row, 3].Value = a.De;
                        ws.Cells[row, 4].Value = a.Di;
                        ws.Cells[row, 5].Value = a.M;
                        ws.Cells[row, 6].Value = a.Io;
                        ws.Cells[row, 7].Value = a.Id;
                        ws.Cells[row, 8].Value = a.K;
                        ws.Cells[row, 9].Value = a.Cm;
                        ws.Cells[row, 10].Value = a.Deleni;
                        ws.Cells[row, 11].Value = a.IdN;
                        ws.Cells[row, 12].Value = a.IdNValue;
                        row++;
                    }
                }
                try
                {
                    p.Save();
                }
                catch
                {
                    Debug.WriteLine("Chyba při ukládání dat hřídele do souboru: {0}", fileName);
                    return false;
                }
            }
            Debug.WriteLine("Soubor {0} byl uložen.", fileName);
            return true;
        }

        /// <summary>
        /// Uloží výsledky výpočtu do souboru .xlsx
        /// </summary>
        /// <param name="fileName">Úplná cesta k výstupnímu souboru</param>
        /// <param name="hridel">Objekt hřídele s daty</param>
        /// <returns></returns>
        public static bool UlozitVysledky(string fileName, Hridel hridel)
        {
            Dictionary<string, string> opVypsatDict = new()
            {
                { Hridel.opVolnyKeyword, "VOLNÝ" },
                { Hridel.opKloubKeyword, "KLOUB" },
                { Hridel.opVetknutiKeyword, "VETKNUTÍ" }
            };
            Dictionary<string, string> gyrosVypsatDict = new()
            {
                { Hridel.gyrosZanedbaniKeyword, "VLIV GYROSKOPICKÝCH ÚČINKŮ NENÍ UVAŽOVÁN" },
                { Hridel.gyrosSoubeznaKeyword, "SOUBĚŽNÁ PRECESE" },
                { Hridel.gyrosProtibeznaKeyword, "PROTIBĚŽNÁ PRECESE" }
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileInfo fileInfo = new FileInfo(fileName);
            using (ExcelPackage p = new ExcelPackage(fileInfo))
            {
                if (p.Workbook.Worksheets[Hridel.excelListVysledky] != null)
                {
                    p.Workbook.Worksheets.Delete(Hridel.excelListVysledky);
                }
                // Pokud ještě nebyl proveden výpočet (-> hridel je null), tak to neřeším, jenom smažu list s výsledky
                if (hridel == null) {
                    try
                    {
                        p.Save();
                        return true;
                    }
                    catch
                    {
                        Debug.WriteLine("Chyba při ukládání výsledků do souboru: {0}", fileName);
                        return false;
                    }
                }
                ExcelWorksheet ws = p.Workbook.Worksheets.Add(Hridel.excelListVysledky);

                ws.Cells.Style.Font.Bold = false;
                ws.Cells.Style.Font.Color.SetColor(Color.Black);
                ws.Cells.Style.Font.Italic = false;
                ws.Cells.Style.Font.UnderLine = false;
                ws.Cells.Style.Font.Size = 11;
                ws.Cells.Style.Font.Name = "Calibri";

                ws.Cells[1, 1].Value = "KRITICKÉ OTÁČKY KROUŽIVÉHO KMITÁNÍ";
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[3, 1].Value = "Název:";
                ws.Cells[3, 2].Value = hridel.VypocetNazev;
                ws.Cells[4, 1].Value = "Popis:";
                ws.Cells[4, 2].Value = hridel.VypocetPopis;
                ws.Cells[5, 1].Value = "Řešil:";
                ws.Cells[5, 2].Value = hridel.VypocetResil;
                ws.Cells[6, 1].Value = "Datum:";
                ws.Cells[6, 2].Value = hridel.VypocetDatum;
                ws.Cells[8, 1].Value = "Okrajové podmínky:";
                ws.Cells[9, 2].Value = "LEVÝ konec rotoru:";
                ws.Cells[9, 4].Value = opVypsatDict[hridel.OpLeva];
                ws.Cells[9, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                ws.Cells[10, 2].Value = "PRAVÝ konec rotoru:";
                ws.Cells[10, 4].Value = opVypsatDict[hridel.OpPrava];
                ws.Cells[10, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                ws.Cells[11, 1].Value = "Modul pružnosti v tahu hřídele:";
                ws.Cells[11, 4].Value = hridel.ModulPruznosti;
                ws.Cells[11, 5].Value = Hridel.jednotkaModuluPruznosti;
                ws.Cells[12, 1].Value = "Hustota materiálu hřídele:";
                ws.Cells[12, 4].Value = hridel.Rho;
                ws.Cells[12, 5].Value = "kg.m⁻³";
                ws.Cells[14, 1].Value = gyrosVypsatDict[hridel.Gyros];
                int row = 15;
                if (hridel.OtackyProvozni > 0)
                {
                    ws.Cells[++row, 1].Value = "Provozní otáčky hřídele:";
                    ws.Cells[row, 4].Value = hridel.OtackyProvozni;
                    ws.Cells[row, 5].Value = "min⁻¹";
                }
                if (hridel.OtackyPrubezne > 0)
                {
                    ws.Cells[++row, 1].Value = "Průběžné otáčky hřídele:";
                    ws.Cells[row, 4].Value = hridel.OtackyPrubezne;
                    ws.Cells[row, 5].Value = "min⁻¹";
                }
                if (hridel.Poznamka != "")
                {
                    row = hridel.OtackyProvozni > 0 || hridel.OtackyPrubezne > 0 ? row + 2 : row + 1;
                    ws.Cells[row, 1].Value = "Poznámky k výpočtu:";
                    ws.Cells[++row, 2].Value = hridel.Poznamka;
                    ws.Cells[row, 2, row, 8].Merge = true;
                    ws.Cells[row, 2].Style.WrapText = true;
                    ws.Row(row).Height = MeasureTextHeight(hridel.Poznamka, ws.Cells[row, 1].Style.Font, ws.Column(1).Width * 7);
                }
                row = hridel.OtackyProvozni > 0 || hridel.OtackyPrubezne > 0 || hridel.Poznamka != "" ? row + 1 : row;
                ws.Cells[++row, 1].Value = "Vypočtené hodnoty:";
                ws.Cells[row, 1].Style.Font.Bold = true;
                for (int i = 0; i < hridel.KritOt.Length; i++)
                {
                    ws.Cells[++row, 2].Value = i + 1 + ". kritické otáčky:";
                    ws.Cells[row, 4].Value = hridel.KritOt[i];
                    ws.Cells[row, 5].Value = "min⁻¹";
                    ws.Cells[row, 6].Value = hridel.KritOt[i] / 60.0;
                    ws.Cells[row, 6].Style.Numberformat.Format = "(0.0##)";
                    ws.Cells[row, 7].Value = "Hz";
                }
                if (hridel.KritOt.Length == 0) { ws.Cells[++row, 2].Value = "Nebyly vypočteny žádné kritické otáčky."; }

                if ((hridel.OtackyProvozni > 0 || hridel.OtackyPrubezne > 0) && hridel.KritOt.Length > 0)
                {
                    row += 2;
                    ws.Cells[row, 2].Value = "1. kritické otáčky odpovídají:";
                }
                if (hridel.OtackyProvozni > 0 && hridel.KritOt.Length > 0)
                {
                    ws.Cells[++row, 2].Value = hridel.KritOt[0] / hridel.OtackyProvozni * 100;
                    ws.Cells[row, 2].Style.Numberformat.Format = "0.0##";
                    ws.Cells[row, 3].Value = "% provozních otáček";
                }
                if (hridel.OtackyPrubezne > 0 && hridel.KritOt.Length > 0)
                {
                    ws.Cells[++row, 2].Value = hridel.KritOt[0] / hridel.OtackyPrubezne * 100;
                    ws.Cells[row, 2].Style.Numberformat.Format = "0.0##";
                    ws.Cells[row, 3].Value = "% průběžných otáček";
                }

                row += 2;
                ws.Cells[row, 1].Value = "Geometrie hřídele:";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 3].Value = "(L, De, Di - [mm]; m - [kg]; Jo, Jd - [kg.m2]; k, Cm - [N/m])";

                if (hridel.PrvkyHrideleTab != null)
                {
                    var alignLeft = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    var alignRight = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    string formatNum = "0.0#####";

                    int i = 0;
                    foreach (Hridel.PrvekTab a in hridel.PrvkyHrideleTab)
                    {
                        i++; row++;
                        ws.Cells[row, 1].Value = i;
                        ws.Cells[row, 1].Style.Numberformat.Format = "0\".\"";
                        ws.Cells[row, 2].Value = Hridel.TypDict[a.Typ];
                        switch (a.Typ)
                        {
                            case Hridel.beamKeyword:
                                ws.Cells[row, 3].Value = "L = ";
                                ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 4].Value = a.L;
                                ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                ws.Cells[row, 5].Value = "De = ";
                                ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 6].Value = a.De;
                                ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                ws.Cells[row, 6].Style.Numberformat.Format = formatNum;
                                ws.Cells[row, 7].Value = "Di = ";
                                ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 8].Value = a.Di;
                                ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                ws.Cells[row, 8].Style.Numberformat.Format = formatNum;
                                break;
                            case Hridel.beamPlusKeyword:
                                ws.Cells[row, 3].Value = "L = ";
                                ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 4].Value = a.L;
                                ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                ws.Cells[row, 5].Value = "De = ";
                                ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 6].Value = a.De;
                                ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                ws.Cells[row, 6].Style.Numberformat.Format = formatNum;
                                ws.Cells[row, 7].Value = "Di = ";
                                ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 8].Value = a.Di;
                                ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                ws.Cells[row, 8].Style.Numberformat.Format = formatNum;
                                ws.Cells[++row, 2].Value = "Hřídel je rozdělena na " + (a.Deleni + 1) + " částí o délce " + string.Format("{0:#.###}", a.L / (a.Deleni + 1)) + " mm, mezi kterými jsou umístěny prvky:";
                                if (a.M > 0)
                                {
                                    ws.Cells[++row, 2].Value = Hridel.TypDict[Hridel.diskKeyword];
                                    ws.Cells[row, 3].Value = "m = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = a.M / a.Deleni;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    if (hridel.Gyros != Hridel.gyrosZanedbaniKeyword)
                                    {
                                        ws.Cells[row, 5].Value = "Jo = ";
                                        ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 6].Value = a.Io / a.Deleni;
                                        ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                        ws.Cells[row, 7].Value = "Jd = ";
                                        ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 8].Value = a.IdN == 0 ? a.Id / a.Deleni * a.IdNValue : a.IdNValue;
                                        ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                    }
                                }
                                if (a.K > 0)
                                {
                                    ws.Cells[++row, 2].Value = Hridel.TypDict[Hridel.springKeyword];
                                    ws.Cells[row, 3].Value = "k = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = a.K / a.Deleni;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                }
                                if (a.Cm > 0)
                                {
                                    ws.Cells[++row, 2].Value = Hridel.TypDict[Hridel.magnetKeyword];
                                    ws.Cells[row, 3].Value = "Cm = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = a.Cm / a.Deleni;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                }
                                break;
                            case Hridel.rigidKeyword:
                                ws.Cells[row, 3].Value = "L = ";
                                ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 4].Value = a.L;
                                ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                break;
                            case Hridel.diskKeyword:
                                ws.Cells[row, 3].Value = "m = ";
                                ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 4].Value = a.M;
                                ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                if (hridel.Gyros != Hridel.gyrosZanedbaniKeyword)
                                {
                                    ws.Cells[row, 5].Value = "Jo = ";
                                    ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 6].Value = a.Io;
                                    ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 7].Value = "Jd = ";
                                    ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 8].Value = a.Id;
                                    ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                }
                                break;
                            case Hridel.springKeyword:
                                ws.Cells[row, 3].Value = "k = ";
                                ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 4].Value = a.K;
                                ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                break;
                            case Hridel.magnetKeyword:
                                ws.Cells[row, 3].Value = "Cm = ";
                                ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                ws.Cells[row, 4].Value = a.Cm;
                                ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                break;
                        }
                    }
                }
                try
                {
                    p.Save();
                }
                catch
                {
                    Debug.WriteLine("Chyba při ukládání výsledků do souboru: {0}", fileName);
                    return false;
                }
            }
            Debug.WriteLine("Soubor {0} byl uložen.", fileName);
            return true;
        }

        /// <summary>
        /// Metoda pro nastavení výšky excelovské buňky podle délky textu.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private static double MeasureTextHeight(string text, ExcelFont font, double width)
        {
            if (text is null or "") { return 0.0; }

            var bitmap = new Bitmap(1, 1);
            var graphics = Graphics.FromImage(bitmap);

            var pixelWidth = Convert.ToInt32(width * 7);  //7 pixels per excel column width
            var fontSize = font.Size * 0.82f;
            var drawingFont = new Font(font.Name, fontSize);
            var size = graphics.MeasureString(text, drawingFont, pixelWidth, new StringFormat { FormatFlags = StringFormatFlags.MeasureTrailingSpaces });

            bitmap.Dispose();
            graphics.Dispose();

            //72 DPI and 96 points per inch.  Excel height in points with max of 409 per Excel requirements.
            double vysledek = Math.Min(Convert.ToDouble(size.Height) * 72 / 96, 409);
            vysledek = vysledek < 15 ? 15 : vysledek;
            return vysledek;
        }
    }
}
