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
    public class NastaveniVyskyTextBoxuPoznamky : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value - 368;
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
