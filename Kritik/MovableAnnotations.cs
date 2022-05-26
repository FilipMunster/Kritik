using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Annotations;

namespace Kritik
{
    class MovableAnnotations<T> : IEnumerable<T> where T : TextualAnnotation
    {
        private readonly List<T> items = new List<T>();

        private T selectedAnnotation;
        private DataPoint initialMousePosition = new DataPoint();
        private DataPoint initialAnnotationPosition = new DataPoint();

        public T this[int i] => items[i];
        public int Count => items.Count;
        
        public void Add(T annotation)
        {
            annotation.MouseDown += Annotation_MouseDown;
            items.Add(annotation);
        }
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public void Clear()
        {
            items.Clear();
        }

        private void Annotation_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            this.selectedAnnotation = (T)sender;
        }

        public void MoveAnnotation(OxyMouseEventArgs e, PlotModel parentModel, bool moveX, bool moveY)
        {
            if (selectedAnnotation is null)
                return;

            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                this.selectedAnnotation = null;
                this.initialMousePosition = new DataPoint();
                this.initialAnnotationPosition = new DataPoint();
                return;
            }

            DataPoint mousePosition = OxyPlot.Axes.Axis.InverseTransform(e.Position, parentModel.Axes[0], parentModel.Axes[1]);
            DataPoint annotationPosition = selectedAnnotation.TextPosition;
            if (this.initialMousePosition.Y == 0)
            {
                this.initialMousePosition = mousePosition;
                this.initialAnnotationPosition = annotationPosition;
            }

            double x = initialAnnotationPosition.X;
            double y = initialAnnotationPosition.Y;

            if (moveX)
                x += mousePosition.X - this.initialMousePosition.X;
            if (moveY)
                y += mousePosition.Y - this.initialMousePosition.Y;
            
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                foreach (T item in items)
                {
                    if (moveX & !moveY)
                        item.TextPosition = new DataPoint(x, item.TextPosition.Y);
                    else if (!moveX & moveY)
                        item.TextPosition = new DataPoint(item.TextPosition.X, y);
                }
            }
            else
                selectedAnnotation.TextPosition = new DataPoint(x, y);

            parentModel.InvalidatePlot(true);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
