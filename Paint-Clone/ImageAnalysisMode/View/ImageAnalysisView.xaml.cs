using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Paint_Clone.ImageAnalysisMode.View;

public partial class ImageAnalysisView : UserControl
{
    private Bitmap bitmap;
    public ImageAnalysisView()
    {
        InitializeComponent();
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            bitmap = new Bitmap(openFileDialog.FileName);

            Image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            TotalGreenAreaTextBox.Text = "";
            LargestGreenAreaTextBox.Text = "";
        }
    }

    private void AnalyzeImage_Click(object sender, RoutedEventArgs e)
    {
        if (bitmap == null)
        {
            MessageBox.Show("Please load an image first.");
            return;
        }

        double greenPercentage = CalculateGreenPercentage(bitmap);

        int largestGreenArea = FindLargestGreenArea(bitmap);

        TotalGreenAreaTextBox.Text = greenPercentage.ToString("F2") + " %";
        LargestGreenAreaTextBox.Text = largestGreenArea.ToString() + " pixels";
    }

    private double CalculateGreenPercentage(Bitmap bitmap)
    {
        int greenPixels = 0;
        int totalPixels = bitmap.Width * bitmap.Height;

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);

                if (pixel.G > 100 && pixel.G > pixel.R && pixel.G > pixel.B)
                {
                    greenPixels++;
                }
            }
        }

        return (double)greenPixels / totalPixels * 100;
    }

    private int FindLargestGreenArea(Bitmap bitmap)
    {
        int[,] binaryImage = new int[bitmap.Width, bitmap.Height];

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);

                binaryImage[x, y] = (pixel.G > 100 && pixel.G > pixel.R && pixel.G > pixel.B) ? 1 : 0;
            }
        }

        bool[,] visited = new bool[bitmap.Width, bitmap.Height];
        int maxArea = 0;

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (binaryImage[x, y] == 1 && !visited[x, y])
                {
                    int area = GetAreaSize(binaryImage, visited, x, y);
                    maxArea = Math.Max(maxArea, area);
                }
            }
        }

        return maxArea;
    }

    private int GetAreaSize(int[,] binaryImage, bool[,] visited, int startX, int startY)
    {
        int width = binaryImage.GetLength(0);
        int height = binaryImage.GetLength(1);

        int areaSize = 0;
        var stack = new Stack<(int x, int y)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            if (x < 0 || y < 0 || x >= width || y >= height || visited[x, y] || binaryImage[x, y] == 0)
            {
                continue;
            }

            visited[x, y] = true;
            areaSize++;

            stack.Push((x + 1, y));
            stack.Push((x - 1, y));
            stack.Push((x, y + 1));
            stack.Push((x, y - 1));
        }

        return areaSize;
    }
}
