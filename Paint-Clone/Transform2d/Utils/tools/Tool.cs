using System.Windows;

namespace Paint_Clone.Transform2d.Utils.tools
{
    public abstract class Tool
    {
        protected bool isDragged;

        public abstract void MouseMoveTo(Point p);
        public abstract void LeftMouseDown(Point p);

        public virtual void LeftMouseUp(Point p)
        {
            if (!isDragged) return;
            isDragged = false;
        }

        public virtual void RightMouseDown(Point p) { }
        public virtual void RightMouseUp(Point p) { }
    }
}
