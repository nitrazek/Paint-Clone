﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Paint_Clone.enums;
using Paint_Clone.models;
using Paint_Clone.viewmodels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint_Clone.viewmodels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        DrawingModes currentDrawingMode = DrawingModes.FreeHand;
        readonly Dictionary<DrawingModes, IDrawableShape> shapeDrawers;
        Point? startPoint;

        public MainWindowViewModel()
        {
            shapeDrawers = new Dictionary<DrawingModes, IDrawableShape>
            {
                { DrawingModes.Triangle, new Triangle() },
                { DrawingModes.Square, new Square() },
                { DrawingModes.StraightLine, new StraightLine() },
                { DrawingModes.Elipse, new Elipse() }
            };
        }

        [RelayCommand]
        void ChangeDrawingMode(DrawingModes newDrawingMode)
        {
            CurrentDrawingMode = newDrawingMode;
        }
        
        public void SaveImage(UIElementCollection canvasElements)
        {
            
        }

        public void SetStartingPosition(Point startPoint)
        {
            this.startPoint = startPoint;
        }

        public Shape? DrawFinalShape(Point endPoint, out Rectangle? shapeFrame)
        {
            shapeFrame = null;
            if (startPoint == null || startPoint.Equals(endPoint))
                return null;

            if (!shapeDrawers.TryGetValue(CurrentDrawingMode, out var shapeDrawer))
            {
                startPoint = null;
                return null;
            }

            shapeFrame = DrawShapeFrame(endPoint);
            Shape shape = shapeDrawer.Draw(startPoint.Value, endPoint);

            startPoint = null;
            return shape;
        }

        public Shape? DrawPreviewShape(Point newPoint)
        {
            if (startPoint == null)
                return null;

            if (!shapeDrawers.TryGetValue(CurrentDrawingMode, out var shapeDrawer))
                return null;

            return shapeDrawer.Draw(startPoint.Value, newPoint);
        }

        private Rectangle? DrawShapeFrame(Point endPoint)
        {
            if (startPoint == null || startPoint.Equals(endPoint))
                return null;

            
            double x1 = Math.Min(startPoint.Value.X, endPoint.X);
            double y1 = Math.Min(startPoint.Value.Y, endPoint.Y);
            double width = Math.Abs(startPoint.Value.X - endPoint.X);
            double height = Math.Abs(startPoint.Value.Y - endPoint.Y);

            Rectangle frame = new Rectangle
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 2 },
                Width = width + 8,
                Height = height + 8,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(frame, Math.Min(startPoint.Value.X, endPoint.X) - 4);
            Canvas.SetTop(frame, Math.Min(startPoint.Value.Y, endPoint.Y) - 4);

            return frame;
        }
    }
}