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
        Point currentPoint = new Point();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void paintSurface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed) return;
            
            currentPoint = e.GetPosition(paintSurface);
        }

        private void paintSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            
            Line line = new();

            line.Stroke = SystemColors.WindowFrameBrush;
            line.X1 = currentPoint.X;
            line.Y1 = currentPoint.Y;
            line.X2 = e.GetPosition(paintSurface).X;
            line.Y2 = e.GetPosition(paintSurface).Y;

            currentPoint = e.GetPosition(paintSurface);

            paintSurface.Children.Add(line);
        }
    }
}