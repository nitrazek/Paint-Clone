using Paint_Clone.BasicDrawingMode.ViewModels;
using Paint_Clone.BezierCurveMode.Utils;
using Paint_Clone.BezierCurveMode.Viewmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Paint_Clone.BezierCurveMode.Views;


public partial class BezierCurveView : UserControl, INotifyPropertyChanged
{
    /* BezierCurveViewmodel viewModel;

     public BezierCurveView(BezierCurveViewmodel viewModel)
     {
         InitializeComponent();
         DataContext = this.viewModel = viewModel;
     }*/

    private ObservableCollection<HighlightablePoint> points;
    public ObservableCollection<HighlightablePoint> Points
    {
        get => points;
        private set { points = value; OnPropertyChanged(nameof(Points)); }
    }
    private int draggedPtId;
    public const int POINT_WIDTH = 10, POINT_HEIGHT = 10;
    // im mniej tym dokładniejsza krzywa Beziera, ale liniowo proporcjonalnie dłużej się rysuje
    private double deltaT;
    public string DeltaT
    {
        get => deltaT.ToString();
        set
        {
            if (!double.TryParse(value, out double val))
            {
                MessageBox.Show("Podaj poprawny przyrost t.");
                return;
            }
            if (!(val > 0 && val <= 1))
            {
                MessageBox.Show($"Podaj przyrost t w przedziale ({0},{1}>.");
                return;
            }
            Cover();
            deltaT = val;
            Draw();
            OnPropertyChanged(nameof(DeltaT));
        }
    }

    public BezierCurveView()
    {
        InitializeComponent();
        DataContext = this;
        Image.Source = new WriteableBitmap(670, 400, 96, 96, PixelFormats.Bgra32, null);
        draggedPtId = -1;
        Points = new ObservableCollection<HighlightablePoint>();
        DeltaT = (1.0 / 4096.0).ToString();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var bmp = (WriteableBitmap)Image.Source;
        if (!double.TryParse(XTextBox.Text, out double x))
        {
            MessageBox.Show("Podaj poprawną współrzędną X.");
            return;
        }
        if (!(x >= 0 && x + POINT_WIDTH < bmp.PixelWidth))
        {
            MessageBox.Show($"Podaj współrzędną X w przedziale <{0}," +
                $"{bmp.PixelWidth - POINT_WIDTH}).");
            return;
        }
        if (!double.TryParse(YTextBox.Text, out double y))
        {
            MessageBox.Show("Podaj poprawną współrzędną Y.");
            return;
        }
        if (!(y >= 0 && y + POINT_HEIGHT < bmp.PixelHeight))
        {
            MessageBox.Show($"Podaj współrzędną Y w przedziale <{0}," +
                $"{bmp.PixelHeight - POINT_HEIGHT}).");
            return;
        }
        Cover();
        Points.Add(new HighlightablePoint(x, y, this));
        Draw();
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (draggedPtId != -1) return;
        Point mse = e.GetPosition(Image);
        int mseX = (int)mse.X, mseY = (int)mse.Y;
        int id = 0;
        foreach (var p in points)
        {
            int pX = (int)p.X, pY = (int)p.Y;
            if (mseX >= pX && mseX < pX + POINT_WIDTH &&
                mseY >= pY && mseY < pY + POINT_HEIGHT)
            {
                draggedPtId = id;
                break;
            }
            ++id;
        }
        if (draggedPtId == -1) // kursor nie jest w żadnym punkcie kontrolnym krzywej Beziera
        {
            Cover();
            Points.Add(new HighlightablePoint(mse.X, mse.Y, this)); // dodajemy nowy punkt kontrolny
            Draw();
        }
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        Point mse = e.GetPosition(Image);
        int mseX = (int)mse.X, mseY = (int)mse.Y;
        if (draggedPtId == -1)
        {
            int id = 0;
            foreach (var p in points)
            {
                int pX = (int)p.X, pY = (int)p.Y;
                if (mseX >= pX && mseX < pX + POINT_WIDTH &&
                    mseY >= pY && mseY < pY + POINT_HEIGHT)
                    p.IsHighlighted = true;
                else
                    p.IsHighlighted = false;
                ++id;
            }
            return;
        }
        HighlightablePoint draggedPt = points[draggedPtId];
        var bmp = (WriteableBitmap)Image.Source;
        if (mseX >= 0 && mseX + POINT_WIDTH < bmp.PixelWidth &&
            mseY >= 0 && mseY + POINT_HEIGHT < bmp.PixelHeight)
        {
            Cover();
            draggedPt.X = mse.X;
            draggedPt.Y = mse.Y;
            Draw();
        }
    }

    private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        draggedPtId = -1;
    }

    private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (draggedPtId != -1) return;
        Point mse = e.GetPosition(Image);
        int mseX = (int)mse.X, mseY = (int)mse.Y;
        int id = 0;
        foreach (var p in points)
        {
            int pX = (int)p.X, pY = (int)p.Y;
            if (mseX >= pX && mseX < pX + POINT_WIDTH &&
                mseY >= pY && mseY < pY + POINT_HEIGHT)
            {
                Cover();
                points.RemoveAt(id);
                Draw();
                return;
            }
            ++id;
        }
    }

    private void Redraw(int color)
    {
        foreach (var p in points)
            DrawPointRectangle(p, color);
        if (points.Count >= 2)
            Bezier.DrawWithLines((WriteableBitmap)Image.Source, points, deltaT, color);
    }

    public void Cover()
    {
        var bmp = (WriteableBitmap)Image.Source;
        bmp.Lock();
        Redraw((255 << 24) | (255 << 16) | (255 << 8) | 255); // biały
    }

    public void Draw()
    {
        var bmp = (WriteableBitmap)Image.Source;
        Redraw((255 << 24) | (0 << 16) | (0 << 8) | 0); // czarny
                                                        // bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        bmp.Unlock();
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {

    }

    private void DrawPointRectangle(HighlightablePoint point, int color)
    {
        int xTopLeft = (int)point.X, yTopLeft = (int)point.Y;
        var bmp = (WriteableBitmap)Image.Source;
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            int bmpWid = bmp.PixelWidth, bmpHei = bmp.PixelHeight;
            int yLimit = yTopLeft + POINT_HEIGHT, xLimit = xTopLeft + POINT_WIDTH;
            for (int y = yTopLeft; y < yLimit; ++y)
                for (int x = xTopLeft; x < xLimit; ++x)
                    *(p + x + bmpWid * y) = color;
        }
        bmp.AddDirtyRect(new Int32Rect(xTopLeft, yTopLeft, POINT_WIDTH, POINT_HEIGHT));
    }
}

    

