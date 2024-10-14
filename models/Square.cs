﻿using System;
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
        public Shape Draw(Point startPoint, Point endPoint)
        {
            Polygon square = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Points = new PointCollection
                {
                    startPoint,
                    new Point(startPoint.X, endPoint.Y),
                    endPoint,
                    new Point(endPoint.X, startPoint.Y)
                }
            };

            return square;
        }
    }
}