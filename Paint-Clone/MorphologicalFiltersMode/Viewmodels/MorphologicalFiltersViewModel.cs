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
using Projekt_4;

namespace Paint_Clone.MorphologicalFiltersMode.Viewmodels;

public partial class MorphologicalFiltersViewModel : ObservableObject
{
    private void PerformStretching()
    {
        var bmp = (WriteableBitmap)Image.Source;
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            byte rMin = (byte)(*p >> 16), gMin = (byte)(*p >> 8), bMin = (byte)(*p);
            byte rMax = rMin, gMax = gMin, bMax = bMin;
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    byte r = (byte)(*p >> 16), g = (byte)(*p >> 8), b = (byte)(*p);
                    if (r < rMin) rMin = r;
                    else if (r > rMax) rMax = r;
                    if (g < gMin) gMin = g;
                    else if (g > gMax) gMax = g;
                    if (b < bMin) bMin = b;
                    else if (b > bMax) bMax = b;
                    ++p;
                }
            p = (int*)bmp.BackBuffer;
            int rDiv = rMax - rMin, gDiv = gMax - gMin, bDiv = bMax - bMin;
            const string err = " ma we wszystkich pikselach taką samą wartość.";
            if (rDiv == 0) { MessageBox.Show("Kolor czerwony" + err); return; }
            if (gDiv == 0) { MessageBox.Show("Kolor zielony" + err); return; }
            if (bDiv == 0) { MessageBox.Show("Kolor niebieski" + err); return; }
            double rMult = 255.0 / rDiv, gMult = 255.0 / gDiv, bMult = 255.0 / bDiv;
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    double r = (byte)(*p >> 16), g = (byte)(*p >> 8), b = (byte)(*p);
                    r = (r - rMin) * rMult;
                    g = (g - gMin) * gMult;
                    b = (b - bMin) * bMult;
                    *p = (255 << 24) | ((int)r << 16) | ((int)g << 8) | (int)b;
                    ++p;
                }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, wid, hei));
        bmp.Unlock();
    }

    private void PerformEqualization()
    {
        var bmp = (WriteableBitmap)Image.Source;
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        bmp.Lock();
        unsafe
        {
            int[] rCnt = new int[256], gCnt = new int[256], bCnt = new int[256];
            /* for (int v = 0; v <= 255; ++v) // po stworzeniu tablice są wyzerowane
            {
                rCnt[v] = 0;
                gCnt[v] = 0;
                bCnt[v] = 0;
            } */
            int* p = (int*)bmp.BackBuffer;
            // wyznaczamy liczbę wystąpień każdej z 256 wartości koloru R, G, B
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    int argb = *p;
                    ++rCnt[(argb >> 16) & 255];
                    ++gCnt[(argb >> 8) & 255];
                    ++bCnt[argb & 255];
                    ++p;
                }
            // wyznaczamy dystrybuanty wartości kolorów R, G, B (skumulowane liczby wystąpień)
            for (int v = 1; v <= 255; ++v)
            {
                rCnt[v] = rCnt[v - 1] + rCnt[v];
                gCnt[v] = gCnt[v - 1] + gCnt[v];
                bCnt[v] = bCnt[v - 1] + bCnt[v];
            }
            int[] rDist = rCnt, gDist = gCnt, bDist = bCnt;
            // wyznaczamy minimalne wartości dystrybuant R, G, B
            int rDistMin = 0, gDistMin = 0, bDistMin = 0;
            // nie trzeba sprawdzać wszystkich wartości dystrybuant, bo zawsze minimum jest na początku kumulacji (pominąwszy zera)
            for (int v = 0; v <= 255; ++v)
                if (rDist[v] > 0) { rDistMin = rDist[v]; break; }
            for (int v = 0; v <= 255; ++v)
                if (gDist[v] > 0) { gDistMin = gDist[v]; break; }
            for (int v = 0; v <= 255; ++v)
                if (bDist[v] > 0) { bDistMin = bDist[v]; break; }
            const string err = "Wszystkie piksele mają wartość 0 koloru ";
            if (rDistMin == 0) { MessageBox.Show(err + "czerwonego"); return; }
            if (gDistMin == 0) { MessageBox.Show(err + "zielonego"); return; }
            if (bDistMin == 0) { MessageBox.Show(err + "niebieskiego"); return; }
            int widHei = wid * hei;
            int rDiv = widHei - rDistMin, gDiv = widHei - gDistMin, bDiv = widHei - bDistMin;
            double rMult = 255.0 / rDiv, gMult = 255.0 / gDiv, bMult = 255.0 / bDiv; // L - 1 = 255
            for (int v = 0; v <= 255; ++v)
            {
                rDist[v] = (int)Math.Round((rDist[v] - rDistMin) * rMult); // & 255
                gDist[v] = (int)Math.Round((gDist[v] - gDistMin) * gMult); // & 255
                bDist[v] = (int)Math.Round((bDist[v] - bDistMin) * bMult); // & 255
            }
            // mapujemy oryginalne wartości kolorów pikseli na nowe odpowiadające im wartości
            p = (int*)bmp.BackBuffer;
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    int argb = *p;
                    *p = (255 << 24) |
                        (rDist[(argb >> 16) & 255] << 16) |
                        (gDist[(argb >> 8) & 255] << 8) |
                        bDist[argb & 255];
                    ++p;
                }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, wid, hei));
        bmp.Unlock();
    }

    private void Binarize(byte[] grayscaleBuffer, int threshold)
    {
        var bmp = (WriteableBitmap)Image.Source;
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        int i = 0;
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    // poniżej lub równo poziomu ustawiamy czarny
                    if (grayscaleBuffer[i] <= threshold) *p = BLACK;
                    else *p = WHITE; // powyżej poziomu ustawiamy biały
                    ++i;
                    ++p;
                }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, wid, hei));
        bmp.Unlock();
    }

    private void PerformManual(int threshold)
    {
        var bmp = (WriteableBitmap)Image.Source;
        var grayBuf = ToGrayscaleBuffer(bmp);
        Binarize(grayBuf, threshold);
    }

    private byte[] ToGrayscaleBuffer(WriteableBitmap bmp)
    {
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        byte[] buf = new byte[wid * hei];
        int i = 0;
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    int argb = *p;
                    byte r = (byte)((argb >> 16) & 255),
                        g = (byte)((argb >> 8) & 255),
                        b = (byte)(argb & 255);
                    byte max;
                    // zamieniamy RGB na V (jasność) z HSV
                    max = r > g ? r : g;
                    if (b > max) max = b;
                    buf[i++] = max;
                    ++p;
                }
        }
        return buf;
    }

    private void PerformPercentBlackSelection(double percent)
    {
        var bmp = (WriteableBitmap)Image.Source;
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        var grayBuf = ToGrayscaleBuffer(bmp);
        var h = ComputeHistogram(grayBuf, wid, hei);
        // wyznaczamy skumulowany histogram - skumulowane liczby wystąpień
        int N = wid * hei;
        int K;
        int accum = 0;
        for (K = 0; K <= 255; ++K)
        {
            accum += h[K];
            if ((double)accum >= N * percent) break;
        }
        Binarize(grayBuf, K);
    }

    private void PerformEntropySelection()
    {
        var bmp = (WriteableBitmap)Image.Source;
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        var grayBuf = ToGrayscaleBuffer(bmp);
        var h = ComputeHistogram(grayBuf, wid, hei);
        double[] fhistogram = new double[h.Length];
        int widHei = wid * hei;
        // wyznaczamy prawdopodobieństwo wystąpienia danej wartości skali szarości w obrazie - histogram znormalizowany do zakresu <0,1>
        for (int i = 0; i < fhistogram.Length; ++i)
            fhistogram[i] = h[i] / (double)widHei;
        // _methIndex == 3
        int threshold = 127;
        double maxsum = double.MinValue,
            f = 0,
            Pt = 0;
        double maxlow = fhistogram[0],
            maxhigh = 0,
            Ht = 0,
            HT = 0;
        for (int i = 0; i < 256; i++)
            HT -= fhistogram[i] * Log2(fhistogram[i]);
        for (int i = 0; i < 256; i++)
        {
            Pt += fhistogram[i];
            // maxlow = Math.Max(maxlow, _fhistogram[i]);
            if (fhistogram[i] > maxlow)
                maxlow = fhistogram[i];
            maxhigh = i < 255 ? fhistogram[i + 1] : fhistogram[i];
            for (int j = i + 2; j < 256; j++)
                if (fhistogram[j] > maxhigh)
                    maxhigh = fhistogram[j];
            Ht -= (fhistogram[i] * Log2(fhistogram[i]));
            f = Ht * Log2(Pt) / (HT * Log2(maxlow)) +
                (1 - Ht / HT) * Log2(1 - Pt) / Log2(maxhigh);
            if (f > maxsum)
            {
                maxsum = f;
                threshold = i;
            }
        }
        Binarize(grayBuf, threshold);
    }

    private void PerformMinimumError()
    {
        var bmp = (WriteableBitmap)Image.Source;
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        var grayBuf = ToGrayscaleBuffer(bmp);
        var histogram = ComputeHistogram(grayBuf, wid, hei);
        // _methIndex == 4
        int threshold = 127;
        double minvalue = double.MaxValue;
        double J, P1, P2, s1, s2, fv, u1, u2, Pi1, Pi2;
        P1 = P2 = Pi1 = Pi2 = 0;
        int v;
        for (int i = 0; i < 256; i++)
        {
            v = histogram[i];
            P2 += v;
            v *= i;
            Pi2 += v;
        }
        for (int i = 0; i < 256; i++)
        {
            v = histogram[i];
            P1 += v;
            P2 -= v;
            v *= i;
            Pi1 += v;
            Pi2 -= v;
            u1 = P1 > 0 ? Pi1 / P1 : 0;
            u2 = P2 > 0 ? Pi2 / P2 : 0;
            s1 = 0;
            if (P1 > 0)
            {
                for (int j = 0; j <= i; j++)
                {
                    fv = j - u1;
                    s1 += fv * fv * histogram[j];
                }
                s1 /= P1;
            }
            s2 = 0;
            if (P2 > 0)
            {
                for (int j = i + 1; j < 256; j++)
                {
                    fv = j - u2;
                    s2 += fv * fv * histogram[j];
                }
                s2 /= P2;
            }
            J = 1 + 2 * P1 * (Log(s1) - Log(P1) + P2 * (Log(s2) - Log(P2)));
            if (J < minvalue)
            {
                threshold = i;
                minvalue = J;
            }
        }
        Binarize(grayBuf, threshold);
    }
    private int[] ComputeHistogram(byte[] grayscaleBuffer, int width, int height)
    {
        int[] hist = new int[256];
        int i = 0;
        // wyznaczamy histogram dla skali szarości - liczbę wystąpień każdej z 256 wartości skali szarości
        for (int y = 0; y < height; ++y)
            for (int x = 0; x < width; ++x)
                ++hist[grayscaleBuffer[i++]];
        return hist;
    }




    // 16, 17 -> PROJEKT 8 NA CEZIE
}