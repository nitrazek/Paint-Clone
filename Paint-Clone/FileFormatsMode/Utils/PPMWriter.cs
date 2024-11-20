using Paint_Clone.FileFormatsMode.Enums;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Paint_Clone.FileFormatsMode.Utils;

public class PPMWriter
{
    public PPMWriter() { }

    public void SavePPMFile(string filename, BitmapSource bitmapSource, PPMFormat format)
    {
        int width = bitmapSource.PixelWidth;
        int height = bitmapSource.PixelHeight;
        int stride = width * 4;

        byte[] data = new byte[height * stride];
        bitmapSource.CopyPixels(data, stride, 0);

        if (format == PPMFormat.P1 || format == PPMFormat.P2 || format == PPMFormat.P3)
        {
            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                switch (format)
                {
                    case PPMFormat.P1:
                        WriteP1(streamWriter, data, width, height);
                        break;
                    case PPMFormat.P2:
                        WriteP2(streamWriter, data, width, height);
                        break;
                    case PPMFormat.P3:
                        WriteP3(streamWriter, data, width, height);
                        break;
                }
            }
        }
        else
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                switch (format)
                {
                    case PPMFormat.P4:
                        WriteP4(binaryWriter, data, width, height);
                        break;
                    case PPMFormat.P5:
                        WriteP5(binaryWriter, data, width, height);
                        break;
                    case PPMFormat.P6:
                        WriteP6(binaryWriter, data, width, height);
                        break;
                }
            }
        }
    }


    private void WriteP1(StreamWriter writer, byte[] pixelData, int width, int height)
    {
        writer.WriteLine("P1");
        writer.WriteLine("# Created by Pawel & Pawel");
        writer.WriteLine($"{width} {height}");
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte gray = (byte)(0.3 * pixelData[i + 2] + 0.59 * pixelData[i + 1] + 0.11 * pixelData[i]);
            writer.Write(gray > 127 ? "0 " : "1 ");
        }
    }

    private void WriteP2(StreamWriter writer, byte[] pixelData, int width, int height)
    {
        writer.WriteLine("P2");
        writer.WriteLine("# Created by Pawel & Pawel");
        writer.WriteLine($"{width} {height}");
        writer.WriteLine("255");
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte gray = (byte)(0.3 * pixelData[i + 2] + 0.59 * pixelData[i + 1] + 0.11 * pixelData[i]);
            writer.Write($"{gray} ");
        }
    }

    private void WriteP3(StreamWriter writer, byte[] pixelData, int width, int height)
    {
        writer.WriteLine("P3");
        writer.WriteLine("# Created by Pawel & Pawel");
        writer.WriteLine($"{width} {height}");
        writer.WriteLine("255");
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            writer.Write($"{pixelData[i + 2]} {pixelData[i + 1]} {pixelData[i]} ");
        }
    }

    private void WriteP4(BinaryWriter writer, byte[] pixelData, int width, int height)
    {
        writer.Write(Encoding.ASCII.GetBytes($"P4\n# Created by Pawel & Pawel\n{width} {height}\n"));

        int byteWidth = (width + 7) / 8; 
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < byteWidth; x++)
            {
                byte byteValue = 0;
                for (int bit = 0; bit < 8 && (x * 8 + bit) < width; bit++)
                {
                    int pixelIndex = (y * width + x * 8 + bit) * 4;
                    byte gray = (byte)(0.3 * pixelData[pixelIndex + 2] + 0.59 * pixelData[pixelIndex + 1] + 0.11 * pixelData[pixelIndex]);
                    if (gray <= 127) byteValue |= (byte)(1 << (7 - bit));
                }
                writer.Write(byteValue);
            }
        }
    }

    private void WriteP5(BinaryWriter writer, byte[] pixelData, int width, int height)
    {
        writer.Write(Encoding.ASCII.GetBytes($"P5\n# Created by Pawel & Pawel\n{width} {height}\n255\n"));

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte gray = (byte)(0.3 * pixelData[i + 2] + 0.59 * pixelData[i + 1] + 0.11 * pixelData[i]);
            writer.Write(gray);
        }
    }

    private void WriteP6(BinaryWriter writer, byte[] pixelData, int width, int height)
    {
        writer.Write(Encoding.ASCII.GetBytes($"P6\n# Created by Pawel & Pawel\n{width} {height}\n255\n"));

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            writer.Write(pixelData[i + 2]); 
            writer.Write(pixelData[i + 1]); 
            writer.Write(pixelData[i]);     
        }
    }


}
