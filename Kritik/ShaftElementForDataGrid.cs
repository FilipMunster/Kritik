using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Kritik
{
    /// <summary>
    /// Shaft element for use in DataGrid
    /// </summary>
    public class ShaftElementForDataGrid : ShaftElement, INotifyPropertyChanged, ICloneable
    {
        private static readonly string[] isEditableNames = {
            nameof(IsEditableL),
            nameof(IsEditableDe),
            nameof(IsEditableDi),
            nameof(IsEditableM),
            nameof(IsEditableIo),
            nameof(IsEditableId),
            nameof(IsEditableK),
            nameof(IsEditableCm)};

        /// <summary>
        /// List of names of <see cref="ElementType"/>
        /// </summary>
        public static string[] ElementTypesItems => Enums.GetNames<ElementType>();

        /// <summary>
        /// Create new <see cref="ShaftElementForDataGrid"/> with <see cref="ElementType.beam"/> 
        /// </summary>
        /// <param name="elementType"></param>
        public ShaftElementForDataGrid() : base(ElementType.beam)
        {
            Type = ElementType.beam;
        }
        /// <summary>
        /// Create new <see cref="ShaftElementForDataGrid"/> with desired <see cref="ElementType"/> 
        /// </summary>
        /// <param name="elementType"></param>
        public ShaftElementForDataGrid(ElementType elementType) : base(elementType)
        {
            Type = elementType;
        }
        /// <summary>
        /// Create <see cref="ShaftElementForDataGrid"/> from <see cref="ShaftElement"/>
        /// </summary>
        /// <param name="shaftElement">Source ShaftElement object</param>
        public ShaftElementForDataGrid(ShaftElement shaftElement) : base(shaftElement.Type)
        {
            PropertyInfo[] parentProperties = typeof(ShaftElement).GetProperties();
            PropertyInfo[] thisProperties = this.GetType().GetProperties();
            foreach (var parentProperty in parentProperties)
            {
                foreach (var thisProperty in thisProperties)
                {
                    if (thisProperty.Name == parentProperty.Name && thisProperty.PropertyType == parentProperty.PropertyType)
                    {
                        thisProperty.SetValue(this, parentProperty.GetValue(shaftElement));
                        break;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ElementType type;
        public new ElementType Type
        {
            get => type;
            set
            {
                type = value;
                base.Type = value;
                foreach (string property in isEditableNames)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
                }
            }
        }
        private int division;
        public new int Division
        {
            get => division > 0 ? division : 1;
            set
            {
                if (value < 1)
                {
                    MessageBox.Show("Hodnota dělení nesmí být menší než 1.", "Špatně zadaná hodnota",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                division = value;
                base.Division = value;
            }
        }

        public bool IsEditableL => Type is ElementType.beam or ElementType.beamPlus or ElementType.rigid;
        public bool IsEditableDe => Type is ElementType.beam or ElementType.beamPlus;
        public bool IsEditableDi => Type is ElementType.beam or ElementType.beamPlus;
        public bool IsEditableM => Type is ElementType.disc or ElementType.beamPlus;
        public bool IsEditableIo => Type is ElementType.disc or ElementType.beamPlus;
        public bool IsEditableId => Type is ElementType.disc or ElementType.beamPlus;
        public bool IsEditableK => Type is ElementType.support or ElementType.beamPlus;
        public bool IsEditableCm => Type is ElementType.magnet or ElementType.beamPlus;

        /// <summary>
        /// Array of IsEditables in order as they are in the table
        /// </summary>
        public bool[] IsEditableArray => new bool[] { true, IsEditableL, IsEditableDe, IsEditableDi, IsEditableM, IsEditableIo, IsEditableId, IsEditableK, IsEditableCm };

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
