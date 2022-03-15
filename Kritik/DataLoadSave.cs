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
        // klíčová slova ve vstupním souboru
        private static readonly string[] colNames = { "Typ", "L", "De", "Di", "m", "Io", "Id", "k", "Cm", "Deleni", "Id/N", "Id/Nvalue" };
        private const string nazevKeyword = "Nazev";
        private const string popisKeyword = "Popis";
        private const string resilKeyword = "Resil";
        private const string datumKeyword = "Datum";
        private const string opLevaKeyword = "OP leva";
        private const string opPravaKeyword = "OP prava";
        private const string volnyKeyword = "volny";
        private const string vetknutiKeyword = "vetknuti";
        private const string kloubKeyword = "kloub";
        private const string gyrosKeyword = "GU";
        private const string soubeznaKeyword = "soubezna";
        private const string protibeznaKeyword = "protibezna";
        private const string zanedbaniKeyword = "zanedbani";
        private const string modulPruznostiKeyword = "E";
        private const string rhoKeyword = "rho";
        private const string otackyProvozniKeyword = "n";
        private const string otackyPrubezneKeyword = "nr";
        private const string nKritMaxKeyword = "nKritMax";
        private const string poznamkaKeyword = "Poznamka";
        private const string diskKeyword = "disk";
        private const string rigidKeyword = "tuhy";
        private const string beamKeyword = "hridel";
        private const string beamPlusKeyword = "hridel+";
        private const string springKeyword = "pruzina";
        private const string magnetKeyword = "magnet";
        private const int radJednotkyModuluPruznosti = 9;
        private const string excelListZadani = "KRITIK_zadani";
        private const string excelListVysledky = "KRITIK_vypocet";

        private static Dictionary<string, BoundaryCondition> boundaryConditionStringToEnum = new Dictionary<string, BoundaryCondition>()
        {
            {volnyKeyword, BoundaryCondition.free },
            {kloubKeyword, BoundaryCondition.joint },
            {vetknutiKeyword, BoundaryCondition.fix }
        };

        private static Dictionary<BoundaryCondition, string> boundaryConditionEnumToString = new Dictionary<BoundaryCondition, string>()
        {
            {BoundaryCondition.free, volnyKeyword },
            {BoundaryCondition.joint, kloubKeyword },
            {BoundaryCondition.fix, vetknutiKeyword }
        };

        private static Dictionary<string, GyroscopicEffect> gyroscopicEffectStringToEnum = new Dictionary<string, GyroscopicEffect>()
        {
            {zanedbaniKeyword, GyroscopicEffect.none },
            {soubeznaKeyword, GyroscopicEffect.forward },
            {protibeznaKeyword, GyroscopicEffect.backward }
        };

        private static Dictionary<GyroscopicEffect, string> gyroscopicEffectEnumToString = new Dictionary<GyroscopicEffect, string>()
        {
            {GyroscopicEffect.none, zanedbaniKeyword},
            {GyroscopicEffect.forward, soubeznaKeyword},
            {GyroscopicEffect.backward, protibeznaKeyword}
        };

        private static Dictionary<string, ElementType> elementTypeStringToEnum = new Dictionary<string, ElementType>()
        {
            {diskKeyword, ElementType.disc },
            {rigidKeyword, ElementType.rigid },
            {beamKeyword, ElementType.beam },
            {beamPlusKeyword, ElementType.beamPlus },
            {springKeyword, ElementType.support },
            {magnetKeyword, ElementType.magnet }
        };

        private static Dictionary<ElementType, string> elementTypeEnumToString = new Dictionary<ElementType, string>()
        {
            {ElementType.disc, diskKeyword },
            {ElementType.rigid, rigidKeyword },
            {ElementType.beam, beamKeyword },
            {ElementType.beamPlus, beamPlusKeyword },
            {ElementType.support, springKeyword },
            {ElementType.magnet, magnetKeyword }
        };

        /// <summary>
        /// Načtení vstupních dat hřídele z excelu do kolekce PrvkyHrideleTab
        /// </summary>
        /// <param name="fileName">Plná cesta ke vstupnímu souboru</param>
        public static (ShaftProperties, CalculationProperties, List<ShaftElement>)? Load(string fileName)
        {
            ShaftProperties shaftProperties = new();
            CalculationProperties calculationProperties = new();
            List<ShaftElement> shaftElements = new();

            // Načtení .xlsx do proměnné excel
            FileInfo fileInfo = new FileInfo(fileName);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet excel;
                    excel = package.Workbook.Worksheets[excelListZadani];

                    if (excel == null) { excel = package.Workbook.Worksheets.FirstOrDefault(); }

                    int numRows;

                    if ((excel != null) && (excel.Dimension != null))
                    {
                        // počet řádků eXcelové tabulky
                        numRows = excel.Dimension.Rows;
                    }
                    else
                    {
                        Debug.WriteLine("Chyba: Nepodařilo se načíst soubor se vstupními daty.");
                        return null;
                    }

                    // Načtení zadaných hodnot
                    int shaftElementsFirstRow = 0;

                    for (int i = 1; i <= numRows; i++)
                    {
                        if ((excel.Cells[i, 1].Value != null) && (excel.Cells[i, 2].Value != null))
                        {
                            string value = excel.Cells[i, 2].Value.ToString();
                            switch (excel.Cells[i, 1].Value.ToString())
                            {
                                case nazevKeyword:
                                    calculationProperties.Title = value;
                                    break;
                                case popisKeyword:
                                    calculationProperties.Description = value;
                                    break;
                                case resilKeyword:
                                    calculationProperties.Author = value;
                                    break;
                                case datumKeyword:
                                    calculationProperties.Date = value;
                                    break;
                                case opLevaKeyword:
                                    if (!boundaryConditionStringToEnum.TryGetValue(value, out BoundaryCondition bCLeft))
                                        bCLeft = BoundaryCondition.free;
                                    shaftProperties.BCLeft = bCLeft;
                                    break;
                                case opPravaKeyword:
                                    if (!boundaryConditionStringToEnum.TryGetValue(value, out BoundaryCondition bCRight))
                                        bCRight = BoundaryCondition.free;
                                    shaftProperties.BCLeft = bCRight;
                                    break;
                                case gyrosKeyword:
                                    if (!gyroscopicEffectStringToEnum.TryGetValue(value, out GyroscopicEffect gyros))
                                        gyros = GyroscopicEffect.none;
                                    shaftProperties.Gyros = gyros;
                                    break;
                                case modulPruznostiKeyword:
                                    if (Double.TryParse(value, out double e))
                                        shaftProperties.YoungModulus = e * Math.Pow(10, radJednotkyModuluPruznosti);
                                    break;
                                case rhoKeyword:
                                    if (Double.TryParse(value, out double rho))
                                        shaftProperties.MaterialDensity = rho;
                                    break;
                                case otackyProvozniKeyword:
                                    if (Double.TryParse(value, out double nOp))
                                        shaftProperties.OperatingSpeed = nOp;
                                    break;
                                case otackyPrubezneKeyword:
                                    if (Double.TryParse(value, out double nRA))
                                        shaftProperties.RunawaySpeed = nRA;
                                    break;
                                case nKritMaxKeyword:
                                    if (Double.TryParse(value, out double nMax))
                                        calculationProperties.MaxCriticalSpeed = nMax;
                                    break;
                                case poznamkaKeyword:
                                    calculationProperties.Notes = value;
                                    break;
                                default:
                                    if (excel.Cells[i, 1].Value.ToString() == colNames[0])
                                    {
                                        for (int j = 0; j < colNames.Length; j++) // kontrola, jestli názvy všech sloupců sedí
                                        {
                                            if (excel.Cells[i, j + 1].Value.ToString() != colNames[j])
                                            {
                                                Debug.WriteLine("Chyba ve vstupním souboru - špatně zadané sloupce dat článků.");
                                                return null;
                                            }
                                        }
                                        shaftElementsFirstRow = i + 1;
                                    }
                                    break;
                            }
                        }
                    }

                    // Naplnit kolekci PrvkyHrideleTab daty ze vstupního souboru
                    if (shaftElementsFirstRow > 0)
                    {
                        for (int i = shaftElementsFirstRow; i <= numRows; i++)
                        {
                            if (!elementTypeStringToEnum.TryGetValue(excel.Cells[i, 1].Value.ToString(), out ElementType type))
                                return null;

                            shaftElements.Add(new ShaftElement(type)
                            {
                                L = Convert.ToDouble(excel.Cells[i, 2].Value),
                                De = Convert.ToDouble(excel.Cells[i, 3].Value),
                                Di = Convert.ToDouble(excel.Cells[i, 4].Value),
                                M = Convert.ToDouble(excel.Cells[i, 5].Value),
                                Io = Convert.ToDouble(excel.Cells[i, 6].Value),
                                Id = Convert.ToDouble(excel.Cells[i, 7].Value),
                                K = Convert.ToDouble(excel.Cells[i, 8].Value),
                                Cm = Convert.ToDouble(excel.Cells[i, 9].Value),
                                Division = Convert.ToDouble(excel.Cells[i, 10].Value),
                                IdN = Convert.ToInt32(excel.Cells[i, 11].Value),
                                IdNValue = Convert.ToDouble(excel.Cells[i, 12].Value)
                            });
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Chyba ve vstupním souboru - nebylo nalezeno zadání hřídele");
                        return null;
                    }
                    Debug.WriteLine("Soubor {0} byl načten.", fileName);
                    return (shaftProperties, calculationProperties, shaftElements);
                }
            } catch { return null; }
        }

        /// <summary>
        /// Uloží data hřídele do souboru .xlsx
        /// </summary>
        /// <param name="fileName">Úplná cesta k výstupnímu souboru</param>
        /// <param name="hridel">Objekt hřídele s daty</param>
        /// <returns>Vrací true, pokud se soubor podařilo uložit</returns>
        public static bool Save(string fileName, ShaftProperties shaftProperties, CalculationProperties calculationProperties, List<ShaftElement> shaftElements, KritikResults kritikResults)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileInfo fileInfo = new FileInfo(fileName);
            try
            {
                using (ExcelPackage p = new ExcelPackage(fileInfo))
                {
                    if (p.Workbook.Worksheets[excelListZadani] is not null)
                    {
                        p.Workbook.Worksheets.Delete(excelListZadani);
                    }
                    ExcelWorksheet ws = p.Workbook.Worksheets.Add(excelListZadani);

                    ws.Cells.Style.Font.Bold = false;
                    ws.Cells.Style.Font.Color.SetColor(Color.Black);
                    ws.Cells.Style.Font.Italic = false;
                    ws.Cells.Style.Font.UnderLine = false;
                    ws.Cells.Style.Font.Size = 11;
                    ws.Cells.Style.Font.Name = "Calibri";

                    ws.Cells[1, 1].Value = "KRITICKÉ OTÁČKY - VSTUPNÍ DATA";
                    ws.Cells[3, 1].Value = nazevKeyword;
                    ws.Cells[3, 2].Value = calculationProperties.Title;
                    ws.Cells[4, 1].Value = popisKeyword;
                    ws.Cells[4, 2].Value = calculationProperties.Description;
                    ws.Cells[5, 1].Value = resilKeyword;
                    ws.Cells[5, 2].Value = calculationProperties.Author;
                    ws.Cells[6, 1].Value = datumKeyword;
                    ws.Cells[6, 2].Value = calculationProperties.Date;
                    ws.Cells[8, 1].Value = "Okrajové podmínky:";
                    ws.Cells[8, 3].Value = "(" + volnyKeyword + " / " + kloubKeyword + " / " + vetknutiKeyword + ")";
                    ws.Cells[9, 1].Value = opLevaKeyword;
                    ws.Cells[9, 2].Value = boundaryConditionEnumToString[shaftProperties.BCLeft];
                    ws.Cells[10, 1].Value = opPravaKeyword;
                    ws.Cells[10, 2].Value = boundaryConditionEnumToString[shaftProperties.BCRight];
                    ws.Cells[12, 1].Value = "Gyroskopické účinky:";
                    ws.Cells[12, 3].Value = "(" + zanedbaniKeyword + " / " + soubeznaKeyword + " / " + protibeznaKeyword + ")";
                    ws.Cells[13, 1].Value = gyrosKeyword;
                    ws.Cells[13, 2].Value = gyroscopicEffectEnumToString[shaftProperties.Gyros];
                    ws.Cells[15, 1].Value = "Materiál hřídele:";
                    ws.Cells[16, 1].Value = modulPruznostiKeyword;
                    ws.Cells[16, 2].Value = shaftProperties.YoungModulus;
                    ws.Cells[16, 3].Value = "GPa";
                    ws.Cells[17, 1].Value = rhoKeyword;
                    ws.Cells[17, 2].Value = shaftProperties.MaterialDensity;
                    ws.Cells[17, 3].Value = "kg/m3";
                    ws.Cells[19, 1].Value = "Otáčky hřídele:";
                    ws.Cells[20, 1].Value = otackyProvozniKeyword;
                    ws.Cells[20, 2].Value = shaftProperties.OperatingSpeed;
                    ws.Cells[20, 3].Value = "rpm";
                    ws.Cells[21, 1].Value = otackyPrubezneKeyword;
                    ws.Cells[21, 2].Value = shaftProperties.RunawaySpeed;
                    ws.Cells[21, 3].Value = "rpm";
                    ws.Cells[22, 1].Value = nKritMaxKeyword;
                    ws.Cells[22, 2].Value = calculationProperties.MaxCriticalSpeed;
                    ws.Cells[22, 3].Value = "rpm";
                    ws.Cells[24, 1].Value = "Poznámky k výpočtu:";
                    ws.Cells[25, 1].Value = poznamkaKeyword;
                    ws.Cells[25, 2].Value = calculationProperties.Notes;
                    ws.Cells[27, 1].Value = "Data článků:";
                    ws.Cells[27, 3].Value = "(L, De, Di - [mm]; m - [kg]; Io, Id - [kg.m2]; k, Cm - [N/m]; Deleni - [-])";
                    for (int i = 0; i < colNames.Length; i++)
                    {
                        ws.Cells[28, i + 1].Value = colNames[i];
                    }

                    int row = 29; // první řádek dat prvků
                    if (shaftElements != null)
                    {
                        foreach (var element in shaftElements)
                        {
                            ws.Cells[row, 1].Value = elementTypeEnumToString[element.Type];
                            ws.Cells[row, 2].Value = element.L;
                            ws.Cells[row, 3].Value = element.De;
                            ws.Cells[row, 4].Value = element.Di;
                            ws.Cells[row, 5].Value = element.M;
                            ws.Cells[row, 6].Value = element.Io;
                            ws.Cells[row, 7].Value = element.Id;
                            ws.Cells[row, 8].Value = element.K;
                            ws.Cells[row, 9].Value = element.Cm;
                            ws.Cells[row, 10].Value = element.Division;
                            ws.Cells[row, 11].Value = element.IdN;
                            ws.Cells[row, 12].Value = element.IdNValue;
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
            } catch { return false; }
            Debug.WriteLine("Soubor {0} byl uložen.", fileName);
            return true;
        }

        /// <summary>
        /// Uloží výsledky výpočtu do souboru .xlsx
        /// </summary>
        /// <param name="fileName">Úplná cesta k výstupnímu souboru</param>
        /// <param name="hridel">Objekt hřídele s daty</param>
        /// <returns></returns>
        public static bool SaveResults(string fileName, ShaftProperties shaftProperties, CalculationProperties calculationProperties, List<ShaftElement> shaftElements, KritikResults results)
        {
            Dictionary<BoundaryCondition, string> opVypsatDict = new()
            {
                { BoundaryCondition.free, Texts.VOLNY },
                { BoundaryCondition.joint, Texts.KLOUB },
                { BoundaryCondition.fix, Texts.VETKNUTI }
            };
            Dictionary<GyroscopicEffect, string> gyrosVypsatDict = new()
            {
                { GyroscopicEffect.none, Texts.VLIVGYROSNENIUVAZOVAN },
                { GyroscopicEffect.forward, Texts.SOUBEZNAPRECESE },
                { GyroscopicEffect.backward, Texts.PROTIBEZNAPRECESE }
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileInfo fileInfo = new FileInfo(fileName);
            try
            {
                using (ExcelPackage p = new ExcelPackage(fileInfo))
                {
                    if (p.Workbook.Worksheets[excelListVysledky] is not null)
                    {
                        p.Workbook.Worksheets.Delete(excelListVysledky);
                    }
                    // Pokud ještě nebyl proveden výpočet (-> kritikResults je null), tak to neřeším, jenom smažu list s výsledky
                    if (results == null)
                    {
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
                    ExcelWorksheet ws = p.Workbook.Worksheets.Add(excelListVysledky);

                    ws.Cells.Style.Font.Bold = false;
                    ws.Cells.Style.Font.Color.SetColor(Color.Black);
                    ws.Cells.Style.Font.Italic = false;
                    ws.Cells.Style.Font.UnderLine = false;
                    ws.Cells.Style.Font.Size = 11;
                    ws.Cells.Style.Font.Name = "Calibri";

                    ws.Cells[1, 1].Value = Texts.KritickeOtackykrouzivehoKmitani;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[3, 1].Value = Texts.NazevDT;
                    ws.Cells[3, 2].Value = calculationProperties.Title;
                    ws.Cells[4, 1].Value = Texts.PopisDT;
                    ws.Cells[4, 2].Value = calculationProperties.Description;
                    ws.Cells[5, 1].Value = Texts.ResilDT;
                    ws.Cells[5, 2].Value = calculationProperties.Author;
                    ws.Cells[6, 1].Value = Texts.DatumDT;
                    ws.Cells[6, 2].Value = calculationProperties.Date;
                    ws.Cells[8, 1].Value = Texts.OkrajovePodminkyDT;
                    ws.Cells[9, 2].Value = Texts.LEVYKonecRotoru;
                    ws.Cells[9, 4].Value = opVypsatDict[shaftProperties.BCLeft];
                    ws.Cells[9, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[10, 2].Value = Texts.PRAVYKonecRotoru;
                    ws.Cells[10, 4].Value = opVypsatDict[shaftProperties.BCRight];
                    ws.Cells[10, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[11, 1].Value = Texts.ModulPruznostiVTahuHrideleDT;
                    ws.Cells[11, 4].Value = shaftProperties.YoungModulus;
                    ws.Cells[11, 5].Value = "GPa";
                    ws.Cells[12, 1].Value = Texts.HustotaMaterialuHrideleDT;
                    ws.Cells[12, 4].Value = shaftProperties.MaterialDensity;
                    ws.Cells[12, 5].Value = "kg.m⁻³";
                    ws.Cells[14, 1].Value = gyrosVypsatDict[shaftProperties.Gyros];
                    int row = 15;
                    if (shaftProperties.OperatingSpeed > 0)
                    {
                        ws.Cells[++row, 1].Value = Texts.ProvozniOtackyHrideleDT;
                        ws.Cells[row, 4].Value = shaftProperties.OperatingSpeed;
                        ws.Cells[row, 5].Value = "min⁻¹";
                    }
                    if (shaftProperties.RunawaySpeed > 0)
                    {
                        ws.Cells[++row, 1].Value = Texts.PrubezneOtackyHrideleDT;
                        ws.Cells[row, 4].Value = shaftProperties.RunawaySpeed;
                        ws.Cells[row, 5].Value = "min⁻¹";
                    }
                    if (calculationProperties.Notes != "")
                    {
                        row = shaftProperties.OperatingSpeed > 0 || shaftProperties.RunawaySpeed > 0 ? row + 2 : row + 1;
                        ws.Cells[row, 1].Value = Texts.PoznamkyKVypoctuDT;
                        ws.Cells[++row, 2].Value = calculationProperties.Notes;
                        ws.Cells[row, 2, row, 8].Merge = true;
                        ws.Cells[row, 2].Style.WrapText = true;
                        ws.Row(row).Height = MeasureTextHeight(calculationProperties.Notes, ws.Cells[row, 1].Style.Font, ws.Column(1).Width * 7);
                    }
                    row = shaftProperties.OperatingSpeed > 0 || shaftProperties.RunawaySpeed > 0 || calculationProperties.Notes != "" ? row + 1 : row;
                    ws.Cells[++row, 1].Value = Texts.VypocteneHodnotyDT;
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    for (int i = 0; i < results.CriticalSpeeds.Length; i++)
                    {
                        ws.Cells[++row, 2].Value = Texts.OrdinalNumber(i + 1) + " " + Texts.kritickeOtacky;
                        ws.Cells[row, 4].Value = results.CriticalSpeeds[i];
                        ws.Cells[row, 5].Value = "min⁻¹";
                        ws.Cells[row, 6].Value = results.CriticalSpeeds[i] / 60.0;
                        ws.Cells[row, 6].Style.Numberformat.Format = "(0.0##)";
                        ws.Cells[row, 7].Value = "Hz";
                    }
                    if (results.CriticalSpeeds.Length == 0) { ws.Cells[++row, 2].Value = Texts.NebylyVypoctenyZadneKritickeOtacky; }

                    if ((shaftProperties.OperatingSpeed > 0 || shaftProperties.RunawaySpeed > 0) && results.CriticalSpeeds.Length > 0)
                    {
                        row += 2;
                        ws.Cells[row, 2].Value = Texts.OrdinalNumber(1) + " " + Texts.kritickeOtacky + " " + Texts.odpovidajiDT;
                    }
                    if (shaftProperties.OperatingSpeed > 0 && results.CriticalSpeeds.Length > 0)
                    {
                        ws.Cells[++row, 2].Value = results.CriticalSpeeds[0] / shaftProperties.OperatingSpeed * 100;
                        ws.Cells[row, 2].Style.Numberformat.Format = "0.0##";
                        ws.Cells[row, 3].Value = "% " + Texts.provoznichOtacek;
                    }
                    if (shaftProperties.RunawaySpeed > 0 && results.CriticalSpeeds.Length > 0)
                    {
                        ws.Cells[++row, 2].Value = results.CriticalSpeeds[0] / shaftProperties.RunawaySpeed * 100;
                        ws.Cells[row, 2].Style.Numberformat.Format = "0.0##";
                        ws.Cells[row, 3].Value = "% " + Texts.prubeznychOtacek;
                    }

                    row += 2;
                    ws.Cells[row, 1].Value = Texts.GeometrieHrideleDT;
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    ws.Cells[row, 3].Value = "(L, De, Di - [mm]; m - [kg]; Jo, Jd - [kg.m2]; k, Cm - [N/m])";

                    if (shaftElements is not null)
                    {
                        var alignLeft = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        var alignRight = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        string formatNum = "0.0#####";

                        int i = 0;
                        foreach (var element in shaftElements)
                        {
                            i++; row++;
                            ws.Cells[row, 1].Value = i;
                            ws.Cells[row, 1].Style.Numberformat.Format = "0\".\"";
                            ws.Cells[row, 2].Value = Texts.Type(element.Type); // !!! potřeba opravit pro novou verzi !!!
                            switch (element.Type)
                            {
                                case ElementType.beam:
                                    ws.Cells[row, 3].Value = "L = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.L;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 5].Value = "De = ";
                                    ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 6].Value = element.De;
                                    ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 6].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 7].Value = "Di = ";
                                    ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 8].Value = element.Di;
                                    ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 8].Style.Numberformat.Format = formatNum;
                                    break;
                                case ElementType.beamPlus:
                                    ws.Cells[row, 3].Value = "L = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.L;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 5].Value = "De = ";
                                    ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 6].Value = element.De;
                                    ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 6].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 7].Value = "Di = ";
                                    ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 8].Value = element.Di;
                                    ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 8].Style.Numberformat.Format = formatNum;
                                    ws.Cells[++row, 2].Value = Texts.HridelJeRozdelenaNa + " " + (element.Division + 1) + " " 
                                        + Texts.castiODelce + " " + string.Format("{0:#.###}", element.L / (element.Division + 1)) + " mm, " 
                                        + Texts.meziKterymiJsouUmistenyPrvkyDT;
                                    if (element.M > 0)
                                    {
                                        ws.Cells[++row, 2].Value = Texts.Type(ElementType.disc);
                                        ws.Cells[row, 3].Value = "m = ";
                                        ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 4].Value = element.M / element.Division;
                                        ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                        if (shaftProperties.Gyros != GyroscopicEffect.none)
                                        {
                                            ws.Cells[row, 5].Value = "Jo = ";
                                            ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                            ws.Cells[row, 6].Value = element.Io / element.Division;
                                            ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                            ws.Cells[row, 7].Value = "Jd = ";
                                            ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                            ws.Cells[row, 8].Value = element.IdN == 0 ? element.Id / element.Division * element.IdNValue : element.IdNValue;
                                            ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                        }
                                    }
                                    if (element.K > 0)
                                    {
                                        ws.Cells[++row, 2].Value = Texts.Type(ElementType.support);
                                        ws.Cells[row, 3].Value = "k = ";
                                        ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 4].Value = element.K / element.Division;
                                        ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    }
                                    if (element.Cm > 0)
                                    {
                                        ws.Cells[++row, 2].Value = Texts.Type(ElementType.magnet);
                                        ws.Cells[row, 3].Value = "Cm = ";
                                        ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 4].Value = element.Cm / element.Division;
                                        ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    }
                                    break;
                                case ElementType.rigid:
                                    ws.Cells[row, 3].Value = "L = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.L;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                    break;
                                case ElementType.disc:
                                    ws.Cells[row, 3].Value = "m = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.M;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    if (shaftProperties.Gyros != GyroscopicEffect.none)
                                    {
                                        ws.Cells[row, 5].Value = "Jo = ";
                                        ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 6].Value = element.Io;
                                        ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                        ws.Cells[row, 7].Value = "Jd = ";
                                        ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 8].Value = element.Id;
                                        ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                    }
                                    break;
                                case ElementType.support:
                                    ws.Cells[row, 3].Value = "k = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.K;
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    break;
                                case ElementType.magnet:
                                    ws.Cells[row, 3].Value = "Cm = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.Cm;
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
            } catch { return false; }
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
