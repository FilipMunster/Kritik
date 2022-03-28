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
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;

namespace Kritik
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow GetMainWindow() { return mainWindow; }
        public static void SetMainWindow(MainWindow value) { mainWindow = value; }
        private static MainWindow mainWindow;

        /// <summary>
        /// Globální instance třídy Hridel
        /// </summary>
        public static Hridel hridel;
        /// <summary>
        /// Obsahuje kopii hřídele vytvořenou po výpočtu. Při ukládání se použijí data z této vlastnosti, aby nedošlo k rozporu mezi výsledky a zadáním.
        /// </summary>
        private Hridel HridelPouzitaKVypoctu { get; set; }
        /// <summary>
        /// Označuje, zda pracuji s novým souborem, nebo s načteným z disku.
        /// </summary>
        public static bool NovySoubor { get; set; } // slouží k rozpoznávání, jestli tlačítko uložit má soubor uložit, nebo uložit jako. true -> uložit jako

        private string VykreslenyHlavniGraf { get; set; }


        public MainWindow()
        {
            //Načteno:
            ShaftElement shaftElement1 = new ShaftElement() { De = 0.5, L = 10 };
            ShaftElement shaftElement2 = new ShaftElement() { Type = ElementType.rigid, L = 0.25 };
            ShaftElement shaftElement3 = new ShaftElement(ElementType.beamPlus) { L = 5, De = 0.6, M = 50000, Id = 40000, Io = 40000, Division = 10, IdN = 0, IdNValue = 0.65 };

            List<ShaftElement> shaftElements = new List<ShaftElement>() { shaftElement1, shaftElement2, shaftElement3};
            // Vytvoří se hřídel:
            Shaft shaft = new Shaft(shaftElements);

            CollectionHistory<ShaftElementForDataGrid> history = new(shaft.Elements);
            
            


            return;

            


            InitializeComponent();
            SetMainWindow(this);

            hridel = new Hridel();
            DataContext = hridel;
            SloupecTyp.ItemsSource = hridel.ListTypuPrvku;
            hridel.HridelNova();
            hridel.AnyPropertyChanged = false;
            hridel.NazevSouboru = "Nový výpočet.xlsx";
            NovySoubor = true;
            VykreslenyHlavniGraf = "w";
            VykreslitKmity();
            VyplnJazyky();
            //Strings.SelectedLanguage = (Strings.Language)jazykCombobox.SelectedIndex;
            NacistNastaveni();

            //////////////////
            //string vstupniSoubor = @"d:\TRANSIENT ANALYSIS\_Pokusy\kriticke otacky\kritik_test1_pulka.xlsx";
            //hridel.HridelNova();
            //hridel.nazevSouboru = vstupniSoubor;
            //DataLoadSave.NacistData(hridel.nazevSouboru,hridel);
            //hridel.AnyPropertyChanged = false;
            //novySoubor = false;
            //Historie.New();
            //backBtn.IsEnabled = Historie.BackBtnEnabled;
            //forwardBtn.IsEnabled = Historie.ForwardBtnEnabled;
            //hridel.VytvorPrvky();
            //(hridel.KritOt, hridel.PrubehRpm, hridel.PrubehDeterminantu) = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
            //hridel.TvaryKmitu = Vypocet.TvaryKmitu(hridel);
            //VykreslitKmity();
            //////////////////

        }

        private void VykreslitHlavniGraf(string value)
        {
            if (hridel.TvaryKmitu != null && hridel.TvaryKmitu.Length > 0)
            {
                plot1Border.BorderBrush = Brushes.Transparent;
                plot2Border.BorderBrush = Brushes.Transparent;
                plot3Border.BorderBrush = Brushes.Transparent;
                plot4Border.BorderBrush = Brushes.Transparent;

                int id = Convert.ToInt32(cisloKritOtZobrazitTextBox.Text);
                while (id > hridel.TvaryKmitu.Length) { id--; cisloKritOtZobrazitTextBox.Text = id.ToString(); }
                id += - 1;

                bool kreslitUzly = (bool)vykreslitUzlyCheckBox.IsChecked;
                PlotModel plt = Plot.NewVelky();
                switch (value)
                {
                    case "w":
                    default:
                        if (kreslitUzly) { plt.Series.Add(Plot.NewCircleLine(hridel.TvaryKmitu[id].xUzly, hridel.TvaryKmitu[id].wUzly)); }
                        plt.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].w));
                        plot1Border.BorderBrush = Brushes.SteelBlue;
                        break;
                    case "phi":
                        if (kreslitUzly) { plt.Series.Add(Plot.NewCircleLine(hridel.TvaryKmitu[id].xUzly, hridel.TvaryKmitu[id].phiUzly)); }
                        plt.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].phi));
                        plot2Border.BorderBrush = Brushes.SteelBlue;
                        break;
                    case "m":
                        if (kreslitUzly) { plt.Series.Add(Plot.NewCircleLine(hridel.TvaryKmitu[id].xUzly, hridel.TvaryKmitu[id].mUzly)); }
                        plt.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].m));
                        plot3Border.BorderBrush = Brushes.SteelBlue;
                        break;
                    case "t":
                        if (kreslitUzly) { plt.Series.Add(Plot.NewCircleLine(hridel.TvaryKmitu[id].xUzly, hridel.TvaryKmitu[id].tUzly)); }
                        plt.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].t));
                        plot4Border.BorderBrush = Brushes.SteelBlue;
                        break;
                }
                plt.Series.Add(Plot.NewOsa(hridel.TvaryKmitu[id].x));
                hlavniPlot.Model = plt;
                VykreslenyHlavniGraf = value;
            }       
        }
        private void VykreslitKmity()
        {
            if (hridel.TvaryKmitu != null && hridel.TvaryKmitu.Length > 0)
            {
                int id = Convert.ToInt32(cisloKritOtZobrazitTextBox.Text);
                while (id > hridel.TvaryKmitu.Length) { id--; cisloKritOtZobrazitTextBox.Text = id.ToString(); }
                id += -1;

                VykreslitHlavniGraf(VykreslenyHlavniGraf);

                PlotModel plt1 = Plot.NewMaly();
                plt1.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].w, OxyColors.Blue));
                plt1.Series.Add(Plot.NewOsa(hridel.TvaryKmitu[id].x));
                plotView1.Model = plt1;
                PlotModel plt2 = Plot.NewMaly();
                plt2.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].phi, OxyColors.Red));
                plt2.Series.Add(Plot.NewOsa(hridel.TvaryKmitu[id].x));
                plotView2.Model = plt2;
                PlotModel plt3 = Plot.NewMaly();
                plt3.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].m, OxyColors.Tan));
                plt3.Series.Add(Plot.NewOsa(hridel.TvaryKmitu[id].x));
                plotView3.Model = plt3;
                PlotModel plt4 = Plot.NewMaly();
                plt4.Series.Add(Plot.NewLine(hridel.TvaryKmitu[id].x, hridel.TvaryKmitu[id].t, OxyColors.Plum));
                plt4.Series.Add(Plot.NewOsa(hridel.TvaryKmitu[id].x));
                plotView4.Model = plt4;
            }
            else
            {
                hlavniPlot.Model = Plot.NewVelky();
                plotView1.Model = Plot.NewMaly();
                plotView2.Model = Plot.NewMaly();
                plotView3.Model = Plot.NewMaly();
                plotView4.Model = Plot.NewMaly();
            }
        }

        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.HridelNova();
            hridel.NazevSouboru = "Nový výpočet.xlsx";
            HridelPouzitaKVypoctu = null;
            hridel.AnyPropertyChanged = false;
            NovySoubor = true;
            Historie.New();
            backBtn.IsEnabled = Historie.BackBtnEnabled;
            forwardBtn.IsEnabled = Historie.ForwardBtnEnabled;
            hridel.NotifyPropertyChanged("SchemaHridele");
            VykreslitKmity();
            return;
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            bool dragEvent = e.GetType().Name == "DragEventArgs";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
            if (dragEvent || openFileDialog.ShowDialog() == true)
            {
                hridel.HridelNova();
                if (!dragEvent) hridel.NazevSouboru = openFileDialog.FileName;
                HridelPouzitaKVypoctu = null;
                NovySoubor = false;
                bool ok = DataLoadSaveOld.NacistData(hridel.NazevSouboru, hridel);
                if (!ok) { 
                    MessageBox.Show("Soubor \"" + hridel.NazevSouboru + "\" se nepodařilo načíst.", "Chyba načítání souboru", MessageBoxButton.OK, MessageBoxImage.Error);
                    hridel.NazevSouboru = "Nový výpočet.xlsx";
                }
                hridel.AnyPropertyChanged = false;
                Historie.New();
                backBtn.IsEnabled = Historie.BackBtnEnabled;
                forwardBtn.IsEnabled = Historie.ForwardBtnEnabled;
                hridel.NotifyPropertyChanged("SchemaHridele");
                VykreslitKmity();
            }
            return;
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!NovySoubor)
            {
                bool ok1 = DataLoadSaveOld.UlozitVysledky(hridel.NazevSouboru, HridelPouzitaKVypoctu);
                bool ok2 = DataLoadSaveOld.UlozitData(hridel.NazevSouboru, hridel);                
                if (!(ok1 && ok2)) { MessageBox.Show("Soubor \"" + hridel.NazevSouboru + "\" se nepodařilo uložit.", "Chyba ukládání souboru", MessageBoxButton.OK, MessageBoxImage.Error); }
                else
                {
                    hridel.AnyPropertyChanged = false;
                }                
                return;
            }
            else { saveAsFileButton_Click(sender, e); }

        }

        private void saveAsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Zadání hřídele (*.xlsx)|*.xlsx";
            saveFileDialog.FileName = System.IO.Path.GetFileName(hridel.NazevSouboru);
            if (saveFileDialog.ShowDialog() == true)
            {
                hridel.NazevSouboru = saveFileDialog.FileName;
                bool ok1 = DataLoadSaveOld.UlozitVysledky(hridel.NazevSouboru, HridelPouzitaKVypoctu);
                bool ok2 = DataLoadSaveOld.UlozitData(hridel.NazevSouboru, hridel);                
                if (!(ok1 && ok2)) { MessageBox.Show("Soubor \"" + hridel.NazevSouboru + "\" se nepodařilo uložit.", "Chyba ukládání souboru", MessageBoxButton.OK, MessageBoxImage.Error); }
                else
                {
                    hridel.AnyPropertyChanged = false;
                    NovySoubor = false;
                }
                return;
            }
        }

        private async void vypocetKritOtButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Hridel.PrvekTab p in hridel.PrvkyHrideleTab)
            {
                if (p.Typ == Hridel.beamPlusKeyword && p.Deleni < 1)
                {
                    int i = hridel.PrvkyHrideleTab.IndexOf(p);
                    MessageBox.Show("Není zadán počet dělení prvku " + (i + 1) + ".", "Špatně zadaná hodnota", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            hridel.KritOt = new double[] { -1 };
            await Task.Run(() => { VypocetKritickychOtacek(); });
            ZkopirovatHridelPouzitouKVypoctu();
            VykreslitKmity();
            VytvorPopisekVypoctu();
            EasterEgg();
        }

        private void VypocetKritickychOtacek()
        {
            hridel.VytvorPrvky();
            hridel.KritOt = null;
            hridel.KritOt2 = null;

            if (!hridel.VlivOtacekRotoruIsChecked)
            {
                hridel.KritOt = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
                hridel.TvaryKmitu = Vypocet.TvaryKmitu(hridel);
            }
            else
            {
                if (hridel.VlivOtacekVlastniIsChecked)
                {
                    foreach (Hridel.Prvek prvek in hridel.PrvkyHridele)
                    {
                        prvek.RpmHridele = hridel.VlivOtacekRotoruVlastni;
                    }
                    hridel.KritOt = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
                    hridel.TvaryKmitu = Vypocet.TvaryKmitu(hridel);
                }
                else
                {
                    if (hridel.OtackyProvozni > 0)
                    {
                        foreach (Hridel.Prvek prvek in hridel.PrvkyHridele)
                        {
                            prvek.RpmHridele = hridel.OtackyProvozni;
                        }
                        hridel.KritOt = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
                        hridel.TvaryKmitu = Vypocet.TvaryKmitu(hridel);
                    }
                    if (hridel.OtackyPrubezne > 0)
                    {
                        foreach (Hridel.Prvek prvek in hridel.PrvkyHridele)
                        {
                            prvek.RpmHridele = hridel.OtackyPrubezne;
                        }
                        hridel.KritOt2 = Vypocet.KritickeOtacky(hridel, hridel.NKritMax);
                        hridel.TvaryKmitu2 = Vypocet.TvaryKmitu(hridel);
                    }
                }
            }
            
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
                        hridel.OznacenyRadek = prvek;
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
            Hridel.PrvekTab r = hridel.OznacenyRadek;
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
                idNHodnotaTextBlock.Text = "(Jdᵢ = " + val.ToString("0.###") + " kg.m²)";
                hridel.AnyPropertyChanged = true;
            }
            else { idNHodnotaTextBlock.Text = "(Jdᵢ =  . . .  kg.m²)"; }
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
            HistorieAdd();
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
            HistorieAdd();
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
            HistorieAdd();
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
            HistorieAdd();
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
            HistorieAdd();
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
            HistorieAdd();
        }

        private void TabulkaDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            HistorieAdd();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            Historie.Back();
            backBtn.IsEnabled = Historie.BackBtnEnabled;
            forwardBtn.IsEnabled = Historie.ForwardBtnEnabled;
            hridel.NotifyPropertyChanged("SchemaHridele");
            TabulkaDataGrid.Focus();            
        }
        private void forwardBtn_Click(object sender, RoutedEventArgs e)
        {
            Historie.Forward();
            backBtn.IsEnabled = Historie.BackBtnEnabled;
            forwardBtn.IsEnabled = Historie.ForwardBtnEnabled;
            TabulkaDataGrid.Items.Refresh();
            hridel.NotifyPropertyChanged("SchemaHridele");
            TabulkaDataGrid.Focus();
        }

        private void deleteAllBtn_Click(object sender, RoutedEventArgs e)
        {
            hridel.PrvkyHrideleTab.Clear();
            HistorieAdd();
        }

        private void HistorieAdd()
        {
            Historie.Add();
            backBtn.IsEnabled = Historie.BackBtnEnabled;
            forwardBtn.IsEnabled = Historie.ForwardBtnEnabled;
            hridel.NotifyPropertyChanged("SchemaHridele");
            TabulkaDataGrid.Focus();
        }

        private void plotView1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VykreslitHlavniGraf("w");
            VytvorPopisekVypoctu();
        }

        private void plotView2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VykreslitHlavniGraf("phi");
            VytvorPopisekVypoctu();
        }

        private void plotView3_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VykreslitHlavniGraf("m");
            VytvorPopisekVypoctu();
        }

        private void plotView4_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VykreslitHlavniGraf("t");
            VytvorPopisekVypoctu();
        }

        private void CisloKritOtUpButton_Click(object sender, RoutedEventArgs e)
        {
            int c = Convert.ToInt32(cisloKritOtZobrazitTextBox.Text);
            if (hridel.TvaryKmitu != null && c < hridel.TvaryKmitu.Length) { cisloKritOtZobrazitTextBox.Text = (c + 1).ToString(); VykreslitKmity(); VytvorPopisekVypoctu(); }
        }
        private void CisloKritOtDownButton_Click(object sender, RoutedEventArgs e)
        {
            int c = Convert.ToInt32(cisloKritOtZobrazitTextBox.Text);
            if (hridel.TvaryKmitu != null && c > 1) { cisloKritOtZobrazitTextBox.Text = (c - 1).ToString(); VykreslitKmity(); VytvorPopisekVypoctu(); }
        }

        private void TabulkaDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            hridel.NotifyPropertyChanged("SchemaHridele");
        }

        private void DataGridCell_LostMouseCapture(object sender, MouseEventArgs e)
        {
            hridel.NotifyPropertyChanged("SchemaHridele");
        }

        private void TabulkaDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TabItem1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vypocetKritOtButton_Click(null, null);
            VykreslitKmity();
            schemaHridele2.Model = Plot.SchemaHridele(hridel.PrvkyHrideleTab, null, schemaHridele2.ActualWidth);
        }

        private void schemaHridele_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (deleniHridelPlusTextBox.IsEnabled) { hridel.HridelPlusDeleni = deleniHridelPlusTextBox.Text; }
            OxyPlot.ElementCollection<OxyPlot.Axes.Axis> axisList = schemaHridele.Model.Axes;
            OxyPlot.Axes.Axis xAxis = axisList.FirstOrDefault(ax => ax.Position == OxyPlot.Axes.AxisPosition.Bottom);
            OxyPlot.Axes.Axis yAxis = axisList.FirstOrDefault(ax => ax.Position == OxyPlot.Axes.AxisPosition.Left);
            var pos = e.GetPosition(schemaHridele);
            OxyPlot.ScreenPoint screenPoint = new ScreenPoint(pos.X, pos.Y);
            DataPoint pozice = OxyPlot.Axes.Axis.InverseTransform(screenPoint, xAxis, yAxis);

            List<double> xovePozice = new List<double> { 0 };
            foreach (Hridel.PrvekTab p in hridel.PrvkyHrideleTab) { xovePozice.Add(xovePozice.Last() + p.L); }
            int i = -1;
            for (int j = 0; j < (xovePozice.Count - 1); j++)
            {
                if (pozice.X > xovePozice[j] && pozice.X < xovePozice[j+1]) { i = j; break; }
            }
            if (i >= 0 && (Math.Abs(pozice.Y) < Math.Abs(hridel.PrvkyHrideleTab[i].De / 2) 
                || (hridel.PrvkyHrideleTab[i].Typ == Hridel.rigidKeyword && Math.Abs(pozice.Y) < Math.Abs(Plot.dTuhy / 2))))
            {
                TabulkaDataGrid.SelectedItem = hridel.PrvkyHrideleTab[i];
                TabulkaDataGrid.ScrollIntoView(TabulkaDataGrid.SelectedItem);
            }
        }

        public void DiskPruzinaMouseDown(object s, OxyPlot.OxyMouseDownEventArgs e)
        {
            OxyPlot.Series.LineSeries line = (OxyPlot.Series.LineSeries)s;
            int i = (int)line.Tag;
            TabulkaDataGrid.SelectedItem = hridel.PrvkyHrideleTab[i];
            TabulkaDataGrid.ScrollIntoView(TabulkaDataGrid.SelectedItem);
        }

        public void VytvorPopisekVypoctu()
        {
            //if (hridel.TvaryKmitu != null && hridel.TvaryKmitu.Count() > 0)
            //{
            //    string popisek = "";
            //    int cisloKritOt = int.Parse(cisloKritOtZobrazitTextBox.Text);
            //    while (cisloKritOt > hridel.TvaryKmitu.Length) { cisloKritOt--; }
            //    string kritOt = String.Format("{0:0.000}", hridel.KritOt[cisloKritOt-1]);
            //    string gyros;
            //    switch (hridel.Gyros)
            //    {
            //        case Hridel.gyrosZanedbaniKeyword:
            //            gyros = Strings.VlivGyrosNeniUvazovan;
            //            break;
            //        case Hridel.gyrosSoubeznaKeyword:
            //            gyros = Strings.SoubeznaPrecese;
            //            break;
            //        case Hridel.gyrosProtibeznaKeyword:
            //            gyros = Strings.ProtibeznaPrecese;
            //            break;
            //        default:
            //            gyros = "";
            //            break;
            //    }
            //    string vykresleno;
            //    switch (VykreslenyHlavniGraf)
            //    {
            //        case "w":
            //        default:
            //            vykresleno = Strings.PruhybHridele + " w";
            //            break;
            //        case "phi":
            //            vykresleno = Strings.NatoceniHridele + " φ";
            //            break;
            //        case "m":
            //            vykresleno = Strings.OhybovyMoment + " M";
            //            break;
            //        case "t":
            //            vykresleno = Strings.PosouvajiciSila + " T";
            //            break;
            //    }
            //    popisek += Strings.OrdinalNumber(cisloKritOt) + " "+ Strings.kritickeOtacky +" = " + kritOt + " min⁻¹\n";
            //    popisek += vykresleno + "\n";
            //    popisek += gyros + "\n";
            //    popisek += nazevTextBox.Text + "\n";
            //    popisek += popisTextBox.Text;
            //    popisVypoctuTextBlock.Text = popisek;
            //}            
        }

        private void ulozitTvarKmituButton_Click(object sender, RoutedEventArgs e)
        {
            hridel.AnyPropertyChanged = true;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Obrázek PNG (*.png)|*.png";
            string path = System.IO.Path.GetDirectoryName(hridel.NazevSouboru);
            string fname = System.IO.Path.GetFileNameWithoutExtension(hridel.NazevSouboru);
            string tvar;
            switch (VykreslenyHlavniGraf)
            {
                case "w":
                case "phi":
                    tvar = VykreslenyHlavniGraf;
                    break;
                case "m":
                case "t":
                    tvar = VykreslenyHlavniGraf.ToUpper();
                    break;
                default:
                    tvar = "";
                    break;
            }

            saveFileDialog.FileName = System.IO.Path.GetFileName(path + "\\" + fname + "_" + tvar + ".png");
            if (saveFileDialog.ShowDialog() == true)
            {
                ModelyDoPNG obrazek = new ModelyDoPNG();
                int vyskaSchematu = Convert.ToInt32(Math.Round(0.22 * obrazek.Sirka));
                int vyskaGrafu = Convert.ToInt32(Math.Round(0.4 * obrazek.Sirka));
                int vyskaPopisu = 240;

                if (vykreslitSchemaCheckBox.IsChecked == true) { obrazek.Pridat(schemaHridele2.Model, vyskaSchematu); }
                if (vykreslitGrafCheckBox.IsChecked == true) { obrazek.Pridat(hlavniPlot.Model, vyskaGrafu); }
                if (vykreslitPopisekCheckBox.IsChecked == true) 
                {
                    PlotModel popisekModel = Plot.ModelFromString(popisVypoctuTextBlock.Text, 4, 0, 15);
                    obrazek.Pridat(popisekModel, vyskaPopisu); 
                }
                obrazek.Ulozit(saveFileDialog.FileName);
                hridel.AnyPropertyChanged = false;
                return;
            }
        }

        private void ZkopirovatHridelPouzitouKVypoctu()
        {
            HridelPouzitaKVypoctu = new Hridel()
            {
                VypocetNazev = hridel.VypocetNazev,
                VypocetPopis = hridel.VypocetPopis,
                VypocetResil = hridel.VypocetResil,
                VypocetDatum = hridel.VypocetDatum,
                OpLeva = hridel.OpLeva,
                OpPrava = hridel.OpPrava,
                ModulPruznosti = hridel.ModulPruznosti,
                Rho = hridel.Rho,
                Gyros = hridel.Gyros,
                VlivOtacekRotoruIsChecked = hridel.VlivOtacekRotoruIsChecked,
                VlivOtacekVlastniIsChecked = hridel.VlivOtacekVlastniIsChecked,
                VlivOtacekRotoruVlastni = hridel.VlivOtacekRotoruVlastni,
                OtackyProvozni = hridel.OtackyProvozni,
                OtackyPrubezne = hridel.OtackyPrubezne,
                Poznamka = hridel.Poznamka,
            };

            if (hridel.KritOt != null)
            {
                HridelPouzitaKVypoctu.KritOt = new double[hridel.KritOt.Length];
                hridel.KritOt.CopyTo(HridelPouzitaKVypoctu.KritOt, 0);
            }

            if (hridel.KritOt2 != null)
            {
                HridelPouzitaKVypoctu.KritOt2 = new double[hridel.KritOt2.Length];
                hridel.KritOt2.CopyTo(HridelPouzitaKVypoctu.KritOt2, 0);
            }

            HridelPouzitaKVypoctu.PrvkyHrideleTab = new ObservableCollection<Hridel.PrvekTab>();
            foreach (Hridel.PrvekTab p in hridel.PrvkyHrideleTab)
            {
                HridelPouzitaKVypoctu.PrvkyHrideleTab.Add(new Hridel.PrvekTab()
                {
                    Typ = p.Typ,
                    L = p.L,
                    De = p.De,
                    Di = p.Di,
                    M = p.M,
                    Io = p.Io,
                    Id = p.Id,
                    K = p.K,
                    Cm = p.Cm,
                    Deleni = p.Deleni,
                    IdN = p.IdN,
                    IdNValue = p.IdNValue
                });
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox s = (TextBox)sender;
            s.SelectAll();
        }

        private void kritOtTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox s = (TextBox)sender;
            s.SelectAll();
        }

        private void EasterEgg()
        {
            if (resilTextBox.Text == "Dave Lister")
            {
                Uri uri = new Uri("pack://application:,,,/icons/rd.dat");
                BitmapImage image = new BitmapImage(uri);
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = image;
                TabulkaDataGrid.Background = ib;
                TabulkaDataGrid.RowBackground = Brushes.Transparent;
                TabulkaDataGrid.AlternatingRowBackground = Brushes.Transparent;
            }
        }

        private void vykreslitUzlyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            VykreslitHlavniGraf(VykreslenyHlavniGraf);
            Properties.Settings.Default.drawNodes = (bool)((CheckBox)sender).IsChecked;
        }
        /// <summary>
        /// Naplní combobox výběru jazyků hodnotami
        /// </summary>
        private void VyplnJazyky()
        {
            //foreach (var j in Strings.LanguageName)
            //{
            //    jazykCombobox.Items.Add(j.Value);
            //}
            //try { jazykCombobox.SelectedIndex = Properties.Settings.Default.lang; }
            //catch { jazykCombobox.SelectedIndex = 0; }
        }

        private void jazykCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ComboBox s = (ComboBox)sender;
            //Strings.SelectedLanguage = (Strings.Language)s.SelectedIndex;
            //try { Properties.Settings.Default.lang = s.SelectedIndex; }
            //catch { Properties.Settings.Default.lang = 0; }
            //VytvorPopisekVypoctu();
        }

        private void HlavniOkno_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void vykreslitSchemaCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.drawScheme = (bool)((CheckBox)sender).IsChecked;
        }

        private void vykreslitGrafCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.drawShape = (bool)((CheckBox)sender).IsChecked;
        }

        private void vykreslitPopisekCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.drawDescription = (bool)((CheckBox)sender).IsChecked;
        }
        private void NacistNastaveni()
        {
            if (Properties.Settings.Default.author != "") { resilTextBox.Text = Properties.Settings.Default.author; }
            vykreslitUzlyCheckBox.IsChecked = Properties.Settings.Default.drawNodes;
            vykreslitSchemaCheckBox.IsChecked = Properties.Settings.Default.drawScheme;
            vykreslitGrafCheckBox.IsChecked = Properties.Settings.Default.drawShape;
            vykreslitPopisekCheckBox.IsChecked = Properties.Settings.Default.drawDescription;
        }

        private void deleniHridelPlusTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            hridel.NotifyPropertyChanged("SchemaHridele");
        }

        private void dnesTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            hridel.VypocetDatum = DateTime.Today.ToShortDateString();
        }

        private void vlivOtacekExpandButton_Click(object sender, RoutedEventArgs e)
        {
            Button s = (Button)sender;
            GridLengthConverter gridLength = new GridLengthConverter();
            System.Windows.ThicknessConverter thickness = new System.Windows.ThicknessConverter();
            if ((string)s.Tag == "hidden")
            {
                s.Tag = "expanded";
                vlivOtacekGridRow.Height = (GridLength)gridLength.ConvertFrom(vlivOtacekGridRow.MaxHeight);
                s.Content = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/icons/StepBackArrow_16x.png")),
                    Width = 10,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    Margin = (Thickness)thickness.ConvertFromString("0, 8, 0, 0")
                };  
            }
            else if (!(bool)vlivOtacekRotoruCheckBox.IsChecked)
            {
                s.Tag = "hidden";
                vlivOtacekGridRow.Height = (GridLength)gridLength.ConvertFromString("0");
                s.Content = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/icons/StepOverArrow_16x.png")),
                    Width = 10,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    Margin = (Thickness)thickness.ConvertFromString("0, 8, 0, 0")
                };
            }
        }

        private void HlavniOkno_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files[0].EndsWith(".xlsx"))
                {
                    hridel.NazevSouboru = files[0];
                    openFileButton_Click(sender, e);
                }
            }
            
        }

        private void vlivOtacekRotoruCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox s = (CheckBox)sender;
            vlivOtacekProvozniAPrubezneRadioButton.IsEnabled = (bool)s.IsChecked;
            vlivOtacekVlastniRadioButton.IsEnabled = (bool)s.IsChecked;
        }
    }
}
