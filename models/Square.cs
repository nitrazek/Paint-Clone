using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint_Clone.models
{
    public class Square : IDrawableShape
    {
        public Shape Draw(Point startPoint, Point endPoint, int brushSize, Shape? shape, string text)
        {
            Polygon square = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = brushSize,
                Points = new PointCollection
                {
                    startPoint,
                    new Point(startPoint.X, endPoint.Y),
                    endPoint,
                    new Point(endPoint.X, startPoint.Y)
                },
                IsHitTestVisible = false
            };

            return square;
        }
    }
}