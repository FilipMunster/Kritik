using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kritik
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Resource dictionary containing currently selected language
        /// </summary>
        public readonly ResourceDictionary resourceDictionary;
        public MainWindow()
        {
            SetApplicationLanguage(out this.resourceDictionary);
            InitializeComponent();
            ((MainViewModel)this.DataContext).SelectedElementChanged += OnSelectedElementChanged;
            ApplicationLanguageComboBox.SelectedValue = Properties.Settings.Default.applicationLanguage;
        }

        /// <summary>
        /// Array of names of available languages
        /// </summary>
        public string[] Languages
        {
            get
            {
                List<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries.ToList();
                List<string> languages = new List<string>();
                foreach (ResourceDictionary dictionary in dictionaries)
                {
                    languages.Add((string)dictionary["@languageName"]);
                }
                languages.Sort();
                return languages.ToArray();
            }
        }

        private void OnSelectedElementChanged(object sender, int elementId)
        {
            ShaftDataGrid.CommitEdit();
            ShaftDataGrid.CommitEdit();
            ShaftDataGrid.Items.Refresh();
            ShaftDataGrid.UpdateLayout();
            if (elementId >= 0)
                ShaftDataGrid.ScrollIntoView(ShaftDataGrid.Items.GetItemAt(elementId));

            DataGridRow row = (DataGridRow)ShaftDataGrid.ItemContainerGenerator.ContainerFromIndex(elementId);
            _ = row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
        }

        private void KritikMainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files[0].EndsWith(".xlsx"))
                {
                    ((MainViewModel)this.DataContext).OpenFile(files[0]);
                }
            }
        }

        private void KritikMainWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void TextBoxSelectContentByKeyboard(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox s = (TextBox)sender;
            s.SelectAll();
        }

        private void TextBoxSelectContentByMouse(object sender, MouseButtonEventArgs e)
        {
            TextBox s = (TextBox)sender;
            s.SelectAll();
        }

        private void ShaftDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            _ = ShaftDataGrid.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        void ShaftDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// Method for skipping non-editable cells
        /// </summary>
        private void DataGridCell_SkipNonEditable(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OldFocus is DataGridCell oldCell && sender is DataGridCell newCell)
            {
                if (newCell.DataContext.GetType().Name == "NamedObject")
                    return;

                DataGridColumn col = newCell.Column;
                ShaftElementForDataGrid element = (ShaftElementForDataGrid)newCell.DataContext;
                bool isCellEditable = element.IsEditableArray[col.DisplayIndex];

                if (!isCellEditable)
                {
                    var isNext = oldCell.Column.DisplayIndex < newCell.Column.DisplayIndex;
                    var direction = isNext ? FocusNavigationDirection.Next : FocusNavigationDirection.Previous;
                    newCell.MoveFocus(new TraversalRequest(direction));
                    e.Handled = true;
                    ((MainViewModel)this.DataContext).ShaftElementSelected = element;
                }
            }
        }

        /// <summary>
        /// Method for opening the ComboBox in DataGrid right after Mouse Click
        /// </summary>
        private void GridColumnFastEdit(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            var dataGrid = ShaftDataGrid;
            if (dataGrid == null)
                return;

            if (!cell.IsFocused)
            {
                _ = cell.Focus();
            }

            var cb = cell.Content as ComboBox;
            if (cb == null) return;
            ShaftDataGrid.BeginEdit(e);
            cell.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            cb.IsDropDownOpen = true;
        }

        /// <summary>
        /// Disables default DataGrid KeyEvents defined in keysToOverride so custom KeyBindings can be handled
        /// </summary>
        private void ShaftDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid is null)
                return;

            bool[] keysToOverride =
                {
                    e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.Control,
                    e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.Control,
                    e.Key == Key.Right && e.KeyboardDevice.Modifiers == ModifierKeys.Control,
                    e.Key == Key.Delete
                };

            if (keysToOverride.Any((key) => key))
            {
                dataGrid.CommitEdit(); // Commiting edit so the datagrid items can be refreshed afterwards.
                dataGrid.CommitEdit(); // Must be called 2 times to work.
                e.Handled = true;
                RaiseEvent(new KeyEventArgs(e.KeyboardDevice, PresentationSource.FromVisual(this), 0, e.Key)
                {
                    RoutedEvent = Keyboard.KeyDownEvent
                });
            }
        }

        private void BeamPlusIndexComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ShaftDataGrid.ScrollIntoView(ShaftDataGrid.SelectedItem);
        }

        private void SetApplicationLanguage(out ResourceDictionary resourceDictionary)
        {
            string selectedLanguage = Properties.Settings.Default.applicationLanguage;
            List<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries.ToList();
            resourceDictionary = dictionaries.Find((dict) => (string)dict["@languageName"] == selectedLanguage);

            if (resourceDictionary is not null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            
        }

        private void ApplicationLanguageComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string selectedLanguage = (string)((ComboBox)sender).SelectedValue;
            if (selectedLanguage is null)
                selectedLanguage = "english";

            if (selectedLanguage == Properties.Settings.Default.applicationLanguage)
                return;

            Properties.Settings.Default.applicationLanguage = selectedLanguage;

            List<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries.ToList();
            ResourceDictionary newResourceDictionary = dictionaries.Find(
                (dict) => (string)dict["@languageName"] == selectedLanguage);

            MessageBox.Show((string)newResourceDictionary["changesWillTakeEfectAfterRestart"],
                (string)newResourceDictionary["Notice"], MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
