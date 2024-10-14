using CommunityToolkit.Mvvm.ComponentModel;
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
        [ObservableProperty]
        int brushSize = 2;
        readonly Dictionary<DrawingModes, IDrawableShape> shapeDrawers;
        Point? startPoint;
        Point? lastMovementPoint;

        public MainWindowViewModel()
        {
            shapeDrawers = new Dictionary<DrawingModes, IDrawableShape>
            {
                { DrawingModes.Triangle, new Triangle() },
                { DrawingModes.Square, new Square() },
                { DrawingModes.StraightLine, new StraightLine() },
                { DrawingModes.Elipse, new Elipse() },

            };
        }

        [RelayCommand]
        void ChangeDrawingMode(DrawingModes newDrawingMode) { CurrentDrawingMode = newDrawingMode; }
        [RelayCommand]
        void IncreaseBrushSize() { if (BrushSize < 9) BrushSize++; }
        [RelayCommand]
        void DecreaseBrushSize() { if (BrushSize > 1) BrushSize--; }

        public void StartDrawing(Point newPoint)
        {
            startPoint = newPoint;
        }

        public Shape? DrawPreviewShape(Point newPoint)
        {
            if (startPoint == null)
                return null;

            if (!shapeDrawers.TryGetValue(CurrentDrawingMode, out var shapeDrawer))
                return null;

            return shapeDrawer.Draw(startPoint.Value, newPoint, BrushSize);
        }

        public Shape? EndPreview(Point endPoint, out Rectangle? shapeFrame)
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
            Shape shape = shapeDrawer.Draw(startPoint.Value, endPoint, BrushSize);

            return shape;
        }

        public void StartMoving(Point newPoint)
        {
            if (startPoint == null) return;
            lastMovementPoint = newPoint;
        }

        public void MoveShape(Shape shape, Point newMousePosition)
        {
            if (lastMovementPoint == null || startPoint == null) return;

            double offsetX = newMousePosition.X - lastMovementPoint.Value.X;
            double offsetY = newMousePosition.Y - lastMovementPoint.Value.Y;

            if (shape is Polygon polygon)
            {
                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    Point point = polygon.Points[i];
                    polygon.Points[i] = new Point(point.X + offsetX, point.Y + offsetY);
                }
            }
            else
            {
                Canvas.SetLeft(shape, Canvas.GetLeft(shape) + offsetX);
                Canvas.SetTop(shape, Canvas.GetTop(shape) + offsetY);
            }

            startPoint = new Point(startPoint.Value.X + offsetX, startPoint.Value.Y + offsetY);
            lastMovementPoint = newMousePosition;
        }

        public Rectangle? EndMoving(Shape shape, Point newMousePosition)
        {
            if (shape == null || startPoint == null || lastMovementPoint == null) return null;
            MoveShape(shape, newMousePosition);

            Rectangle? shapeFrame = DrawShapeFrame(new Point(startPoint.Value.X + shape.ActualWidth, startPoint.Value.Y + shape.ActualHeight));
            lastMovementPoint = null;
            return shapeFrame;
        }

        public void EndDrawing()
        {
            startPoint = null;
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