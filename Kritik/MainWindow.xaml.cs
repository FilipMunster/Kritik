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
using Microsoft.Win32;


// Udělat: Název souboru v hlavičce okna (viz PSPad)
// Udělat: Lépe zvýraznit označenou buňku

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
        private string NazevSouboru { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            DataContext = hridel;
            SloupecTyp.ItemsSource = hridel.ListTypuPrvku;

            string vstupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test.xlsx";
            hridel.HridelNova();
            bool nacteno = hridel.NacistData(vstupniSoubor);

        }

        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.HridelNova();
            return;
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                hridel.HridelNova();
                NazevSouboru = openFileDialog.FileName;
                hridel.NacistData(NazevSouboru);
            }
            return;
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.UlozitData(NazevSouboru);
            return;
        }

        private void saveAsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                NazevSouboru = saveFileDialog.FileName;
                hridel.UlozitData(NazevSouboru);
                return;
            }
        }

        private void vypocetKritOtButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.VytvorPrvky();
            (hridel.KritOt, hridel.PrubehRpm, hridel.PrubehDeterminantu) = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
            hridel.VyslekyPlatne = true;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private bool posledniBunkaBylaFalse = false;
        /// <summary>
        /// Funkce pro přeskakování needitovatelných buněk
        /// </summary>
        private void DataGridCell_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OldFocus is DataGridCell oldCell && sender is DataGridCell newCell)
            {
                try
                {
                    DataGridColumn col = newCell.Column;
                    Hridel.PrvekTab prvek = (Hridel.PrvekTab)newCell.DataContext;
                    bool isCellEditable = prvek.IsEditableArray[col.DisplayIndex];

                    if (!isCellEditable)
                    {
                        var isNext = oldCell.Column.DisplayIndex < newCell.Column.DisplayIndex;
                        var direction = isNext ? FocusNavigationDirection.Next : FocusNavigationDirection.Previous;
                        newCell.MoveFocus(new TraversalRequest(direction));
                        e.Handled = true;
                    }
                    posledniBunkaBylaFalse = false;
                }
                catch { }
            }
        }
    }
}
