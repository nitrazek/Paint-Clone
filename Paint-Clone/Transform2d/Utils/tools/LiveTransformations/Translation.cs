using Paint_Clone.Transform2d.Views;
using System.Windows;

namespace Paint_Clone.Transform2d.Utils.tools.LiveTransformations;

public class Translation : LiveTransformation
{
    public override void MouseMoveTo(Point point)
    {
        if (!isDragged) return;
        Vector startToPoint = point - mouseStart;
        var win = Transform2dView.Instance;
        var pol = win.SelectedPolygon;
        win.Cover();
        pol.RestoreVerticesFrom(startVertices);
        pol.Translate(startToPoint.X, startToPoint.Y);
        win.Draw();
    }

    public override void LeftMouseDown(Point point)
    {
        var win = Transform2dView.Instance;
        if (win.SelectedPolygonIndex == -1) return;
        if (isDragged) return;
        mouseStart = point;
        startVertices = win.SelectedPolygon.CloneVertices();
        isDragged = true;
    }
}
