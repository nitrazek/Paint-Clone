﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Paint_Clone.BasicDrawingMode.Interfaces;

namespace Paint_Clone.BasicDrawingMode.Models;

public class StraightLine : IDrawableShape
{
    public Shape Draw(Point startPoint, Point endPoint, int brushSize, Shape? shape, string text)
    {
        Line straightLine = new Line
        {
            Stroke = Brushes.Black,
            StrokeThickness = brushSize,
            X1 = startPoint.X,
            Y1 = startPoint.Y,
            X2 = endPoint.X,
            Y2 = endPoint.Y,
            IsHitTestVisible = false,
        };

        return straightLine;
    }
}
