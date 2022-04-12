using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{
    public static class ObservableCollectionExtensionMethods
    {
        /// <summary>
        /// Creates a new <see cref="ObservableCollection{T}"/> that is copy of the current instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns>A new <see cref="ObservableCollection{T}"/> that is a copy of this instance.</returns>
        public static ObservableCollection<T> Clone<T>(this ObservableCollection<T> collection) where T : ICloneable
        {
            ObservableCollection<T> collectionClone = new ObservableCollection<T>();
            foreach (T item in collection)
            {
                collectionClone.Add((T)item.Clone());
            }
            return collectionClone;
        }
    }
}
