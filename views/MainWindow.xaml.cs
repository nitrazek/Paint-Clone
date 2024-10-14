using Paint_Clone.enums;
using Paint_Clone.models;
using Paint_Clone.viewmodels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint_Clone
{
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel = new();
        Shape? previewShape = null;
        Rectangle? shapeFrame = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void PaintSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point cursorPosition = e.GetPosition(PaintSurface);

            if (shapeFrame != null &&
                cursorPosition.X >= Canvas.GetLeft(shapeFrame) &&
                cursorPosition.Y >= Canvas.GetTop(shapeFrame) &&
                cursorPosition.X <= Canvas.GetLeft(shapeFrame) + shapeFrame.Width &&
                cursorPosition.Y <= Canvas.GetTop(shapeFrame) + shapeFrame.Height)
            {

            }
            if (shapeFrame != null)
            {
                PaintSurface.Children.Remove(shapeFrame);
                shapeFrame = null;
            }

            viewModel.SetStartingPosition(cursorPosition);
        }

        private void PaintSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(previewShape != null)
            {
                PaintSurface.Children.Remove(previewShape);
                previewShape = null;
            }

            Point cursorPosition = e.GetPosition(PaintSurface);
            Shape? shape = viewModel.DrawFinalShape(cursorPosition, out shapeFrame);
            if (shape == null || shapeFrame == null) return;

            PaintSurface.Children.Add(shape);
            PaintSurface.Children.Add(shapeFrame);
        }

        private void PaintSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if(previewShape != null)
                PaintSurface.Children.Remove(previewShape);

            Point cursorPosition = e.GetPosition(PaintSurface);
            previewShape = viewModel.DrawPreviewShape(cursorPosition);
            if (previewShape == null) return;
            PaintSurface.Children.Add(previewShape);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SaveImage(PaintSurface.Children);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PaintSurface.Children.Clear();
        }
    }
}