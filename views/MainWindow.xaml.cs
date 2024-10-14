using Microsoft.Win32;
using Paint_Clone.enums;
using Paint_Clone.models;
using Paint_Clone.utils;
using Paint_Clone.viewmodels;
using System.IO;
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
        CanvasToImage canvasToImage = new();
        Shape? previewShape = null;
        Shape? finalShape = null;
        Rectangle? shapeFrame = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void PaintSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point cursorPosition = e.GetPosition(PaintSurface);

            if (shapeFrame == null || finalShape == null)
            {
                shapeFrame = null;
                finalShape = null;
                viewModel.StartDrawing(cursorPosition);
            }
            else if (cursorPosition.X >= Canvas.GetLeft(shapeFrame) &&
                    cursorPosition.Y >= Canvas.GetTop(shapeFrame) &&
                    cursorPosition.X <= Canvas.GetLeft(shapeFrame) + shapeFrame.Width &&
                    cursorPosition.Y <= Canvas.GetTop(shapeFrame) + shapeFrame.Height)
            {
                PaintSurface.Children.Remove(shapeFrame);
                shapeFrame = null;
                viewModel.StartMoving(cursorPosition);
            }
            else
            {
                viewModel.EndDrawing();
                PaintSurface.Children.Remove(shapeFrame);
                finalShape = null;
                shapeFrame = null;
            }
        }

        private void PaintSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point cursorPosition = e.GetPosition(PaintSurface);

            if (finalShape == null)
            {
                PaintSurface.Children.Remove(previewShape);
                previewShape = null;

                finalShape = viewModel.EndPreview(cursorPosition, out shapeFrame);
                if (finalShape == null || shapeFrame == null)
                    return;

                PaintSurface.Children.Add(finalShape);
                PaintSurface.Children.Add(shapeFrame);
            }
            else
            {
                shapeFrame = viewModel.EndMoving(finalShape, cursorPosition);
                if (shapeFrame == null) return;

                PaintSurface.Children.Add(shapeFrame);
            }

        }

        private void PaintSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point cursorPosition = e.GetPosition(PaintSurface);
            if (finalShape == null)
            {
                PaintSurface.Children.Remove(previewShape);
                previewShape = null;

                previewShape = viewModel.DrawPreviewShape(cursorPosition);
                if (previewShape == null) return;

                PaintSurface.Children.Add(previewShape);
            }
            else
            {
                viewModel.MoveShape(finalShape, cursorPosition);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Canvas canvas = PaintSurface;
            canvas.Background = Brushes.White;
            var dialog = new SaveFileDialog();
            dialog.Title = "Zapisz";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = "PNG (*.png)|*.png";
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() != true) return;
            canvasToImage.SaveCanvasToImage(canvas, dialog.FileName);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PaintSurface.Children.Clear();
        }
    }
}