using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Paint_Clone.models
{
    public class FreeHand : IDrawableShape
    {
        private Polyline _currentPolyline;

        public Shape Draw(Point startPoint, Point endPoint)
        {
            Line line = new Line
            {

            };

            return line;
        }
    }
}
