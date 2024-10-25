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

namespace Paint_Clone.utils;

class CanvasToImage
{
    public static void SaveCanvasToImage(Canvas canvas, string filename)
    {
        double width = canvas.ActualWidth;
        double height = canvas.ActualHeight;

        if (width == 0 || height == 0)
        {
            MessageBox.Show("Błędne wymiary obrazu.");
            return;
        }
        RenderTargetBitmap render = new RenderTargetBitmap((int)width, (int)height, 96d, 96d, PixelFormats.Pbgra32);

        canvas.Measure(new Size(width, height));
        canvas.Arrange(new Rect(new Size(width, height)));

        render.Render(canvas);

        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(render));

        using (FileStream fileStream = new FileStream(filename, FileMode.Create)) 
        { 
            encoder.Save(fileStream);
        }

    }
}
