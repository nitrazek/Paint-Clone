using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Paint_Clone.BasicDrawingMode.Enums;
using Paint_Clone.BasicDrawingMode.Interfaces;
using Paint_Clone.BasicDrawingMode.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Paint_Clone.BasicDrawingMode.ViewModels;

public partial class BasicDrawingViewModel : ObservableObject
{
    [ObservableProperty]
    DrawingModes currentDrawingMode = DrawingModes.FreeHand;
    [ObservableProperty]
    int brushSize = 2;
    [ObservableProperty]
    string textField = string.Empty;
    readonly Dictionary<DrawingModes, IDrawableShape> shapeDrawers;
    Point? startPoint;
    Point? lastMovementPoint;

    public BasicDrawingViewModel()
    {
        shapeDrawers = new Dictionary<DrawingModes, IDrawableShape>
            {
                { DrawingModes.Triangle, new Triangle() },
                { DrawingModes.Square, new Square() },
                { DrawingModes.StraightLine, new StraightLine() },
                { DrawingModes.Elipse, new Elipse() },
                { DrawingModes.FreeHand, new FreeHandLine() },
                { DrawingModes.Text, new Text() }
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

    public Shape? DrawPreviewShape(Point newPoint, Shape previousShape)
    {
        if (startPoint == null)
            return null;

        if (!shapeDrawers.TryGetValue(CurrentDrawingMode, out var shapeDrawer))
            return null;

        return shapeDrawer.Draw(startPoint.Value, newPoint, BrushSize, previousShape, TextField);
    }

    public Shape? EndPreview(Point endPoint, Shape previousShape, out Rectangle? shapeFrame)
    {
        shapeFrame = null;
        if (startPoint == null || startPoint.Equals(endPoint))
            return null;

        if (!shapeDrawers.TryGetValue(CurrentDrawingMode, out var shapeDrawer))
        {
            startPoint = null;
            return null;
        }

        Shape shape = shapeDrawer.Draw(startPoint.Value, endPoint, BrushSize, previousShape, TextField);
        shapeFrame = DrawShapeFrame(shape);

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
        else if (shape is Polyline polyline)
        {
            for (int i = 0; i < polyline.Points.Count; i++)
            {
                Point point = polyline.Points[i];
                polyline.Points[i] = new Point(point.X + offsetX, point.Y + offsetY);
            }
        }
        else if (shape is Path path)
        {
            var geometry = path.Data;
            if (geometry != null)
            {
                var translateTransform = path.RenderTransform as TranslateTransform ?? new TranslateTransform();
                translateTransform.X += offsetX;
                translateTransform.Y += offsetY;
                path.RenderTransform = translateTransform;
            }
        }
        else if (shape is Line line)
        {
            line.X1 += offsetX;
            line.Y1 += offsetY;
            line.X2 += offsetX;
            line.Y2 += offsetY;
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
        Rectangle? shapeFrame = DrawShapeFrame(shape);

        lastMovementPoint = null;
        return shapeFrame;
    }

    public void EndDrawing()
    {
        startPoint = null;
        lastMovementPoint = null;
    }

    private Rectangle? DrawShapeFrame(Shape shape)
    {
        double minX, minY, maxX, maxY;

        if (shape is Polygon polygon)
        {
            if (polygon.Points.Count == 0) return null;

            minX = maxX = polygon.Points[0].X;
            minY = maxY = polygon.Points[0].Y;

            foreach (var point in polygon.Points)
            {
                if (point.X < minX) minX = point.X;
                if (point.X > maxX) maxX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.Y > maxY) maxY = point.Y;
            }
        }
        else if (shape is Polyline polyline)
        {
            if (polyline.Points.Count == 0) return null;

            minX = maxX = polyline.Points[0].X;
            minY = maxY = polyline.Points[0].Y;

            foreach (var point in polyline.Points)
            {
                if (point.X < minX) minX = point.X;
                if (point.X > maxX) maxX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.Y > maxY) maxY = point.Y;
            }
        }
        else if (shape is Path path)
        {
            if (path.Data == null) return null;

            Rect bounds = path.Data.Bounds;

            if (path.RenderTransform is TranslateTransform translateTransform)
            {
                bounds.X += translateTransform.X;
                bounds.Y += translateTransform.Y;
            }

            minX = bounds.X;
            minY = bounds.Y;
            maxX = bounds.X + bounds.Width;
            maxY = bounds.Y + bounds.Height;
        }
        else if (shape is Line line)
        {
            minX = Math.Min(line.X1, line.X2);
            maxX = Math.Max(line.X1, line.X2);
            minY = Math.Min(line.Y1, line.Y2);
            maxY = Math.Max(line.Y1, line.Y2);
        }
        else
        {
            minX = Canvas.GetLeft(shape);
            minY = Canvas.GetTop(shape);
            maxX = minX + shape.Width;
            maxY = minY + shape.Height;
        }

        double width = maxX - minX;
        double height = maxY - minY;

        Rectangle frame = new Rectangle
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 2 },
            Width = width + 8,
            Height = height + 8,
            IsHitTestVisible = false
        };

        Canvas.SetLeft(frame, minX - 4);
        Canvas.SetTop(frame, minY - 4);

        return frame;
    }
}