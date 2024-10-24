using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using Paint_Clone.BasicDrawingMode.Interfaces;

namespace Paint_Clone.BasicDrawingMode.Models
{
    public class FreeHandLine : IDrawableShape
    {
        public Shape Draw(Point startPoint, Point endPoint, int brushSize, Shape? shape, string text)
        {
            if (shape != null && shape is Polyline oldPolyline)
            {
                Polyline polyline = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = brushSize,
                    Points = oldPolyline.Points.Clone(),
                    IsHitTestVisible = false
                };
                polyline.Points.Add(endPoint);

                return polyline;
            }
            else
            {
                Polyline polyline = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = brushSize,
                    Points = new PointCollection()
                    {
                        startPoint,
                        endPoint
                    },
                    IsHitTestVisible = false
                };

                return polyline;
            }
        }
    }
}
