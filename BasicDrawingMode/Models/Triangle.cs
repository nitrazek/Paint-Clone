using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Paint_Clone.BasicDrawingMode.Interfaces;

namespace Paint_Clone.BasicDrawingMode.Models
{
    public class Triangle : IDrawableShape
    {
        public Shape Draw(Point startPoint, Point endPoint, int brushSize, Shape? shape, string text)
        {
            Polygon triangle = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = brushSize,
                Points = new PointCollection
                {
                    startPoint,
                    new Point(startPoint.X, endPoint.Y),
                    endPoint
                },
                IsHitTestVisible = false
            };

            return triangle;
        }
    }
}
