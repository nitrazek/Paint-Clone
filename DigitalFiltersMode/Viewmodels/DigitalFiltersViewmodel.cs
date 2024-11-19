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

namespace Paint_Clone.DigitalFiltersMode.Viewmodels;

public partial class DigitalFiltersViewModel : ObservableObject
{
    string initialDirectory = Directory.GetCurrentDirectory();

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

    [RelayCommand]
    void ChangeFilterMode(FilterMode newFilterMode) { CurrentFilterMode = newFilterMode; }

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
        
    }
}
