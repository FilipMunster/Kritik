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
using System.Windows.Threading;
using Microsoft.Win32;


// Udělat: Název souboru v hlavičce okna (viz PSPad) - nový soubor -> náze v něco jako Nový výpočet.xlsx. Uložit jako - předvyplnit název souboru

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
            SloupecTyp.ItemsSource = hridel.ListTypuPrvku;

            //string vstupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test.xlsx";
            //hridel.nazevSouboru = vstupniSoubor;
            //hridel.HridelNova();
            //bool nacteno = hridel.NacistData(vstupniSoubor);

        }

        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.HridelNova();
            hridel.nazevSouboru = null;
            hridel.AnyPropertyChanged = false;
            return;
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                hridel.HridelNova();
                hridel.nazevSouboru = openFileDialog.FileName;
                hridel.NacistData(hridel.nazevSouboru);
                hridel.AnyPropertyChanged = false;
            }
            return;
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (hridel.nazevSouboru != null)
            {
                hridel.UlozitData(hridel.nazevSouboru);
                hridel.AnyPropertyChanged = false;
                return;
            }
            else { saveAsFileButton_Click(sender, e); }
            
        }

        private void saveAsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                hridel.nazevSouboru = saveFileDialog.FileName;
                hridel.UlozitData(hridel.nazevSouboru);
                hridel.AnyPropertyChanged = false;
                return;
            }
        }

        private void vypocetKritOtButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.VytvorPrvky();
            (hridel.KritOt, hridel.PrubehRpm, hridel.PrubehDeterminantu) = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            GridColumnFastEdit(cell, e);
        }

        /// <summary>
        /// Funkce pro rozbalení Comboboxu v datgridu hned po kliknutí
        /// </summary>
        private void GridColumnFastEdit(DataGridCell cell, RoutedEventArgs e)
        {
            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            var dataGrid = TabulkaDataGrid;
            if (dataGrid == null)
                return;

            if (!cell.IsFocused)
            {
                cell.Focus();
            }

            var cb = cell.Content as ComboBox;
            if (cb == null) return;
            TabulkaDataGrid.BeginEdit(e);
            cell.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            cb.IsDropDownOpen = true;
        }


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
                }
                catch { }
            }
        }
    }
}
