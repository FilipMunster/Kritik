using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public class Shaft : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void NotifySenderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(sender.GetType().Name);
        }

        private ObservableCollection<ShaftElementForDataGrid> elements;
        public ObservableCollection<ShaftElementForDataGrid> Elements
        {
            get => elements;
            set
            {
                elements = value;
                NotifyPropertyChanged();
            }
        }
        private ShaftElementForDataGrid selectedItem;
        public ShaftElementForDataGrid SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                NotifyPropertyChanged();
            }
        }
        private ShaftProperties properties;
        public ShaftProperties Properties
        {
            get => properties;
            set
            {
                properties = value;
                NotifyPropertyChanged();
                properties.PropertyChanged += new PropertyChangedEventHandler(NotifySenderPropertyChanged);
            }
        }

        /// <summary>
        /// Creates shaft with given list of ShaftElements which are retyped to ShaftElementsForDataGrid
        /// </summary>
        /// <param name="shaftElements">List of ShaftElements</param>
        public Shaft(List<ShaftElement> shaftElements)
        {
            Elements = new ObservableCollection<ShaftElementForDataGrid>();
            foreach (var shaftElement in shaftElements)
            {
                Elements.Add(new ShaftElementForDataGrid(shaftElement));
            }
            Properties = new ShaftProperties();
        }
        /// <summary>
        /// Creates shaft with empty collection of Elements
        /// </summary>
        public Shaft()
        {
            Elements = new ObservableCollection<ShaftElementForDataGrid>();
            Properties = new ShaftProperties();
        }

        /// <summary>
        /// Inserts new element to shaft at selected row index. If no row is selected, the element is added at the end of the collection.
        /// </summary>
        public void AddElement()
        {
            if (SelectedItem is not null)
                Elements.Insert(Elements.IndexOf(SelectedItem), new ShaftElementForDataGrid());
            else
                Elements.Add(new ShaftElementForDataGrid());
        }
        /// <summary>
        /// Removes selected element from the collection
        /// </summary>
        public void RemoveSelectedElement()
        {
            if (SelectedItem is null)
                return;

            Elements.Remove(SelectedItem);
        }
        /// <summary>
        /// Moves selected element one row up.
        /// </summary>
        public void MoveElementUp()
        {
            if (SelectedItem is null)
                return;

            int selectedItemIndex = Elements.IndexOf(SelectedItem);
            if (selectedItemIndex > 0)
                Elements.Move(selectedItemIndex, selectedItemIndex - 1);
        }

        /// <summary>
        /// Moves selected element one row down.
        /// </summary>
        public void MoveElementDown()
        {
            if (SelectedItem is null)
                return;

            int selectedItemIndex = Elements.IndexOf(SelectedItem);
            if (selectedItemIndex < (Elements.Count - 1))
                Elements.Move(selectedItemIndex, selectedItemIndex + 1);
        }

        /// <summary>
        /// Mirrors the shaft
        /// </summary>
        public void Mirror()
        {
            for (int i = Elements.Count - 1; i >=0; i--)
            {
                Elements.Add((ShaftElementForDataGrid)Elements[i].Clone());
            }
        }

        /// <summary>
        /// Removes all elements of the shaft
        /// </summary>
        public void RemoveAllElements()
        {
            Elements.Clear();
        }

        /// <summary>
        /// Transforms collection of Shaft Elements to List of ElementsWithMatrix
        /// </summary>
        /// <returns>List of Shaft Elements With Matrix</returns>
        private List<ShaftElementWithMatrix> GetElementsWithMatrix()
        {
            List<ShaftElementWithMatrix> elementsWithMatrix = new List<ShaftElementWithMatrix>();
            foreach (var element in Elements)
            {
                if (element.Type == ElementType.beamPlus)
                {
                    double li = element.L / (element.Division + 1);
                    double mi = element.M / element.Division;
                    double ioi = element.Io / element.Division;
                    double idi = element.IdN == 0 ? element.Id / element.Division * element.IdNValue : element.IdNValue;
                    double ki = element.K / element.Division;
                    double cmi = element.Cm / element.Division;

                    ElementType[] elementTypes = { ElementType.beam, ElementType.disc, ElementType.support, ElementType.magnet };

                    for (int i = 0; i < element.Division; i++)
                    {
                        foreach (ElementType type in elementTypes)
                        {
                            elementsWithMatrix.Add(new ShaftElementWithMatrix(type)
                            {
                                L = li,
                                De = element.De,
                                Di = element.Di,
                                M = mi,
                                Io = ioi,
                                Id = idi,
                                K = ki,
                                Cm = cmi
                            });
                        }
                    }
                    elementsWithMatrix.Add(new ShaftElementWithMatrix(ElementType.beam) // finally add one element of type "beam"
                    {
                        L = li,
                        De = element.De,
                        Di = element.Di
                    });
                }
                else
                {
                    elementsWithMatrix.Add(new ShaftElementWithMatrix(element));
                }
            }

            // finally set common properties to all elements
            foreach (var element in elementsWithMatrix)
            {
                element.Gyros = Properties.Gyros;
                element.E = Properties.YoungModulus;
                element.Rho = Properties.MaterialDensity;
                element.ShaftRotationInfluence = Properties.ShaftRotationInfluence;
                element.ShaftRPM = Properties.ShaftRPM;
            }
            return elementsWithMatrix;
        }
        /// <summary>
        /// Computes critical speeds of the shaft
        /// </summary>
        /// <returns>Array of computed critical speeds</returns>
        public double[] GetCriticalSpeeds()
        {
            return Compute().Result;

            async Task<double[]> Compute()
            {
                return await Task.Run(() => Computation.CriticalSpeed(GetElementsWithMatrix(),
                    Properties.BCLeft, Properties.BCRight, Properties.MaxCriticalSpeed));
            }
        }
        /// <summary>
        /// Computes the oscillation shapes of the shaft
        /// </summary>
        /// <param name="criticalSpeeds">Array of critical speeds for which the Oscillation shapes are computed</param>
        /// <returns>Array of oscillation shapes</returns>
        public OscillationShape[] GetOscillationShapes(double[] criticalSpeeds)
        {
            return Computation.OscillationShapes(GetElementsWithMatrix(), Properties.BCLeft, Properties.BCRight, criticalSpeeds);
        }

        public object Clone()
        {
            Shaft newShaft = (Shaft)this.MemberwiseClone();
            
            newShaft.Elements = new ObservableCollection<ShaftElementForDataGrid>();
            foreach (ShaftElementForDataGrid element in this.Elements)
            {
                newShaft.Elements.Add((ShaftElementForDataGrid)element.Clone());
            }

            newShaft.Properties = (ShaftProperties)this.Properties.Clone();
            newShaft.SelectedItem = null;
            return (object)newShaft;
        }
    }
}
