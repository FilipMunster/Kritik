using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
using System.Timers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Kritik
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //public static MainWindow AppWindow; - díky tomuto můžu z ostatních tříd přistupovat do MainWindow (vlastnosti, metody, prvky okna)

        /// <summary>
        /// Globální instance třídy Hridel
        /// </summary>
        public static readonly Hridel hridel = new();

        private bool novySoubor; // slouží k rozpoznávání, jestli tlačítko uložit má soubor uložit, nebo uložit jako. true -> uložit jako
        public MainWindow()
        {
            InitializeComponent();

            DataContext = hridel;
            SloupecTyp.ItemsSource = hridel.ListTypuPrvku;
            hridel.HridelNova();
            hridel.AnyPropertyChanged = false;
            hridel.nazevSouboru = "Nový výpočet.xlsx";
            novySoubor = true;


            //////////////////
            string vstupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test1_pulka.xlsx";
            hridel.HridelNova();
            hridel.nazevSouboru = vstupniSoubor;
            hridel.NacistData(hridel.nazevSouboru);
            hridel.AnyPropertyChanged = false;
            novySoubor = false;
            Historie.New();
            //////////////////

        }

        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.HridelNova();
            hridel.nazevSouboru = "Nový výpočet.xlsx";
            hridel.AnyPropertyChanged = false;
            novySoubor = true;
            Historie.New();
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
                novySoubor = false;
                Historie.New();
            }
            return;
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (novySoubor == false)
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
            saveFileDialog.FileName = System.IO.Path.GetFileName(hridel.nazevSouboru);
            if (saveFileDialog.ShowDialog() == true)
            {
                hridel.nazevSouboru = saveFileDialog.FileName;
                hridel.UlozitData(hridel.nazevSouboru);
                hridel.AnyPropertyChanged = false;
                novySoubor = false;
                return;
            }
        }

        private async void vypocetKritOtButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.KritOt = new double[] { -1 };

            await Task.Run(() => {
                hridel.VytvorPrvky();
                (hridel.KritOt, hridel.PrubehRpm, hridel.PrubehDeterminantu) = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
            });
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void cisloPrvkuHridelPlusComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ((ComboBox)sender).ItemsSource = hridel.IndexyHrideliPlus;
            NotifyPropertyChanged("IndexyHrideliPlus");
        }
        private void cisloPrvkuHridelPlusComboBox_DropDownClosed(object sender, EventArgs e)
        {
            int id;
            if (((ComboBox)sender).HasItems)
            {
                if (((ComboBox)sender).SelectedValue != null)
                {
                    id = (int)((ComboBox)sender).SelectedValue;
                    TabulkaDataGrid.SelectedItem = hridel.PrvkyHrideleTab[id - 1];
                }

                TabulkaDataGrid.Focus();
                return;
            }
        }

        private void HridelPlus_MenuChanged()
        {
            var r = hridel.OznacenyRadek;
            if (r != null && r.Typ == Hridel.beamPlusKeyword)
            {
                double val;
                double deleni;
                double hodnota;
                try { deleni = Convert.ToDouble(deleniHridelPlusTextBox.Text); } catch { deleni = 1; }
                try { hodnota = Convert.ToDouble(idNTextBox.Text); } catch { hodnota = 0; }

                switch (r.IdN)
                {
                    case 0:
                        val = r.Id / deleni * hodnota;
                        break;
                    case 1:
                        val = hodnota; ;
                        break;
                    default:
                        val = 0;
                        break;
                }
                idNHodnotaTextBlock.Text = "(Idᵢ = " + val.ToString("0.###") + " kg.m²)";
                hridel.AnyPropertyChanged = true;
            }
            else { idNHodnotaTextBlock.Text = "(Idᵢ =  . . .  kg.m²)"; }
        }

        private void deleniHridelPlusTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HridelPlus_MenuChanged();
        }

        private void cisloPrvkuHridelPlusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HridelPlus_MenuChanged();
        }

        private void IdNZpusobComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HridelPlus_MenuChanged();
        }

        private void idNTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HridelPlus_MenuChanged();
        }
        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void TabulkaDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new Hridel.PrvekTab
            {
                Typ = Hridel.beamKeyword
            };
            TabulkaDataGrid.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            Historie.Add();
        }

        private void addRowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (hridel.OznacenyRadek != null)
            {
                int indx = hridel.PrvkyHrideleTab.IndexOf(hridel.OznacenyRadek);
                hridel.PrvkyHrideleTab.Insert(indx, new Hridel.PrvekTab() { Typ = Hridel.beamKeyword });
                TabulkaDataGrid.Focus();
                TabulkaDataGrid.SelectedIndex = indx;
                TabulkaDataGrid.Items.Refresh();
            }
            Historie.Add();
        }

        private void deleteRowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (hridel.OznacenyRadek != null)
            {
                int indx = hridel.PrvkyHrideleTab.IndexOf(hridel.OznacenyRadek);
                hridel.PrvkyHrideleTab.RemoveAt(indx);
                TabulkaDataGrid.Focus();
                if (hridel.PrvkyHrideleTab.Count() == indx) { indx--; }
                TabulkaDataGrid.SelectedIndex = indx;
                TabulkaDataGrid.Items.Refresh();
            }
            Historie.Add();
        }

        private void moveUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (hridel.OznacenyRadek != null)
            {
                int indx = hridel.PrvkyHrideleTab.IndexOf(hridel.OznacenyRadek);
                if (indx > 0) { hridel.PrvkyHrideleTab.Move(indx, indx - 1); }
                TabulkaDataGrid.Focus();
                TabulkaDataGrid.Items.Refresh();
            }
            Historie.Add();
        }

        private void moveDownBtn_Click(object sender, RoutedEventArgs e)
        {
            if (hridel.OznacenyRadek != null)
            {
                int indx = hridel.PrvkyHrideleTab.IndexOf(hridel.OznacenyRadek);
                int tabLen = hridel.PrvkyHrideleTab.Count();
                if (indx < (tabLen - 1)) { hridel.PrvkyHrideleTab.Move(indx, indx + 1); }
                TabulkaDataGrid.Focus();
                TabulkaDataGrid.Items.Refresh();
            }
            Historie.Add();
        }

        private void mirrorBtn_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Hridel.PrvekTab> p = hridel.PrvkyHrideleTab;
            for (int i = p.Count() - 1; i >= 0; i--)
            {
                p.Add(new Hridel.PrvekTab
                {
                    Typ = p[i].Typ,
                    L = p[i].L,
                    De = p[i].De,
                    Di = p[i].Di,
                    M = p[i].M,
                    Io = p[i].Io,
                    Id = p[i].Id,
                    K = p[i].K,
                    Cm = p[i].Cm,
                    Deleni = p[i].Deleni,
                    IdN = p[i].IdN,
                    IdNValue = p[i].IdNValue
                });
            }
            hridel.PrvkyHrideleTab = p;
            Historie.Add();
        }

        private void TabulkaDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Historie.Add();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            Historie.Back();
            TabulkaDataGrid.Focus();
        }
        private void forwardBtn_Click(object sender, RoutedEventArgs e)
        {
            Historie.Forward();
            TabulkaDataGrid.Focus();
        }

        private void deleteAllBtn_Click(object sender, RoutedEventArgs e)
        {
            hridel.PrvkyHrideleTab.Clear();
            Historie.Add();
            TabulkaDataGrid.Focus();
        }
        public static class Historie
        {
            private static List<ObservableCollection<Hridel.PrvekTab>> h = new();
            private static int hIndex;
            private const int delkaHistorie = 10;
            public static void New()
            {
                h.Clear();
                h.Add(new ObservableCollection<Hridel.PrvekTab>(KopieHridele()));
                hIndex = 0;
            }
            public static void Add()
            {
                if (hIndex < (h.Count -1)) { h.RemoveRange(hIndex + 1, h.Count - hIndex - 1); }
                if (h.Count == delkaHistorie) { h.RemoveAt(0); }

                h.Add(new ObservableCollection<Hridel.PrvekTab>(KopieHridele()));
                hIndex = h.Count() - 1;
            }
            public static void Back()
            {
                var hh = h;
                if (hIndex > 0)
                {
                    hIndex--;
                    hridel.PrvkyHrideleTab = new ObservableCollection<Hridel.PrvekTab>(h[hIndex]);
                }
            }
            public static void Forward()
            {
                if (hIndex < (h.Count - 1))
                {
                    hIndex++;
                    hridel.PrvkyHrideleTab = new ObservableCollection<Hridel.PrvekTab>(h[hIndex]);
                }
            }
            private static ObservableCollection<Hridel.PrvekTab> KopieHridele()
            {
                ObservableCollection<Hridel.PrvekTab> p = hridel.PrvkyHrideleTab;
                ObservableCollection<Hridel.PrvekTab> pH = new();
                for (int i = 0; i < p.Count(); i++)
                {
                    pH.Add(new Hridel.PrvekTab
                    {
                        Typ = p[i].Typ,
                        L = p[i].L,
                        De = p[i].De,
                        Di = p[i].Di,
                        M = p[i].M,
                        Io = p[i].Io,
                        Id = p[i].Id,
                        K = p[i].K,
                        Cm = p[i].Cm,
                        Deleni = p[i].Deleni,
                        IdN = p[i].IdN,
                        IdNValue = p[i].IdNValue
                    });
                }
                return pH;
            }
        }


    }
}
