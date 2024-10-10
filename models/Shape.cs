using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Paint_Clone.models
{
    public abstract class Shape
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

    }
}
