using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Kritik
{
    public static class GetOnlyNumbers
    {
        private static char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', 'e', 'E' };
        public static string GON(string input)
        {

            return new string(input.Where(c => chars.Contains(c)).ToArray());
        }
    }
    public class NotesTextBoxHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value - 376;
            if (v < 0)
            {
                return 0;
            }
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FormatEConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)value == 0) { return "0"; }
            else { return String.Format("{0:#.######e0}", value); }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class FormatDefaultConverter : IValueConverter
    {
        /// <summary>
        /// Converts double value into string with max. 12 decimal places
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">(string) order of the unit of the number to be shown in View (eg. GPa -> '9')</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not null)
            {
                int exponent;
                if (Int32.TryParse(parameter as string, out exponent))
                {
                    double val = (double)value * Math.Pow(10, -1 * exponent);
                    if (val == 0)
                        return "0";
                    return String.Format("{0:#.############}", val);
                }
            }

            return value;
        }
        /// <summary>
        /// Converts string representation of number, replaces ',' with '.' and deletes '-' -> stores absolute value of number
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">(string) order of the unit of the number which is shown in View (eg. GPa -> '9')</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string v = (string)value;
            v = v.Replace(",", ".");
            v = v.TrimStart('-');
            v = GetOnlyNumbers.GON(v);

            if (parameter is not null)
            {
                double val;
                int exponent;
                if (Double.TryParse(v, out val) && Int32.TryParse(parameter as string, out exponent))
                    return val * Math.Pow(10, exponent);
            }

            return v;
        }
    }
    /// <summary>
    /// Converts string representation of number to integer, replaces ',' with '.' and deletes '-' -> stores absolute value of number
    /// </summary>
    public class FormatToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string v = (string)value;
            v = v.Replace(",", ".");
            v = v.TrimStart('-');
            v = GetOnlyNumbers.GON(v);
            try { v = Math.Round(System.Convert.ToDouble(v)).ToString(); }
            catch { return ""; }
            return v;
        }
    }
    public class FormatSpacesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)value == 0) { return "0"; }
            string s = String.Format("{0:# ### ###.######}", value);
            s = s.TrimStart();
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    /// <summary>
    /// Converts Enum to string and vice versa using Enum Description
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Enum)value).GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Array enumValues = Enum.GetValues(targetType);
            foreach (Enum enumValue in enumValues)
            {
                if (enumValue.GetDescription() == (string)value)
                    return enumValue;
            }
            return null;
        }
    }

    /// <summary>
    /// Converts Enum to string and vice versa using current application resource dictionary
    /// </summary>
    public class EnumToStringByResourceDictionaryConverter : IValueConverter
    {
        private ResourceDictionary resourceDictionary = Application.Current.Resources.MergedDictionaries[^1];
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Enum)value).GetNameUsingResourceDictionary(resourceDictionary);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Array enumValues = Enum.GetValues(targetType);
            foreach (Enum enumValue in enumValues)
            {
                if (enumValue.GetNameUsingResourceDictionary(resourceDictionary) == (string)value)
                    return enumValue;
            }
            return null;
        }
    }

    /// <summary>
    /// Converts Enum to bool and vice versa based on number set in parameter
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool parsed = Int32.TryParse(parameter as string, out int index);
            if (!parsed)
                throw new ArgumentException("Parameter could not be converted to Int32", nameof(parameter));

            string[] enumNames = value.GetType().GetEnumNames();

            return enumNames[index] == value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
                return Binding.DoNothing;

            bool parsed = Int32.TryParse(parameter as string, out int index);
            if (!parsed)
                throw new ArgumentException("Parameter could not be converted to Int32", nameof(parameter));

            var enumValues = Type.GetType(targetType.FullName).GetEnumValues();

            return enumValues.GetValue(index);
        }
    }

    public class NewDataGridItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(ShaftElementForDataGrid) && value?.GetType().Name == "NamedObject")
                return new ShaftElementForDataGrid();
            return value;
        }
    }
}
