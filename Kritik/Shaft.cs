using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    class Shaft
    {
        public ObservableCollection<ShaftElementForDataGrid> Elements { get; set; }
        public ShaftElementForDataGrid SelectedItem { get; set; }

        /// <summary>
        /// Creates shaft with Elements
        /// </summary>
        /// <param name="shaftElements">List of ShaftElements (works with its copy)</param>
        public Shaft(List<ShaftElement> shaftElements)
        {
            Elements = new ObservableCollection<ShaftElementForDataGrid>();
            foreach (var shaftElement in shaftElements)
            {
                Elements.Add(new ShaftElementForDataGrid(shaftElement));
            }
        }
        /// <summary>
        /// Creates shaft with empty collection of Elements
        /// </summary>
        public Shaft()
        {
            Elements = new ObservableCollection<ShaftElementForDataGrid>();
        }
        public List<ShaftElementWithMatrix> GetElementsWithMatrix(GyroscopicEffect gyros, double youngModulus, double materialDensity, bool shaftRotationInfluence, double shaftRPM)
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
                                Cm = cmi,
                            });
                        }
                    }
                    elementsWithMatrix.Add(new ShaftElementWithMatrix(ElementType.beam) // nakonec přidat ještě jeden prvek typu beam
                    {
                        L = li,
                        De = element.De,
                        Di = element.Di,
                    });
                }
                else
                {
                    elementsWithMatrix.Add(new ShaftElementWithMatrix(element));
                }
            }

            // nakonec nastavit všem prvkům společné vlastnosti
            foreach (var element in elementsWithMatrix)
            {
                element.Gyros = gyros;
                element.E = youngModulus;
                element.Rho = materialDensity;
                element.ShaftRotationInfluence = shaftRotationInfluence;
                element.ShaftRPM = shaftRPM;
            }
            return elementsWithMatrix;
        }
    }
}
