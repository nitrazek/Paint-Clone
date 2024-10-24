using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using Paint_Clone.BasicDrawingMode.Interfaces;


namespace Paint_Clone.BasicDrawingMode.Models
{
    public class Text : IDrawableShape
    {
        public Shape Draw(Point startPoint, Point endPoint, int brushSize, Shape? shape, string text)
        {
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Comic Sans MS"),
                brushSize * 15,
                Brushes.Black,
                96);

            Geometry textGeometry = formattedText.BuildGeometry(startPoint);
            RectangleGeometry clip = new RectangleGeometry(new Rect(startPoint, endPoint));

            Path textPath = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = brushSize/3,
                Data = textGeometry,
                Clip = clip,
                Fill = Brushes.Black
            };

            return textPath;
        }
    }
}