﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    /// <summary>
    /// Prvek hřídele s vlastnostmi pro použití v DataGridu
    /// </summary>
    internal class ShaftElementForDataGrid : ShaftElement
    {
        /// <summary>
        /// Text typu prvku tak, jak je zobrazen v tabulce
        /// </summary>
        public string TypZobrazeny
        {
            get => ElementTypeToName[Type];
            set => Type = ElementNameToType[value];
        }
        /// <summary>
        /// Zda je editovatelná buňka na základě typu prvku
        /// </summary>
        public bool IsEditableL => Type is ElementType.beam or ElementType.beamPlus or ElementType.rigid;
        public bool IsEditableDe => Type is ElementType.beam or ElementType.beamPlus;
        public bool IsEditableDi => Type is ElementType.beam or ElementType.beamPlus;
        public bool IsEditableM => Type is ElementType.disc or ElementType.beamPlus;
        public bool IsEditableIo => Type is ElementType.disc or ElementType.beamPlus;
        public bool IsEditableId => Type is ElementType.disc or ElementType.beamPlus;
        public bool IsEditableK => Type is ElementType.support or ElementType.beamPlus;
        public bool IsEditableCm => Type is ElementType.magnet or ElementType.beamPlus;
        /// <summary>
        /// Pole IsEditable v pořadí, jako jsou sloupce v tabulce
        /// </summary>
        public bool[] IsEditableArray
        {
            get
            {
                bool[] b = { true, IsEditableL, IsEditableDe, IsEditableDi, IsEditableM, IsEditableIo, IsEditableId, IsEditableK, IsEditableCm };
                return b;
            }
        }
        private readonly Dictionary<ElementType, string> ElementTypeToName = new Dictionary<ElementType, string>
        {
            {ElementType.beam, "Hřídel" },
            {ElementType.beamPlus, "Hřídel+" },
            {ElementType.rigid, "Tuhý" },
            {ElementType.disc, "Disk" },
            {ElementType.support, "Podpora" },
            {ElementType.magnet, "Magnet. tah" }
        };
        private readonly Dictionary<string, ElementType> ElementNameToType = new Dictionary<string, ElementType>
        {
            {"Hřídel", ElementType.beam },
            {"Hřídel+", ElementType.beamPlus },
            {"Tuhý", ElementType.rigid },
            {"Disk", ElementType.disc },
            {"Podpora", ElementType.support },
            {"Magnet. tah", ElementType.magnet }
        };
    }
}
