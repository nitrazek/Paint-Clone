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

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void PaintSurface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed) return;

            viewModel.SetStartingPosition(e.GetPosition(PaintSurface));
        }

        private void PaintSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            viewModel.DrawShape(e.GetPosition(PaintSurface));
        }

        private void DrawTriangle_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeDrawingMode(DrawingMode.Triangle);
        }

        private void DrawRectangle_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeDrawingMode(DrawingMode.Rectangle);
        }

        private void DrawElipse_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeDrawingMode(DrawingMode.Elipse);
        }

        private void DrawLine_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeDrawingMode(DrawingMode.Line);
        }

        private void DrawFreeHand_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeDrawingMode(DrawingMode.FreeHand);
        }

        private void DrawText_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ChangeDrawingMode(DrawingMode.Text);
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