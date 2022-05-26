using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kritik
{
    public static class BinsTeoremer
    {
        private static string[] setninger = new string[] {
            "\u0056\u0161\u0065\u0063\u0068\u006E\u006F \u006A\u0065 \u0076 \u0070\u0072\u0064\u0065\u006C\u0069\u002E",
            "\u0054\u0061\u0064\u0079 \u0074\u006F \u006E\u0069\u006B\u0064\u006F \u006E\u0065\u0159\u00ED\u0064\u00ED\u002E",
            "\u0054\u006F \u006A\u0065 \u0076 \u0070\u0072\u0064\u0065\u006C\u0069, \u0068\u006F\u0161\u0069, \u0074\u0061\u006B\u006F\u0076\u00E1 \u0061\u006B\u0063\u0065 \u0074\u0061\u0064\u0079 \u006A\u0065\u0161\u0074\u011B \u006E\u0065\u0062\u0079\u006C\u0061\u002E",
            "\u0044\u006F\u0075\u0066\u00E1\u006D, \u017E\u0065 \u0074\u0075 \u0061\u006B\u0063\u0069 \u006E\u0065\u0064\u006F\u0073\u0074\u0061\u006E\u0065\u006D\u0065\u002E",
            "\u0055\u017E \u0061\u0062\u0079\u0063\u0068 \u0062\u0079\u006C \u0076 \u0064\u016F\u0063\u0068\u006F\u0064\u0075\u002E"
        };

        private static string[] setningerUtviklet = new string[] {
            "\u0056\u0161\u0065\u0063\u0068\u006E\u006F \u006A\u0065 \u00FA\u0070\u006C\u006E\u011B \u0076 \u0070\u0072\u0064\u0065\u006C\u0069\u002E",
            "\u0054\u0061\u0064\u0079 \u0074\u006F \u006E\u0069\u006B\u0064\u006F \u006E\u0065\u0159\u00ED\u0064\u00ED, \u0074\u0069 \u0161\u00E9\u0066\u006F\u0076\u00E9 \u0073\u0074\u006F\u006A\u00ED \u00FA\u0070\u006C\u006E\u011B \u007A\u0061 \u0068\u006F\u0076\u006E\u006F\u002E \u0054\u006F \u006A\u0065 \u00FA\u0070\u006C\u006E\u00E1 \u0074\u0072\u0061\u0067\u00E9\u0064\u0069\u0065 \u0074\u0061\u0064\u0079\u002E",
            "\u0044\u006F\u0075\u0066\u00E1\u006D, \u017E\u0065 \u0074\u0075 \u0061\u006B\u0063\u0069 \u006E\u0065\u0064\u006F\u0073\u0074\u0061\u006E\u0065\u006D\u0065\u002E \u0054\u006F \u0076\u016F\u0062\u0065\u0063 \u006E\u0065\u006D\u00E1\u0074\u0065 \u0062\u0072\u00E1\u0074, \u0074\u0061\u006B\u006F\u0076\u00FD \u0061\u006B\u0063\u0065\u002E \u0054\u006F \u006D\u00E1\u0074\u0065 \u006F\u0064\u006D\u00ED\u0074\u0061\u0074\u0021",
            "\u0055\u017E \u0061\u0062\u0079\u0063\u0068 \u0062\u0079\u006C \u0076 \u0064\u016F\u0063\u0068\u006F\u0064\u0075\u002E \u0054\u006F \u0075\u017E \u006E\u0065\u006E\u00ED \u0070\u0072\u006F \u006D\u011B\u002E"
        };

        private static readonly Random random = new Random();

        private static (string tittel, string setning) TilfeldigSetning()
        {
            bool utviklet = Convert.ToBoolean(random.Next(0, 2));
            string[] utvalgteSetninger = setninger;
            string tittel = "\u0042\u00ED\u006E\u006F\u0076\u0061\u0020\u0076\u011B\u0074\u0061\u003A";

            if (utviklet)
            {
                utvalgteSetninger = setningerUtviklet;
                tittel = "\u0042\u00ED\u006E\u006F\u0076\u0061\u0020\u0076\u011B\u0074\u0061\u0020\u0072\u006F\u007A\u0076\u0069\u0074\u00E1\u003A";
            }

            int id = random.Next(0, utvalgteSetninger.Length);
            tittel = (id + 1) + ". " + tittel;

            return (tittel, utvalgteSetninger[id]);
        }

        public static void VisSetningen()
        {
            (string tittel, string text) = TilfeldigSetning();
            MessageBox.Show(text, tittel, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
