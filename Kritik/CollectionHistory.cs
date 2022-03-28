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
    public class CollectionHistory<T>
    {
        private ObservableCollection<T> collection;
        private List<ObservableCollection<T>> history;
        private int position;
        private const int maxHistoryCount = 33;

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
        /// Adds current state of collection to history
        /// </summary>
        public void Add()
        {
            if (position < (history.Count - 1))
                history.RemoveRange(position + 1, history.Count - position - 1);
            if (history.Count == maxHistoryCount)
                history.RemoveAt(0);

            history.Add(new ObservableCollection<T>(collection));
            position = history.Count - 1;
        }
        /// <summary>
        /// Returns previous state of collection
        /// </summary>
        /// <returns>New collection</returns>
        public ObservableCollection<T> Back()
        {
            if (position > 0)
                position--;

            return new ObservableCollection<T>(history[position]);
        }
        /// <summary>
        /// Returns next state of collection
        /// </summary>
        /// <returns>New collection</returns>
        public ObservableCollection<T> Forward()
        {
            if (position < (history.Count - 1))
                position++;

            return new ObservableCollection<T>(history[position]);
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
