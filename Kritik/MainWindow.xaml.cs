using System;
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
        public MainWindow()
        {
            InitializeComponent();
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

    }
}
