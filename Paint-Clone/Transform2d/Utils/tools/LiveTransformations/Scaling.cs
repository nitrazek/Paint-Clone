using Paint_Clone.Transform2d.Views;
using System.Windows;

namespace Paint_Clone.Transform2d.Utils.tools.LiveTransformations;

public class Scaling : LiveTransformation
{
    public override void MouseMoveTo(Point point)
    {
        if (!isDragged) return;
        var win = Transform2dView.Instance;
        var center = win.TransformationPoint;
        Vector centerToStart = center.Subtract(mouseStart);
        Vector centerToPoint = center.Subtract(point);
        var pol = win.SelectedPolygon;
        win.Cover();
        pol.RestoreVerticesFrom(startVertices);
        pol.Scale(center.X, center.Y,
            centerToPoint.X / centerToStart.X, centerToPoint.Y / centerToStart.Y);
        win.Draw();
    }
}
