using Paint_Clone.ColorSpacesMode.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint_Clone.ColorSpacesMode.Views;

public partial class ColorSpacesView : UserControl
{
    private Point start;
    private double startRotationX, startRotationY;
    private const double cubeEdgeLength = 0.5;
    private const int width = 256, height = 256;
    public ColorSpacesView(ColorSpaceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        BackBrush.ImageSource = GenerateWallTexture(0, 255, 0, 0, 0, 255);
        LeftBrush.ImageSource = GenerateWallTexture(0, 255, 0, 255, 0, 0);
        BottomBrush.ImageSource = GenerateWallTexture(0, 0, 0, 255, 0, 255);
        RightBrush.ImageSource = GenerateWallTexture(0, 255, 0, 255, 255, 255);
        FrontBrush.ImageSource = GenerateWallTexture(0, 255, 255, 255, 0, 255);
        TopBrush.ImageSource = GenerateWallTexture(255, 255, 0, 255, 0, 255);
        TransformIndicator(0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    private WriteableBitmap GenerateWallTexture(int rMin, int rMax, int gMin, int gMax,
            int bMin, int bMax)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        FillWith2DLinearGradient(bmp, rMin, rMax, gMin, gMax, bMin, bMax);
        return bmp;
    }

    private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (CubeViewPort.IsMouseCaptured) return;
        CubeViewPort.CaptureMouse(); 
        start = e.GetPosition(CubeViewPort);
        startRotationX = RotationX.Angle; 
        startRotationY = RotationY.Angle; 
    }

    private void Viewport_MouseMove(object sender, MouseEventArgs e)
    {
        if (!CubeViewPort.IsMouseCaptured) return;
        Point p = e.GetPosition(CubeViewPort);
        RotationY.Angle = startRotationY + (p.X - start.X);
        RotationX.Angle = startRotationX + (p.Y - start.Y);
    }

    private void Viewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        CubeViewPort.ReleaseMouseCapture();
    }

    private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle) return; 
        RotationX.Angle = 0;
        RotationY.Angle = 0;
    }

    private void Viewport_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point mousePos = e.GetPosition(CubeViewPort);
        HitTestResult htres = VisualTreeHelper.HitTest(CubeViewPort, mousePos);
        var res = htres as RayMeshGeometry3DHitTestResult;
        if (res != null)
        {
            var qx = new Quaternion(new Vector3D(1, 0, 0), -RotationX.Angle);
            var qy = new Quaternion(new Vector3D(0, 1, 0), -RotationY.Angle);
            var cen = new Point3D(0, 0, 0);
            var mat = Matrix3D.Identity;
            mat.RotateAtPrepend(qx, cen);
            mat.RotateAtPrepend(qy, cen);
            var p = res.PointHit;
            p = mat.Transform(p);
            unsafe
            {
                var p01 = new Point3D(p.X + 0.5, p.Y + 0.5, p.Z + 0.5);
                const double epsilon = 1.0 / 16.0;
                bool x0 = Math.Abs(p01.X - 0) < epsilon;
                bool x1 = Math.Abs(p01.X - 1) < epsilon;
                bool y0 = Math.Abs(p01.Y - 0) < epsilon;
                bool y1 = Math.Abs(p01.Y - 1) < epsilon;
                bool z0 = Math.Abs(p01.Z - 0) < epsilon;
                bool z1 = Math.Abs(p01.Z - 1) < epsilon;
                if (y0)
                {
                    if (z1 || z0) _0123(p);
                    else if (x1 || x0) _4567(p);
                }
                else if (y1)
                {
                    if (z0 || z1) _0123(p);
                    else if (x0 || x1) _4567(p);
                }
                else if (x1)
                {
                    if (z1 || z0) _891011(p);
                }
                else if (x0)
                {
                    if (z0 || z1) _891011(p);
                }
            }
          
        }
    }

    private void _0123(Point3D click)
    {
        double db = click.X + 0.5;
        byte b = (byte)(db * 255);
        TransformIndicator(1.2, 1.2, 1.2, click.X, 0, 0, 0, 0, 0);
    }

    private void _4567(Point3D click)
    {
        double dg = click.Z + 0.5;
        byte g = (byte)(dg * 255);
        TransformIndicator(1.2, 1.2, 1.2, 0, 0, click.Z, 0, 90, 0);
    }

    private void _891011(Point3D click) 
    {
        double dr = click.Y + 0.5;
        byte r = (byte)(dr * 255);
        TransformIndicator(1.2, 1.2, 1.2, 0, click.Y, 0, 0, 0, 90);
    }

    private void FillWith2DLinearGradient(WriteableBitmap bmp, int rMin, int rMax, int gMin,
        int gMax, int bMin, int bMax)
    {
        bmp.Lock();
        int i = 0;
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int r = rMin; r <= rMax; ++r)
                for (int g = gMin; g <= gMax; ++g)
                    for (int b = bMin; b <= bMax; ++b)
                        p[i++] = (255 << 24) | (r << 16) | (g << 8) | b;
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        bmp.Unlock();
    }

    private void TransformIndicator(double scaleX, double scaleY, double scaleZ,
        double offsetX, double offsetY, double offsetZ,
        double angleX, double angleY, double angleZ)
    {
        IndicatorScale.ScaleX = scaleX;
        IndicatorScale.ScaleY = scaleY;
        IndicatorScale.ScaleZ = scaleZ;
        IndicatorTranslation.OffsetX = offsetX;
        IndicatorTranslation.OffsetY = offsetY;
        IndicatorTranslation.OffsetZ = offsetZ;
        IndicatorRotationX.Angle = angleX;
        IndicatorRotationY.Angle = angleY;
        IndicatorRotationZ.Angle = angleZ;
    }


}
