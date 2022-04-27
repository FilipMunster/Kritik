using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kritik
{
    public class Shaft : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private ShaftProperties properties;
        public ShaftProperties Properties
        {
            get => properties;
            set
            {
                properties = value;
                NotifyPropertyChanged();
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
        public ShaftElementForDataGrid AddElement(ShaftElementForDataGrid selectedElement)
        {
            int index = Elements.IndexOf(selectedElement);
            if (selectedElement is null || index < 0)
            {
                Elements.Add(new ShaftElementForDataGrid());
                return Elements.Last();
            }

            Elements.Insert(index, new ShaftElementForDataGrid());
            return Elements[index];
        }
        /// <summary>
        /// Removes selected element from the collection
        /// </summary>
        /// <param name="selectedElement">Selected element of shaft</param>
        /// <returns>New selected element</returns>
        public ShaftElementForDataGrid RemoveSelectedElement(ShaftElementForDataGrid selectedElement)
        {
            if (selectedElement is null)
                return null;

            int index = Elements.IndexOf(selectedElement);
            Elements.Remove(selectedElement);
            if (index == Elements.Count)
                index--;
            return index >= 0 ? Elements[index] : null;
        }
        /// <summary>
        /// Moves selected element one row up.
        /// </summary>
        public void MoveElementUp(ShaftElementForDataGrid selectedElement)
        {
            if (selectedElement is null)
                return;

            int selectedItemIndex = Elements.IndexOf(selectedElement);
            if (selectedItemIndex > 0)
                Elements.Move(selectedItemIndex, selectedItemIndex - 1);
        }

        /// <summary>
        /// Moves selected element one row down.
        /// </summary>
        public void MoveElementDown(ShaftElementForDataGrid selectedElement)
        {
            if (selectedElement is null)
                return;

            int selectedItemIndex = Elements.IndexOf(selectedElement);
            if (selectedItemIndex < (Elements.Count - 1))
                Elements.Move(selectedItemIndex, selectedItemIndex + 1);
        }

        /// <summary>
        /// Mirrors the shaft
        /// </summary>
        /// <returns>New selected element</returns>
        public ShaftElementForDataGrid Mirror()
        {
            int count = Elements.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                Elements.Add((ShaftElementForDataGrid)Elements[i].Clone());
            }

            return Elements[count];
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
        public List<ShaftElementWithMatrix> GetElementsWithMatrix()
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

        public object Clone()
        {
            Shaft newShaft = (Shaft)this.MemberwiseClone();
            newShaft.Elements = this.Elements.Clone();
            newShaft.Properties = (ShaftProperties)this.Properties.Clone();
            return (object)newShaft;
        }
    }
}
