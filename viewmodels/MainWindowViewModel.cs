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
    public class MainWindowViewModel
    {
        DrawingMode drawingMode;
        Point startingPoint;

        public MainWindowViewModel()
        {
            drawingMode = DrawingMode.FreeHand;
        }

        public void SaveImage(UIElementCollection canvasElements)
        {

        }

        public void ChangeDrawingMode(DrawingMode drawingMode)
        {
            this.drawingMode = drawingMode;
        }

        public void SetStartingPosition(Point startingPoint)
        {
            this.startingPoint = startingPoint;
        }

        public void DrawShape(Point newPoint)
        {
            /*Line line = new()
            {
                Stroke = SystemColors.WindowFrameBrush,
                X1 = currentPoint.X,
                Y1 = currentPoint.Y,
                X2 = e.GetPosition(PaintSurface).X,
                Y2 = e.GetPosition(PaintSurface).Y
            };

            currentPoint = e.GetPosition(PaintSurface);

            PaintSurface.Children.Add(line);*/
        }
    }
}