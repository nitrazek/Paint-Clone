using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Paint_Clone.FileFormatsMode.Utils;
using System.Windows.Media;
using Paint_Clone.DigitalFiltersMode.Enums;
using Paint_Clone.ColorSpacesMode.Utils;

namespace Paint_Clone.DigitalFiltersMode.Viewmodels;

public partial class DigitalFiltersViewModel : ObservableObject
{
    string initialDirectory = Directory.GetCurrentDirectory();
    readonly static int RGB_MIN = 0;
    readonly static int RGB_MAX = 255;
    readonly static int BRIGHTNESS_MIN = -100;
    readonly static int BRIGHTNESS_MAX = 100;

    [ObservableProperty]
    FilterMode currentFilterMode = FilterMode.Addition;

    [ObservableProperty]
    ImageSource imageBitmapSource = new BitmapImage();

    [ObservableProperty]
    int imageWidth;

    [ObservableProperty]
    int imageHeight;

    [ObservableProperty]
    int imageMaxWidth = 1070;

    [ObservableProperty]
    int imageMaxHeight = 675;

    [ObservableProperty]
    int rgbRed;

    [ObservableProperty]
    int rgbGreen;

    [ObservableProperty]
    int rgbBlue;

    [ObservableProperty]
    int brightness;

    [ObservableProperty]
    SolidColorBrush previewColor = new SolidColorBrush(Colors.Black);

    [RelayCommand]
    void ChangeFilterMode(FilterMode newFilterMode) { CurrentFilterMode = newFilterMode; }

    partial void OnRgbRedChanged(int value) { RgbRed = ValidateValue(value, RGB_MIN, RGB_MAX); UpdateFromRGB(); }
    partial void OnRgbGreenChanged(int value) { RgbGreen = ValidateValue(value, RGB_MIN, RGB_MAX); UpdateFromRGB(); }
    partial void OnRgbBlueChanged(int value) { RgbBlue = ValidateValue(value, RGB_MIN, RGB_MAX); UpdateFromRGB(); }
    partial void OnBrightnessChanged(int value) { Brightness = ValidateValue(value, BRIGHTNESS_MIN, BRIGHTNESS_MAX);}

    int ValidateValue(int value, int minValue, int maxValue)
    {
        if (value < minValue) return minValue;
        if (value > maxValue) return maxValue;
        return value;
    }

    void UpdateFromRGB()
    {
        PreviewColor.Color = Color.FromScRgb(1, (float)RgbRed / 255, (float)RgbGreen / 255, (float)RgbBlue / 255);
    }


    public void LoadFile()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Pliki graficzne|*.png;*.jpg;*.jpeg;*.bmp"
        };

        if (openFileDialog.ShowDialog() != true) return;

        try
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            SetImageSource(bitmap);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd wczytywania obrazu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    void SetImageSource(BitmapSource bitmapSource)
    {
        int bmpWid = bitmapSource.PixelWidth, bmpHei = bitmapSource.PixelHeight;
        int imgWid, imgHei;
        if (bitmapSource.PixelWidth > ImageMaxWidth)
        {
            imgWid = ImageMaxWidth;
            imgHei = (imgWid * bmpHei) / bmpWid;
            if (imgHei > ImageMaxHeight)
            {
                imgHei = ImageMaxHeight;
                imgWid = (imgHei * bmpWid) / bmpHei;
            }
        }
        else
        {
            imgWid = bitmapSource.PixelWidth;
            imgHei = (imgWid * bmpHei) / bmpWid;
            if (imgHei > ImageMaxHeight)
            {
                imgHei = ImageMaxHeight;
                imgWid = (imgHei * bmpWid) / bmpHei;
            }
        }
        ImageWidth = imgWid;
        ImageHeight = imgHei;
        ImageBitmapSource = bitmapSource;
    }

    public void ApplyFilters()
    {
        if (ImageBitmapSource is not BitmapSource bitmapSource)
        {
            MessageBox.Show("Brak obrazu do przetworzenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int stride = width * (writeableBitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            writeableBitmap.CopyPixels(pixelData, stride, 0);

            switch (CurrentFilterMode)
            {
                case FilterMode.Addition:
                    ApplyAdditionFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.Substract:
                    ApplySubtractionFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.Multiply:
                    ApplyMultiplicationFilter(pixelData, width, height, stride);
                    break;


                case FilterMode.Divide:
                    if (RgbRed == 0 || RgbGreen == 0 || RgbRed == 0)
                        MessageBox.Show("Podaj wartości różne od 0.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    else 
                        ApplyDivisionFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.Brightness:
                    ApplyBrightnessFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.GrayScaleAverage:
                    ApplyGrayscaleAverageFilter(pixelData);
                    break;

                case FilterMode.GrayScaleMax:
                    ApplyGrayscaleMaxFilter(pixelData);
                    break;

                case FilterMode.Smoothing:
                    ApplySmoothingFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.Median:
                    ApplyMedianFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.Sobel:
                    ApplySobelFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.HighPass:
                    ApplyHighPassFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.Gaussian:
                    ApplyGaussianFilter(pixelData, width, height, stride);
                    break;

                case FilterMode.Mask:
                    double[,] customKernel = { { 0, -1, 0 }, { -1, 4, -1 }, { 0, -1, 0 } }; // Przykładowa maska
                    ApplyMaskFilter(pixelData, width, height, stride, customKernel, 1.0);
                    break;

                default:
                    MessageBox.Show("Wybrano nieobsługiwany tryb filtru.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;

            }
            var modifiedBitmap = BitmapSource.Create(width, height, bitmapSource.DpiX, bitmapSource.DpiY, writeableBitmap.Format, null, pixelData, stride);
            ImageBitmapSource = modifiedBitmap;
        }

        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas stosowania filtru: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyAdditionFilter(byte[] pixelData, int width, int height, int stride)
    {
        for (int i = 0; i < pixelData.Length; i += 4) // ARGB
        {
            pixelData[i] = (byte)Math.Min(pixelData[i] + RgbBlue, 255);  // Blue
            pixelData[i + 1] = (byte)Math.Min(pixelData[i + 1] + RgbGreen, 255); // Green
            pixelData[i + 2] = (byte)Math.Min(pixelData[i + 2] + RgbRed, 255); // Red
        }
        RgbRed = 0;
        RgbGreen = 0;
        RgbBlue = 0;
    }

    private void ApplySubtractionFilter(byte[] pixelData, int width, int height, int stride)
    {
        for (int i = 0; i < pixelData.Length; i += 4) // ARGB
        {
            pixelData[i] = (byte)Math.Max(pixelData[i] - RgbBlue, 0);  // Blue
            pixelData[i + 1] = (byte)Math.Max(pixelData[i + 1] - RgbGreen, 0); // Green
            pixelData[i + 2] = (byte)Math.Max(pixelData[i + 2] - RgbRed, 0); // Red
        }
        RgbRed = 0;
        RgbGreen = 0;
        RgbBlue = 0;
    }
    private void ApplyMultiplicationFilter(byte[] pixelData, int width, int height, int stride)
    {
        for (int i = 0; i < pixelData.Length; i += 4) // ARGB
        {
            pixelData[i] = (byte)Math.Min(pixelData[i] * RgbBlue / 255, 255);  // Blue
            pixelData[i + 1] = (byte)Math.Min(pixelData[i + 1] * RgbGreen / 255, 255); // Green
            pixelData[i + 2] = (byte)Math.Min(pixelData[i + 2] * RgbRed / 255, 255); // Red
        }
        RgbRed = 0;
        RgbGreen = 0;
        RgbBlue = 0;
    }

    private void ApplyDivisionFilter(byte[] pixelData, int width, int height, int stride)
    {
        for (int i = 0; i < pixelData.Length; i += 4) // ARGB
        {
            pixelData[i] = (byte)Math.Min(pixelData[i] / Math.Max(RgbBlue, 1), 255);  // Blue
            pixelData[i + 1] = (byte)Math.Min(pixelData[i + 1] / Math.Max(RgbGreen, 1), 255); // Green
            pixelData[i + 2] = (byte)Math.Min(pixelData[i + 2] / Math.Max(RgbRed, 1), 255); // Red
        }
        RgbRed = 0;
        RgbGreen = 0;
        RgbBlue = 0;

    }

    private void ApplyBrightnessFilter(byte[] pixelData, int width, int height, int stride)
    {
        // Obliczamy czynnik jasności w zakresie od -1.0 do 1.0
        double brightnessFactor = Brightness / 100.0;

        for (int i = 0; i < pixelData.Length; i += 4) // ARGB
        {
            // Zmiana jasności dla każdej składowej RGB
            pixelData[i] = (byte)Math.Clamp(pixelData[i] * (1 + brightnessFactor), 0, 255);      // Blue
            pixelData[i + 1] = (byte)Math.Clamp(pixelData[i + 1] * (1 + brightnessFactor), 0, 255); // Green
            pixelData[i + 2] = (byte)Math.Clamp(pixelData[i + 2] * (1 + brightnessFactor), 0, 255); // Red
        }
        Brightness = 0;
    }

    private void ApplyGrayscaleAverageFilter(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4) // ARGB
        {
            byte gray = (byte)((pixelData[i] + pixelData[i + 1] + pixelData[i + 2]) / 3); // Średnia
            pixelData[i] = gray;   // Blue
            pixelData[i + 1] = gray; // Green
            pixelData[i + 2] = gray; // Red
        }
    }

    private void ApplyGrayscaleMaxFilter(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4) // ARGB
        {
            byte gray = Math.Max(pixelData[i], Math.Max(pixelData[i + 1], pixelData[i + 2])); // Max z RGB
            pixelData[i] = gray;   // Blue
            pixelData[i + 1] = gray; // Green
            pixelData[i + 2] = gray; // Red
        }
    }

    private void ApplySmoothingFilter(byte[] pixelData, int width, int height, int stride)
    {
        int kernelSize = 3; // Rozmiar maski (3x3)
        int radius = kernelSize / 2;
        byte[] copy = (byte[])pixelData.Clone();

        for (int y = radius; y < height - radius; y++)
        {
            for (int x = radius; x < width - radius; x++)
            {
                int blue = 0, green = 0, red = 0;

                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int idx = ((y + ky) * stride) + ((x + kx) * 4);
                        blue += copy[idx];
                        green += copy[idx + 1];
                        red += copy[idx + 2];
                    }
                }

                int area = kernelSize * kernelSize;
                int centerIdx = (y * stride) + (x * 4);
                pixelData[centerIdx] = (byte)(blue / area);       // Blue
                pixelData[centerIdx + 1] = (byte)(green / area); // Green
                pixelData[centerIdx + 2] = (byte)(red / area);   // Red
            }
        }
    }

    private void ApplyMedianFilter(byte[] pixelData, int width, int height, int stride)
    {
        int kernelSize = 3; // Rozmiar maski (3x3)
        int radius = kernelSize / 2;
        byte[] copy = (byte[])pixelData.Clone();

        for (int y = radius; y < height - radius; y++)
        {
            for (int x = radius; x < width - radius; x++)
            {
                List<byte> blue = new List<byte>();
                List<byte> green = new List<byte>();
                List<byte> red = new List<byte>();

                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int idx = ((y + ky) * stride) + ((x + kx) * 4);
                        blue.Add(copy[idx]);
                        green.Add(copy[idx + 1]);
                        red.Add(copy[idx + 2]);
                    }
                }

                blue.Sort();
                green.Sort();
                red.Sort();

                int centerIdx = (y * stride) + (x * 4);
                pixelData[centerIdx] = blue[blue.Count / 2];       // Blue
                pixelData[centerIdx + 1] = green[green.Count / 2]; // Green
                pixelData[centerIdx + 2] = red[red.Count / 2];     // Red
            }
        }
    }

    private void ApplySobelFilter(byte[] pixelData, int width, int height, int stride)
    {
        int[,] gx = new int[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        int[,] gy = new int[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        byte[] copy = (byte[])pixelData.Clone();

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                // Sobel dla każdego kanału kolorów (R, G, B)
                int gxBlue = 0, gyBlue = 0;
                int gxGreen = 0, gyGreen = 0;
                int gxRed = 0, gyRed = 0;

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int idx = ((y + ky) * stride) + ((x + kx) * 4);

                        gxBlue += copy[idx] * gx[ky + 1, kx + 1];
                        gyBlue += copy[idx] * gy[ky + 1, kx + 1];

                        gxGreen += copy[idx + 1] * gx[ky + 1, kx + 1];
                        gyGreen += copy[idx + 1] * gy[ky + 1, kx + 1];

                        gxRed += copy[idx + 2] * gx[ky + 1, kx + 1];
                        gyRed += copy[idx + 2] * gy[ky + 1, kx + 1];
                    }
                }

                // Obliczenie gradientu dla każdego kanału
                int magnitudeBlue = (int)Math.Sqrt(gxBlue * gxBlue + gyBlue * gyBlue);
                int magnitudeGreen = (int)Math.Sqrt(gxGreen * gxGreen + gyGreen * gyGreen);
                int magnitudeRed = (int)Math.Sqrt(gxRed * gxRed + gyRed * gyRed);

                // Ograniczenie wartości w zakresie 0-255
                byte blue = (byte)Math.Clamp(magnitudeBlue, 0, 255);
                byte green = (byte)Math.Clamp(magnitudeGreen, 0, 255);
                byte red = (byte)Math.Clamp(magnitudeRed, 0, 255);

                // Zapisanie wyników w obrazie wyjściowym
                int centerIdx = (y * stride) + (x * 4);
                pixelData[centerIdx] = blue;   // Blue
                pixelData[centerIdx + 1] = green; // Green
                pixelData[centerIdx + 2] = red;   // Red
            }
        }
    }


    private void ApplyHighPassFilter(byte[] pixelData, int width, int height, int stride)
    {
        byte[] copy = (byte[])pixelData.Clone();
        int[,] kernel = {
        { -1, -1, -1 },
        { -1,  8, -1 },
        { -1, -1, -1 }
    };

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int index = y * stride + x * 4;
                int[] rgbSums = { 0, 0, 0 };

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int neighborIndex = (y + ky) * stride + (x + kx) * 4;
                        int weight = kernel[ky + 1, kx + 1];

                        rgbSums[0] += copy[neighborIndex] * weight;     // Blue
                        rgbSums[1] += copy[neighborIndex + 1] * weight; // Green
                        rgbSums[2] += copy[neighborIndex + 2] * weight; // Red
                    }
                }

                pixelData[index] = (byte)Math.Clamp(rgbSums[0], 0, 255);     // Blue
                pixelData[index + 1] = (byte)Math.Clamp(rgbSums[1], 0, 255); // Green
                pixelData[index + 2] = (byte)Math.Clamp(rgbSums[2], 0, 255); // Red
            }
        }
    }

    private void ApplyGaussianFilter(byte[] pixelData, int width, int height, int stride)
    {
        byte[] copy = (byte[])pixelData.Clone();
        double[,] kernel = {
        { 1, 2, 1 },
        { 2, 4, 2 },
        { 1, 2, 1 }
    };
        double kernelSum = 16.0; // Suma wag kernela

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int index = y * stride + x * 4;
                double[] rgbSums = { 0.0, 0.0, 0.0 };

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int neighborIndex = (y + ky) * stride + (x + kx) * 4;
                        double weight = kernel[ky + 1, kx + 1];

                        rgbSums[0] += copy[neighborIndex] * weight;     // Blue
                        rgbSums[1] += copy[neighborIndex + 1] * weight; // Green
                        rgbSums[2] += copy[neighborIndex + 2] * weight; // Red
                    }
                }

                pixelData[index] = (byte)Math.Clamp(rgbSums[0] / kernelSum, 0, 255);     // Blue
                pixelData[index + 1] = (byte)Math.Clamp(rgbSums[1] / kernelSum, 0, 255); // Green
                pixelData[index + 2] = (byte)Math.Clamp(rgbSums[2] / kernelSum, 0, 255); // Red
            }
        }
    }

    private void ApplyMaskFilter(byte[] pixelData, int width, int height, int stride, double[,] kernel, double kernelSum)
    {
        byte[] copy = (byte[])pixelData.Clone();
        int kernelSize = kernel.GetLength(0);
        int offset = kernelSize / 2;

        for (int y = offset; y < height - offset; y++)
        {
            for (int x = offset; x < width - offset; x++)
            {
                int index = y * stride + x * 4;
                double[] rgbSums = { 0.0, 0.0, 0.0 };

                for (int ky = -offset; ky <= offset; ky++)
                {
                    for (int kx = -offset; kx <= offset; kx++)
                    {
                        int neighborIndex = (y + ky) * stride + (x + kx) * 4;
                        double weight = kernel[ky + offset, kx + offset];

                        rgbSums[0] += copy[neighborIndex] * weight;     // Blue
                        rgbSums[1] += copy[neighborIndex + 1] * weight; // Green
                        rgbSums[2] += copy[neighborIndex + 2] * weight; // Red
                    }
                }

                pixelData[index] = (byte)Math.Clamp(rgbSums[0] / kernelSum, 0, 255);     // Blue
                pixelData[index + 1] = (byte)Math.Clamp(rgbSums[1] / kernelSum, 0, 255); // Green
                pixelData[index + 2] = (byte)Math.Clamp(rgbSums[2] / kernelSum, 0, 255); // Red
            }
        }
    }
}
