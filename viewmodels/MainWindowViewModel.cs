using Microsoft.Win32;
using Paint_Clone.enums;
using Paint_Clone.models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Paint_Clone.viewmodels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private DrawingMode drawingMode;
        private Point startingPoint;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            drawingMode = DrawingMode.Triangle;
        }

        public DrawingMode CurrentDrawingMode
        {
            get => drawingMode;
            set
            {
                if (drawingMode != value)
                {
                    drawingMode = value;
                    OnPropertyChanged(nameof(CurrentDrawingMode)); 
                }
            }
        }

        public void SaveImage(UIElementCollection canvasElements)
        {
            
        }

        public void ChangeDrawingMode(DrawingMode drawingMode)
        {
            CurrentDrawingMode = drawingMode;
        }

        public void SetStartingPosition(Point startingPoint)
        {
            this.startingPoint = startingPoint;
        }

        public void DrawShape(Point newPoint)
        {
            /* 
            Line line = new()
            {
                Stroke = SystemColors.WindowFrameBrush,
                X1 = startingPoint.X,
                Y1 = startingPoint.Y,
                X2 = newPoint.X,
                Y2 = newPoint.Y
            };

            startingPoint = newPoint;
            PaintSurface.Children.Add(line);
            */
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
