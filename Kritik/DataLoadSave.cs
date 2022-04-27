using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Kritik
{
    /// <summary>
    /// Provides methods for loading and saving Kritik data from / to .xlsx file
    /// </summary>
    public static class DataLoadSave
    {
        // keywords used in input file
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
        private const string vlivOtacekKeyword = "vliv";
        private const string otackyHrideleKeyword = "ni";
        private const string nKritMaxKeyword = "nKritMax";
        private const string poznamkaKeyword = "Poznamka";
        private const string diskKeyword = "disk";
        private const string rigidKeyword = "tuhy";
        private const string beamKeyword = "hridel";
        private const string beamPlusKeyword = "hridel+";
        private const string springKeyword = "pruzina";
        private const string magnetKeyword = "magnet";
        private const int youngModulusOrder = 9; // Young Modulus is given in GPa
        private const int lengthOrder = -3; // Lengths are given in mm
        private const string excelSheetZadani = "KRITIK_zadani";
        private const string excelSheetResultsName = "KRITIK_vypocet";

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

        public struct LoadResult
        {
            public ShaftProperties ShaftProperties { get; set; }
            public CalculationProperties CalculationProperties { get; set; }
            public List<ShaftElement> ShaftElements { get; set; }
        }


        /// <summary>
        /// Loads the input data from xlsx file
        /// </summary>
        /// <param name="fileName">Full path to source file</param>
        /// <returns>Loaded data</returns>
        public static LoadResult? Load(string fileName)
        {
            ShaftProperties shaftProperties = new();
            CalculationProperties calculationProperties = new();
            List<ShaftElement> shaftElements = new();

            // Loads .xlsx file into excel variable
            FileInfo fileInfo = new FileInfo(fileName);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet excel;
                    excel = package.Workbook.Worksheets[excelSheetZadani];

                    // if WorskSheet named excelSheetZadani was not found, try to load the FirstOrDefault()
                    if (excel == null) { excel = package.Workbook.Worksheets.FirstOrDefault(); }

                    int numRows; // number of rows in excel sheet

                    if ((excel != null) && (excel.Dimension != null))
                    {
                        numRows = excel.Dimension.Rows;
                    }
                    else
                    {
                        Debug.WriteLine("Chyba: Nepodařilo se načíst soubor se vstupními daty.");
                        return null;
                    }

                    // Loading of the input values:
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
                                    shaftProperties.BCRight = bCRight;
                                    break;
                                case gyrosKeyword:
                                    if (!gyroscopicEffectStringToEnum.TryGetValue(value, out GyroscopicEffect gyros))
                                        gyros = GyroscopicEffect.none;
                                    shaftProperties.Gyros = gyros;
                                    break;
                                case modulPruznostiKeyword:
                                    if (Double.TryParse(value, out double e))
                                        shaftProperties.YoungModulus = e * Math.Pow(10, youngModulusOrder);
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
                                case vlivOtacekKeyword:
                                    if (Boolean.TryParse(value, out bool nInfluence))
                                        shaftProperties.ShaftRotationInfluence = nInfluence;
                                    break;
                                case otackyHrideleKeyword:
                                    if (Double.TryParse(value, out double ni))
                                        shaftProperties.ShaftRPM = ni;
                                    break;
                                case nKritMaxKeyword:
                                    if (Double.TryParse(value, out double nMax))
                                        shaftProperties.MaxCriticalSpeed = nMax;
                                    break;
                                case poznamkaKeyword:
                                    calculationProperties.Notes = value;
                                    break;
                                default:
                                    if (excel.Cells[i, 1].Value.ToString() == colNames[0])
                                    {
                                        for (int j = 0; j < colNames.Length; j++) // checks if all column titles are correct
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

                    // Creating ShaftElements collection:
                    if (shaftElementsFirstRow > 0)
                    {
                        for (int i = shaftElementsFirstRow; i <= numRows; i++)
                        {
                            if (!elementTypeStringToEnum.TryGetValue(excel.Cells[i, 1].Value.ToString(), out ElementType type))
                                return null;

                            int division = Convert.ToInt32(excel.Cells[i, 10].Value);

                            shaftElements.Add(new ShaftElement(type)
                            {
                                L = Convert.ToDouble(excel.Cells[i, 2].Value) * Math.Pow(10, lengthOrder),
                                De = Convert.ToDouble(excel.Cells[i, 3].Value) * Math.Pow(10, lengthOrder),
                                Di = Convert.ToDouble(excel.Cells[i, 4].Value) * Math.Pow(10, lengthOrder),
                                M = Convert.ToDouble(excel.Cells[i, 5].Value),
                                Io = Convert.ToDouble(excel.Cells[i, 6].Value),
                                Id = Convert.ToDouble(excel.Cells[i, 7].Value),
                                K = Convert.ToDouble(excel.Cells[i, 8].Value),
                                Cm = Convert.ToDouble(excel.Cells[i, 9].Value),
                                Division = division > 0 ? division : 1,
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

                    LoadResult loadResult = new LoadResult();
                    loadResult.ShaftProperties = shaftProperties;
                    loadResult.CalculationProperties = calculationProperties;
                    loadResult.ShaftElements = shaftElements;
                    return loadResult;
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// Saves shaft and calculation data into xlsx file
        /// </summary>
        /// <param name="fileName">Full path to output file</param>
        /// <param name="shaft">Shaft</param>
        /// <param name="calculationProperties">Calculation properties</param>
        /// <returns>true if file saved successfully</returns>
        public static bool SaveData(string fileName, Shaft shaft, CalculationProperties calculationProperties)
        {
            ShaftProperties shaftProperties = shaft.Properties;
            List<ShaftElement> shaftElements = new List<ShaftElement>(shaft.Elements);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileInfo fileInfo = new FileInfo(fileName);
            try
            {
                using (ExcelPackage p = new ExcelPackage(fileInfo))
                {
                    if (p.Workbook.Worksheets[excelSheetZadani] is not null)
                    {
                        p.Workbook.Worksheets.Delete(excelSheetZadani);
                    }
                    ExcelWorksheet ws = p.Workbook.Worksheets.Add(excelSheetZadani);

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
                    ws.Cells[16, 2].Value = shaftProperties.YoungModulus * Math.Pow(10, -1 * youngModulusOrder);
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
                    ws.Cells[22, 1].Value = vlivOtacekKeyword;
                    ws.Cells[22, 2].Value = shaftProperties.ShaftRotationInfluence.ToString();
                    ws.Cells[23, 1].Value = otackyHrideleKeyword;
                    ws.Cells[23, 2].Value = shaftProperties.ShaftRPM;
                    ws.Cells[23, 3].Value = "rpm";
                    ws.Cells[24, 1].Value = nKritMaxKeyword;
                    ws.Cells[24, 2].Value = shaftProperties.MaxCriticalSpeed;
                    ws.Cells[24, 3].Value = "rpm";
                    ws.Cells[26, 1].Value = "Poznámky k výpočtu:";
                    ws.Cells[27, 1].Value = poznamkaKeyword;
                    ws.Cells[27, 2].Value = calculationProperties.Notes;
                    ws.Cells[29, 1].Value = "Data článků:";
                    ws.Cells[29, 3].Value = "(L, De, Di - [mm]; m - [kg]; Io, Id - [kg.m2]; k, Cm - [N/m]; Deleni - [-])";
                    for (int i = 0; i < colNames.Length; i++)
                    {
                        ws.Cells[30, i + 1].Value = colNames[i];
                    }

                    int row = 31; // první řádek dat prvků
                    if (shaftElements != null)
                    {
                        foreach (var element in shaftElements)
                        {
                            ws.Cells[row, 1].Value = elementTypeEnumToString[element.Type];
                            ws.Cells[row, 2].Value = element.L * Math.Pow(10, -1 * lengthOrder);
                            ws.Cells[row, 3].Value = element.De * Math.Pow(10, -1 * lengthOrder);
                            ws.Cells[row, 4].Value = element.Di * Math.Pow(10, -1 * lengthOrder);
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
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Saves results of critical speed calculation
        /// </summary>
        /// <param name="fileName">Full path to output file</param>
        /// <param name="calculation">KritikCalculation with results</param>
        /// <param name="strings">Instance of Strings class</param>
        /// <returns></returns>
        public static bool SaveResults(string fileName, KritikCalculation calculation, Strings strings)
        {
            if (!calculation.IsReady)
                calculation = new KritikCalculation(new Shaft(), new CalculationProperties());

            ShaftProperties shaftProperties = calculation.Shaft.Properties;
            CalculationProperties calculationProperties = calculation.CalculationProperties;
            List<ShaftElement> shaftElements = new List<ShaftElement>(calculation.Shaft.Elements);
            double[] criticalSpeeds = calculation.CriticalSpeeds;

            Dictionary<BoundaryCondition, string> bcToStringDict = new()
            {
                { BoundaryCondition.free, strings.VOLNY },
                { BoundaryCondition.joint, strings.KLOUB },
                { BoundaryCondition.fix, strings.VETKNUTI }
            };
            Dictionary<GyroscopicEffect, string> gyrosToStringDict = new()
            {
                { GyroscopicEffect.none, strings.VLIVGYROSNENIUVAZOVAN },
                { GyroscopicEffect.forward, strings.SOUBEZNAPRECESE },
                { GyroscopicEffect.backward, strings.PROTIBEZNAPRECESE }
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileInfo fileInfo = new FileInfo(fileName);

            try
            {
                using (ExcelPackage p = new ExcelPackage(fileInfo))
                {
                    if (p.Workbook.Worksheets[excelSheetResultsName] is not null)
                    {
                        p.Workbook.Worksheets.Delete(excelSheetResultsName);
                    }
                    ExcelWorksheet ws = p.Workbook.Worksheets.Add(excelSheetResultsName);

                    ws.Cells.Style.Font.Bold = false;
                    ws.Cells.Style.Font.Color.SetColor(Color.Black);
                    ws.Cells.Style.Font.Italic = false;
                    ws.Cells.Style.Font.UnderLine = false;
                    ws.Cells.Style.Font.Size = 11;
                    ws.Cells.Style.Font.Name = "Calibri";

                    ws.Cells[1, 1].Value = strings.KritickeOtackykrouzivehoKmitani;
                    ws.Cells[1, 1].Style.Font.Bold = true;

                    // If calculation has not been done yet (<- criticalSpeeds == null):
                    if (criticalSpeeds is null)
                    {
                        ws.Cells[2, 1].Value = strings.VypocetNebylDosudProveden;
                        try
                        {
                            p.Save();
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    ws.Cells[3, 1].Value = strings.NazevDT;
                    ws.Cells[3, 2].Value = calculationProperties.Title;
                    ws.Cells[4, 1].Value = strings.PopisDT;
                    ws.Cells[4, 2].Value = calculationProperties.Description;
                    ws.Cells[5, 1].Value = strings.ResilDT;
                    ws.Cells[5, 2].Value = calculationProperties.Author;
                    ws.Cells[6, 1].Value = strings.DatumDT;
                    ws.Cells[6, 2].Value = calculationProperties.Date;
                    ws.Cells[8, 1].Value = strings.OkrajovePodminkyDT;
                    ws.Cells[9, 2].Value = strings.LEVYKonecRotoru;
                    ws.Cells[9, 4].Value = bcToStringDict[shaftProperties.BCLeft];
                    ws.Cells[9, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[10, 2].Value = strings.PRAVYKonecRotoru;
                    ws.Cells[10, 4].Value = bcToStringDict[shaftProperties.BCRight];
                    ws.Cells[10, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[11, 1].Value = strings.ModulPruznostiVTahuHrideleDT;
                    ws.Cells[11, 4].Value = shaftProperties.YoungModulus * Math.Pow(10, -1 * youngModulusOrder);
                    ws.Cells[11, 5].Value = "GPa";
                    ws.Cells[12, 1].Value = strings.HustotaMaterialuHrideleDT;
                    ws.Cells[12, 4].Value = shaftProperties.MaterialDensity;
                    ws.Cells[12, 5].Value = "kg.m⁻³";
                    ws.Cells[14, 1].Value = gyrosToStringDict[shaftProperties.Gyros];

                    if (shaftProperties.ShaftRotationInfluence)
                    {
                        ws.Cells[14, 1].Value = ws.Cells[14, 1].Value + " " + strings.PROZADANEOTACKY;
                        int niCol = 5;
                        if (strings.SelectedLanguage == Strings.Language.en) niCol = 6;
                        ws.Cells[14, niCol].Value = shaftProperties.ShaftRPM;
                        ws.Cells[14, niCol + 1].Value = "min⁻¹";
                    }

                    int row = 15;
                    if (shaftProperties.OperatingSpeed > 0)
                    {
                        ws.Cells[++row, 1].Value = strings.ProvozniOtackyHrideleDT;
                        ws.Cells[row, 4].Value = shaftProperties.OperatingSpeed;
                        ws.Cells[row, 5].Value = "min⁻¹";
                    }
                    if (shaftProperties.RunawaySpeed > 0)
                    {
                        ws.Cells[++row, 1].Value = strings.PrubezneOtackyHrideleDT;
                        ws.Cells[row, 4].Value = shaftProperties.RunawaySpeed;
                        ws.Cells[row, 5].Value = "min⁻¹";
                    }
                    if (calculationProperties.Notes != "")
                    {
                        row = shaftProperties.OperatingSpeed > 0 || shaftProperties.RunawaySpeed > 0 ? row + 2 : row + 1;
                        ws.Cells[row, 1].Value = strings.PoznamkyKVypoctuDT;
                        ws.Cells[++row, 2].Value = calculationProperties.Notes;
                        ws.Cells[row, 2, row, 8].Merge = true;
                        ws.Cells[row, 2].Style.WrapText = true;
                        ws.Row(row).Height = MeasureTextHeight(calculationProperties.Notes, ws.Cells[row, 1].Style.Font, ws.Column(1).Width * 7);
                    }
                    row = shaftProperties.OperatingSpeed > 0 || shaftProperties.RunawaySpeed > 0 || calculationProperties.Notes != "" ? row + 1 : row;
                    ws.Cells[++row, 1].Value = strings.VypocteneHodnotyDT;
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    for (int i = 0; i < criticalSpeeds.Length; i++)
                    {
                        ws.Cells[++row, 2].Value = strings.OrdinalNumber(i + 1) + " " + strings.kritickeOtacky;
                        ws.Cells[row, 4].Value = criticalSpeeds[i];
                        ws.Cells[row, 5].Value = "min⁻¹";
                        ws.Cells[row, 6].Value = criticalSpeeds[i] / 60.0;
                        ws.Cells[row, 6].Style.Numberformat.Format = "(0.0##)";
                        ws.Cells[row, 7].Value = "Hz";
                    }
                    if (criticalSpeeds.Length == 0) { ws.Cells[++row, 2].Value = strings.NebylyVypoctenyZadneKritickeOtacky; }

                    if ((shaftProperties.OperatingSpeed > 0 || shaftProperties.RunawaySpeed > 0) && criticalSpeeds.Length > 0)
                    {
                        row += 2;
                        ws.Cells[row, 2].Value = strings.OrdinalNumber(1) + " " + strings.kritickeOtacky + " " + strings.odpovidajiDT;
                    }
                    if (shaftProperties.OperatingSpeed > 0 && criticalSpeeds.Length > 0)
                    {
                        ws.Cells[++row, 2].Value = criticalSpeeds[0] / shaftProperties.OperatingSpeed * 100;
                        ws.Cells[row, 2].Style.Numberformat.Format = "0.0##";
                        ws.Cells[row, 3].Value = "% " + strings.provoznichOtacek;
                    }
                    if (shaftProperties.RunawaySpeed > 0 && criticalSpeeds.Length > 0)
                    {
                        ws.Cells[++row, 2].Value = criticalSpeeds[0] / shaftProperties.RunawaySpeed * 100;
                        ws.Cells[row, 2].Style.Numberformat.Format = "0.0##";
                        ws.Cells[row, 3].Value = "% " + strings.prubeznychOtacek;
                    }

                    row += 2;
                    ws.Cells[row, 1].Value = strings.GeometrieHrideleDT;
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    ws.Cells[row, 3].Value = "(L, De, Di - [mm]; m - [kg]; Jo, Jd - [kg.m2]; k, Cm - [N/m])";

                    if (shaftElements is not null)
                    {
                        var alignLeft = ExcelHorizontalAlignment.Left;
                        var alignRight = ExcelHorizontalAlignment.Right;
                        string formatNum = "0.0#####";

                        int i = 0;
                        foreach (var element in shaftElements)
                        {
                            i++; row++;
                            ws.Cells[row, 1].Value = i;
                            ws.Cells[row, 1].Style.Numberformat.Format = "0\".\"";
                            ws.Cells[row, 2].Value = strings.Type(element.Type);
                            switch (element.Type)
                            {
                                case ElementType.beam:
                                    ws.Cells[row, 3].Value = "L = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.L * Math.Pow(10, -1 * lengthOrder);
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 5].Value = "De = ";
                                    ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 6].Value = element.De * Math.Pow(10, -1 * lengthOrder);
                                    ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 6].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 7].Value = "Di = ";
                                    ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 8].Value = element.Di * Math.Pow(10, -1 * lengthOrder);
                                    ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 8].Style.Numberformat.Format = formatNum;
                                    break;
                                case ElementType.beamPlus:
                                    ws.Cells[row, 3].Value = "L = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.L * Math.Pow(10, -1 * lengthOrder);
                                    ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 4].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 5].Value = "De = ";
                                    ws.Cells[row, 5].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 6].Value = element.De * Math.Pow(10, -1 * lengthOrder);
                                    ws.Cells[row, 6].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 6].Style.Numberformat.Format = formatNum;
                                    ws.Cells[row, 7].Value = "Di = ";
                                    ws.Cells[row, 7].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 8].Value = element.Di * Math.Pow(10, -1 * lengthOrder);
                                    ws.Cells[row, 8].Style.HorizontalAlignment = alignLeft;
                                    ws.Cells[row, 8].Style.Numberformat.Format = formatNum;
                                    ws.Cells[++row, 2].Value = strings.HridelJeRozdelenaNa + " " + (element.Division + 1) + " "
                                        + strings.castiODelce + " " + String.Format("{0:#.###}", element.L * Math.Pow(10, -1 * lengthOrder) / (element.Division + 1)) + " mm, "
                                        + strings.meziKterymiJsouUmistenyPrvkyDT;
                                    if (element.M > 0)
                                    {
                                        ws.Cells[++row, 2].Value = strings.Type(ElementType.disc);
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
                                        ws.Cells[++row, 2].Value = strings.Type(ElementType.support);
                                        ws.Cells[row, 3].Value = "k = ";
                                        ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 4].Value = element.K / element.Division;
                                        ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    }
                                    if (element.Cm > 0)
                                    {
                                        ws.Cells[++row, 2].Value = strings.Type(ElementType.magnet);
                                        ws.Cells[row, 3].Value = "Cm = ";
                                        ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                        ws.Cells[row, 4].Value = element.Cm / element.Division;
                                        ws.Cells[row, 4].Style.HorizontalAlignment = alignLeft;
                                    }
                                    break;
                                case ElementType.rigid:
                                    ws.Cells[row, 3].Value = "L = ";
                                    ws.Cells[row, 3].Style.HorizontalAlignment = alignRight;
                                    ws.Cells[row, 4].Value = element.L * Math.Pow(10, -1 * lengthOrder);
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
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes aproximate cell height for given text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private static double MeasureTextHeight(string text, ExcelFont font, double width)
        {
            if (text is null or "") return 0.0;

            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            int dpi = (int)g.DpiX;

            Bitmap bitmap = new Bitmap(1, 1);
            Graphics graphics = Graphics.FromImage(bitmap);

            int pixelWidth = Convert.ToInt32(width * 7);  //7 pixels per excel column width
            Single fontSize = font.Size * 1.01f;
            Font drawingFont = new Font(font.Name, fontSize);
            SizeF size = graphics.MeasureString(text, drawingFont, pixelWidth, new StringFormat { FormatFlags = StringFormatFlags.MeasureTrailingSpaces });

            g.Dispose();
            bitmap.Dispose();
            graphics.Dispose();

            //72 DPI and dpi (default 96) points per inch.  Excel height in points with max of 409 per Excel requirements.
            double result = Math.Min(Convert.ToDouble(size.Height) * 72 / dpi, 409);
            return result;
        }
    }
}
