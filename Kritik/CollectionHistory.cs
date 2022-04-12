using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kritik
{    
    /// <summary>
    /// Manages history of ObservableCollection given in constructor
    /// </summary>
    /// <typeparam name="T">Type of elements in ObservableCollection</typeparam>
    public class CollectionHistory<T> where T : ICloneable
    {
        private ObservableCollection<T> collection;
        private List<ObservableCollection<T>> history;
        private int position;
        private const int maxHistoryCount = 50;

        /// <summary>
        /// Manages history of given ObservableCollection
        /// </summary>
        /// <param name="collection">Collection of objects</param>
        public CollectionHistory(ObservableCollection<T> collection)
        {
            this.collection = collection;
            history = new();
        }
        /// <summary>
        /// History length
        /// </summary>
        public int Count => history.Count;
        /// <summary>
        /// Sets new link to collection. Needs to be called after <see cref="Back"/> or <see cref="Forward"/>.
        /// </summary>
        /// <param name="collection">The same collection property as parameter in constructor</param>
        public void SetCollection(ObservableCollection<T> collection)
        {
            this.collection = collection;
        }
        /// <summary>
        /// Adds current state of collection to history
        /// </summary>
        public void Add()
        {
            if (position < (history.Count - 1))
                history.RemoveRange(position + 1, history.Count - position - 1);
            if (history.Count == maxHistoryCount)
                history.RemoveAt(0);

            history.Add(collection.Clone());
            position = history.Count - 1;
        }
        /// <summary>
        /// Returns previous state of collection. Note: Its necessary to subsequently call <see cref="SetCollection(ObservableCollection{T})"/> method!
        /// </summary>
        /// <returns>New collection</returns>
        public ObservableCollection<T> Back()
        {
            if (position > 0)
                position--;

            return history[position].Clone();
        }
        /// <summary>
        /// Returns next state of collection. Note: Its necessary to subsequently call <see cref="SetCollection(ObservableCollection{T})"/> method!
        /// </summary>
        /// <returns>New collection</returns>
        public ObservableCollection<T> Forward()
        {
            if (position < (history.Count - 1))
                position++;

            return history[position].Clone();
        }

        /// <summary>
        /// Checks if it is possible to go back in history
        /// </summary>
        public bool CanGoBack()
        {
            return position > 0;
        }
        /// <summary>
        /// Checks if it is possible to go forward in history
        /// </summary>
        public bool CanGoForward()
        {
            return position < (history.Count - 1);
        }

    }
}
