using Paint_Clone.Transform2d.Views;
using System.Windows;

namespace Paint_Clone.Transform2d.Utils.tools.LiveTransformations;

public abstract class LiveTransformation : Tool
{
    protected Point[] startVertices;
    protected Point mouseStart;

    public override void LeftMouseDown(Point point)
    {
        var win = Transform2dView.Instance;
        if (win.SelectedPolygonIndex == -1) return;
        mouseStart = point;
        startVertices = win.SelectedPolygon.CloneVertices();
        isDragged = true;
    }
}
