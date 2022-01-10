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
    public static class VratCisla
    {
        private static char[] znaky = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', 'e', 'E' };
        public static string VC(string input)
        {
            
            return new string(input.Where(c => znaky.Contains(c)).ToArray());
        }
    }
    public class NastaveniVyskyTextBoxuPoznamky : IValueConverter
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
            // Volá se při čtení
            if ((double)value == 0) { return "0"; }
            else { return String.Format("{0:#.######e0}", value); }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Volá se při zapisování
            return value;
        }
    }

    public class FormatZadnyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Volá se při čtení
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Volá se při zapisování
            string v = (string)value;
            v = v.Replace(",", ".");
            v = v.TrimStart('-');
            v = VratCisla.VC(v);
            return v;
        }
    }

    public class FormatToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Volá se při čtení
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Volá se při zapisování
            string v = (string)value;
            v = v.Replace(",", ".");
            v = v.TrimStart('-');
            v = VratCisla.VC(v);
            try { v = Math.Round(System.Convert.ToDouble(v)).ToString(); }
            catch { return ""; }
            return v;
        }
    }
    public class FormatMezeryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Volá se při čtení
            if ((double)value == 0) { return "0"; }
            string s = String.Format("{0:# ### ###.######}", value);
            s = s.TrimStart();
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Volá se při zapisování
            return value;
        }
    }

}
