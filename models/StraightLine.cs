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
    public class StraightLine : IDrawableShape
    {
        public Shape Draw(Point startPoint, Point endPoint, int brushSize)
        {
            Polygon straightLine = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = brushSize,
                Points = new PointCollection
                {
                    startPoint,
                    endPoint
                },
                IsHitTestVisible = false
            };

            return straightLine;
        }
    }
}
