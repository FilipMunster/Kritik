using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return v;
        }
    }

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

}
