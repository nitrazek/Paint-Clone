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

namespace Paint_Clone.DigitalFiltersMode.Viewmodels;

public partial class DigitalFiltersViewModel : ObservableObject
{
    string initialDirectory = Directory.GetCurrentDirectory();

    [ObservableProperty]
    ImageSource imageBitmapSource = new BitmapImage();

    [ObservableProperty]
    double imageWidth;

    [ObservableProperty]
    double imageHeight;

    [ObservableProperty]
    double imageMaxWidth = 1070;

    [ObservableProperty]
    double imageMaxHeight = 675;

    [ObservableProperty]
    double rgbRed;

    [ObservableProperty]
    double rgbGreen;

    [ObservableProperty]
    double rgbBlue;

    [ObservableProperty]
    double brightness;

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
        double imgWid, imgHei;
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
        
    }

    private void ApplyAddition(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = Clamp(pixelData[i] + (byte)RgbBlue);
            pixelData[i + 1] = Clamp(pixelData[i + 1] + (byte)RgbGreen);
            pixelData[i + 2] = Clamp(pixelData[i + 2] + (byte)RgbRed);
        }
    }

    private void ApplySubstract(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = Clamp(pixelData[i] - (byte)RgbBlue);
            pixelData[i + 1] = Clamp(pixelData[i + 1] - (byte)RgbGreen);
            pixelData[i + 2] = Clamp(pixelData[i + 2] - (byte)RgbRed);
        }
    }

    private void ApplyMultiply(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = Clamp(pixelData[i] * (byte)RgbBlue / 255);
            pixelData[i + 1] = Clamp(pixelData[i + 1] * (byte)RgbGreen / 255);
            pixelData[i + 2] = Clamp(pixelData[i + 2] * (byte)RgbRed / 255);
        }
    }

    private void ApplyDivide(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = Clamp(pixelData[i] / ((byte)RgbBlue / 255 + 1));
            pixelData[i + 1] = Clamp(pixelData[i + 1] / ((byte)RgbGreen / 255 + 1));
            pixelData[i + 2] = Clamp(pixelData[i + 2] / ((byte)RgbRed / 255 + 1));
        }
    }

    private void ApplyBrightness(byte[] pixelData)
    {
        int brightnessOffset = (int)Brightness;
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = Clamp(pixelData[i] + brightnessOffset);
            pixelData[i + 1] = Clamp(pixelData[i + 1] + brightnessOffset);
            pixelData[i + 2] = Clamp(pixelData[i + 2] + brightnessOffset);
        }
    }

    private void ApplyGrayScaleAverage(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte gray = (byte)((pixelData[i] + pixelData[i + 1] + pixelData[i + 2]) / 3);
            pixelData[i] = gray;
            pixelData[i + 1] = gray;
            pixelData[i + 2] = gray;
        }
    }

    private void ApplyGrayScaleMax(byte[] pixelData)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte max = Math.Max(pixelData[i], Math.Max(pixelData[i + 1], pixelData[i + 2]));
            pixelData[i] = max;
            pixelData[i + 1] = max;
            pixelData[i + 2] = max;
        }
    }

    private byte Clamp(int value)
    {
        return (byte)(value < 0 ? 0 : (value > 255 ? 255 : value));
    }
}
