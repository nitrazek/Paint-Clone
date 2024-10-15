using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint_Clone.models
{
    public class Elipse : IDrawableShape
    {
        public Shape Draw(Point startPoint, Point endPoint, int brushSize, Shape? shape, string text)
        {
            Ellipse elipse = new Ellipse
            {
                Stroke = Brushes.Black,
                StrokeThickness = brushSize,
                Width = Math.Abs(endPoint.X - startPoint.X),
                Height = Math.Abs(endPoint.Y - startPoint.Y),
                IsHitTestVisible = false
            };

            Canvas.SetLeft(elipse, Math.Min(startPoint.X, endPoint.X));
            Canvas.SetTop(elipse, Math.Min(startPoint.Y, endPoint.Y));

            return elipse;
        }
    }
}