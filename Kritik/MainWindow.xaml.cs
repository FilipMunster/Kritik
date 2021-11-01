using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kritik
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Globální instance třídy Hridel
        /// </summary>
        public static readonly Hridel hridel = new Hridel();
        public MainWindow()
        {
            InitializeComponent();

            DataContext = hridel;

            //TestovaciFunkce();

        }

        private void TestovaciFunkce()
        {
            string vstupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test.xlsx";
            bool nacteno = hridel.NacistData(vstupniSoubor);

            if (nacteno)
            {
                // Vypsání zadaných dat do konzole:
                Console.WriteLine(hridel.VypocetNazev);
                Console.WriteLine(hridel.VypocetPopis);
                Console.WriteLine(hridel.VypocetResil);
                Console.WriteLine(hridel.VypocetDatum);
                Console.WriteLine(hridel.OpLeva);
                Console.WriteLine(hridel.OpPrava);
                Console.WriteLine(hridel.Gyros);
                Console.WriteLine(hridel.ModulPruznosti);
                Console.WriteLine(hridel.Rho);
                Console.WriteLine(hridel.Poznamka);
                Console.WriteLine();
                Console.WriteLine(hridel.DataClankuTab);
                Console.Write("#\t");
                foreach (string n in hridel.nazvySloupcu)
                {
                    Console.Write("{0}\t", n);
                }
                Console.Write("\n");
                foreach (DataRow r in hridel.DataClankuTab.Rows)
                {
                    foreach (var item in r.ItemArray)
                    {
                        Console.Write("{0}\t", item);
                    }
                    Console.Write("\n");
                }
                // konec vypsání zadaných dat do konzole
            }

            string vystupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test_out.xlsx";
            hridel.UlozitData(vystupniSoubor);

            hridel.VytvorPrvky();
            var kO = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);

            // Výpis kritických otáček do konzole
            Console.WriteLine("\nKritické otáčky:");
            foreach (var x in kO.kritOt)
            {
                Console.Write("{0}; ", x);
            }
            Console.WriteLine();
            // Konec výpisu

            //hridel = new Hridel();
            //hridel.UlozitData(@"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test_out2.xlsx");
        }

        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.HridelNova();
            return;
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            string vstupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test.xlsx";
            hridel.HridelNova();
            bool nacteno = hridel.NacistData(vstupniSoubor);
            return;
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            string vystupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test_out.xlsx";
            hridel.UlozitData(vystupniSoubor);
            return;
        }

        private void saveAsFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void vypocetKritOtButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.VytvorPrvky();
            (hridel.KritOt, hridel.PrubehRpm, hridel.PrubehDeterminantu) = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
            hridel.VyslekyPlatne = true;


            // Krit. Ot. dát jako vlastnost hřídele. A text "1 kritické..." dát taky jako vlastnost,
            // a její obsah se bude tvořit při get. A bude mít private set. A na set krit. ot. (double[]) se nastaví
            // NotifyPropertyChanged vlastnosti s textem. Vymazat krit. ot. při novém a otevření výpočtu.
        }
    }
}
