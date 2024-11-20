using Paint_Clone.FileFormatsMode.Utils;
using Paint_Clone.FileFormatsMode.Viewmodels;
using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint_Clone.FileFormatsMode.Views;

public partial class FileFormatsView : UserControl
{
    TaskQueue taskQueue;
    FileFormatsViewModel viewModel;
    Point origin;
    Point start;

    public FileFormatsView(FileFormatsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this.viewModel = viewModel;
        taskQueue = new(Dispatcher);
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        taskQueue.EnqueueTask(viewModel.LoadFile);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        taskQueue.EnqueueTask(viewModel.SaveFile);
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        Point p = e.GetPosition(Image);
        Matrix m = Image.RenderTransform.Value;
        if (e.Delta > 0)
            m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
        else
            m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

        Image.RenderTransform = new MatrixTransform(m);
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        Point mousRelToImg = e.GetPosition(Image);
        var imgSrc = (BitmapSource)Image.Source;
        int wid = imgSrc.PixelWidth, hei = imgSrc.PixelHeight;

        if (!Image.IsMouseCaptured) return;
        Point p = e.GetPosition(Border);
        Matrix m = Image.RenderTransform.Value;
        m.OffsetX = origin.X + (p.X - start.X);
        m.OffsetY = origin.Y + (p.Y - start.Y);
        Image.RenderTransform = new MatrixTransform(m);
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Image.IsMouseCaptured) return;
        Image.CaptureMouse();
        start = e.GetPosition(Border);
        var mat = Image.RenderTransform.Value;
        origin.X = mat.OffsetX;
        origin.Y = mat.OffsetY;
    }

    private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Image.ReleaseMouseCapture();
    }
}
