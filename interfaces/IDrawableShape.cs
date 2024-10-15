using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Paint_Clone.models
{
    public interface IDrawableShape
    {
        Shape Draw(Point startPoint, Point endPoint, int brushSize, Shape? shape, string text);
    }
}
