using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Paint_Clone.FileFormatsMode.Enums;
using Paint_Clone.FileFormatsMode.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paint_Clone.FileFormatsMode.Viewmodels;

public partial class FileFormatsViewModel : ObservableObject
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


    public void LoadFile()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "Load",
            InitialDirectory = initialDirectory,
            Filter = "NetPBM files|*.ppm;*.pgm;*.pbm",
            FilterIndex = 1
        };
        
        if (dialog.ShowDialog() != true) return;
        var path = dialog.FileName;
        initialDirectory = Path.GetDirectoryName(path);
        if (!File.Exists(path)) MessageBox.Show($"File {path} does not exist.");
        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
        {
            var ext = Path.GetExtension(path);
            string errorMessage = $"Error while loading file {path}";
            WriteableBitmap bitmap = new PPMReader(stream).DecodeFile();
            if (bitmap == null) MessageBox.Show(errorMessage);
            else SetImageSource(bitmap);
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

    public void SaveFile()
    {
        var dialog = new SaveFileDialog()
        {
            Title = "Save file",
            InitialDirectory = initialDirectory,
            Filter = "P1 PBM ASCII|*.pbm|" +
                         "P2 PGM ASCII|*.pgm|" +
                         "P3 PPM ASCII|*.ppm|" +
                         "P4 PBM Binary|*.pbm|" +
                         "P5 PGM Binary|*.pgm|" +
                         "P6 PPM Binary|*.ppm",
            DefaultExt = "ppm"
        };

        if (dialog.ShowDialog() == true)
        {
            PPMFormat format = PPMFormat.P3;
            switch (dialog.FilterIndex)
            {
                case 1:
                    format = PPMFormat.P1;
                    break;
                case 2:
                    format = PPMFormat.P2;
                    break;
                case 3:
                    format = PPMFormat.P3;
                    break;
                case 4:
                    format = PPMFormat.P4;
                    break;
                case 5:
                    format = PPMFormat.P5;
                    break;
                case 6:
                    format = PPMFormat.P6;
                    break;
            }

            PPMWriter writer = new PPMWriter();
            string path = dialog.FileName;
            writer.SavePPMFile(path, (BitmapSource)ImageBitmapSource, format);
            MessageBox.Show("Obraz zapisany w formacie: " + format);
        }
    }
}    
