using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Paint_Clone.FileFormatsMode.Utils;

public class PPMReader
{
    private readonly byte[] buf;
    private int bytIdx;

    public PPMReader(FileStream stream)
    {
        int bytCnt = (int)stream.Length;
        if (bytCnt > (1 << 30))
        {
            throw new OutOfMemoryException("File size is bigger than 1GiB");
        }
        buf = new byte[bytCnt];
        stream.Read(buf, 0, bytCnt);
        bytIdx = 0;

    }

    private int ReadByte() => buf[bytIdx++];
    private int PeekByte() => buf[bytIdx];
    private int SkipByte() => ++bytIdx;
    private bool EndOfStream => bytIdx == buf.Length;


    public WriteableBitmap DecodeFile()
    {
        int wid = 0, hei = 0, maxColVal = 0, type = 0;
        SkipWhitespaces();
        if (ReadByte() != 'P') return null;
        type = ReadByte();
        SkipWhitespaces();
        if (!ReadUntilNextNumber(out wid)) return null;
        if (!ReadUntilNextNumber(out hei)) return null;
        if (type != '1' && type != '4' && !ReadUntilNextNumber(out maxColVal)) return null; 
        if (wid <= 0 || hei <= 0 || (type != '1' && type != '4' && maxColVal <= 0)) return null;

        switch (type)
        {
            case '1': return DecodeP1(wid, hei);
            case '2': return DecodeP2(wid, hei, maxColVal);
            case '3': return DecodeP3(wid, hei, maxColVal);
            case '4': return DecodeP4(wid, hei);
            case '5': return DecodeP5(wid, hei, maxColVal);
            case '6': return DecodeP6(wid, hei, maxColVal);
            default: return null;
        }
    }


    private bool ReadUntilNextNumber(out int result)
    {
        int cnt = 0;
        result = 0;
        while (!EndOfStream)
        {
            int c = PeekByte();

            if (c == '#') { SkipComment(); continue; }
            if ((c >= 9 && c <= 13) || (c == 32) || (c == 133) || (c == 160)) break;
            SkipByte();
            if (!(c >= '0' && c <= '9')) { result = -1; return false; } 
            ++cnt;
            result = 10 * result + (c - '0');
        }
        if (cnt == 0) return false;
        SkipWhitespaces();
        return true;
    }

    private void SkipWhitespaces()
    {
        while (!EndOfStream)
        {
            int c = PeekByte();
            if (c == '#') { SkipComment(); continue; }
            if (!((c >= 9 && c <= 13) || (c == 32) || (c == 133) || (c == 160))) break;
            SkipByte();
        }
    }

    private void SkipComment()
    {
        while (!EndOfStream)
        {
            int c = ReadByte();
            if (c == '\n') return;
        }
    }

    private WriteableBitmap DecodeP1(int width, int height)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!ReadUntilNextNumber(out int pixel)) return null;
                    int color = (int)((pixel == 0) ? 0xFFFFFFFF : 0xFF000000); 
                    p[y * width + x] = color;
                }
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
        bmp.Unlock();
        return bmp;
    }

    private WriteableBitmap DecodeP2(int width, int height, int maxColorValue)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!ReadUntilNextNumber(out int gray)) return null;
                    byte intensity = (byte)((gray * 255) / maxColorValue);
                    int color = (255 << 24) | (intensity << 16) | (intensity << 8) | intensity;
                    p[y * width + x] = color;
                }
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
        bmp.Unlock();
        return bmp;
    }
    private WriteableBitmap DecodeP3(int width, int height, int maxColorValue)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            int i = 0;
            int y = 0, x = 0, shf = 0;
            bmp.Lock();
            for (y = 0; y < height; ++y)
            {
                for (x = 0; x < width; ++x)
                {
                    int pixVal = (255 << 24);
                    for (shf = 16; shf >= 0; shf -= 8)
                    {
                        if (!ReadUntilNextNumber(out int colVal)) goto eof;
                        pixVal = pixVal | (((colVal * 255) / maxColorValue) << shf);
                    }
                    p[i++] = pixVal;
                }
            }
        eof: if (!(y == height && x == width && shf == -8)) return null;
            bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bmp.Unlock();
            return bmp;
        }
    }
    private WriteableBitmap DecodeP4(int width, int height)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 8)
                {
                    if (EndOfStream) return null;
                    int byteVal = ReadByte();
                    for (int bit = 7; bit >= 0 && (x + (7 - bit)) < width; bit--)
                    {
                        int color = (int)(((byteVal & (1 << bit)) == 0) ? 0xFFFFFFFF : 0xFF000000);
                        p[y * width + x + (7 - bit)] = color;
                    }
                }
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
        bmp.Unlock();
        return bmp;
    }

    private WriteableBitmap DecodeP5(int width, int height, int maxColorValue)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (EndOfStream) return null;
                    int gray = ReadByte();
                    byte intensity = (byte)((gray * 255) / maxColorValue);
                    int color = (255 << 24) | (intensity << 16) | (intensity << 8) | intensity;
                    p[y * width + x] = color;
                }
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
        bmp.Unlock();
        return bmp;
    }

    private WriteableBitmap DecodeP6(int width, int height, int maxColorValue)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            int i = 0;
            int y = 0, x = 0, shf = 0;
            bmp.Lock();
            for (y = 0; y < height; ++y)
            {
                for (x = 0; x < width; ++x)
                {
                    int pixVal = (255 << 24);
                    for (shf = 16; shf >= 0; shf -= 8)
                    {
                        if (EndOfStream) goto eof;
                        pixVal = pixVal | (((ReadByte() * 255) / maxColorValue) << shf);
                    }
                    p[i++] = pixVal;
                }
            }
        eof: if (!(y == height && x == width && shf == -8)) return null;
            bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bmp.Unlock();
        }
        return bmp;
    }

}