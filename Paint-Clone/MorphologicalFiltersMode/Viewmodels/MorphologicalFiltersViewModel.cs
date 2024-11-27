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
using Paint_Clone.ColorSpacesMode.Utils;
using Projekt_4;
using System.Runtime.InteropServices;
using Paint_Clone.MorphologicalFiltersMode.Enums;

namespace Paint_Clone.MorphologicalFiltersMode.Viewmodels;

public partial class MorphologicalFiltersViewModel : ObservableObject
{
    string initialDirectory = Directory.GetCurrentDirectory();
    [ObservableProperty]
    FilterMode currentFilterMode = FilterMode.Streching;
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
    int manualThreshold = 1;
    [ObservableProperty]
    int percentBlackSelection = 100;

    [RelayCommand]
    void ChangeFilterMode(FilterMode newFilterMode) { CurrentFilterMode = newFilterMode; }

    private const int BLACK = (255 << 24) | (0 << 16) | (0 << 8) | 0,
           WHITE = (255 << 24) | (255 << 16) | (255 << 8) | 255;

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

        ImageBitmapSource = new WriteableBitmap(bitmapSource);
    }

    private WriteableBitmap GetWriteableBitmap()
    {
        if (ImageBitmapSource is WriteableBitmap writeableBitmap)
        {
            return writeableBitmap;
        }
        else if (ImageBitmapSource is BitmapSource bitmapSource)
        {
            writeableBitmap = new WriteableBitmap(bitmapSource);
            ImageBitmapSource = writeableBitmap;
            return writeableBitmap;
        }
        else
        {
            throw new InvalidOperationException("Nieobsługiwany typ obrazu w ImageBitmapSource.");
        }
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
            switch (CurrentFilterMode)
            {
                case FilterMode.Streching:
                    PerformStretching();
                    break;

                case FilterMode.Equalization:
                    PerformEqualization();
                    break;

                case FilterMode.Manual:
                    PerformManual(ManualThreshold);
                    break;

                case FilterMode.PercentBlackSelection:
                    PerformPercentBlackSelection(PercentBlackSelection);
                    break;

                case FilterMode.EntropySelection:
                    PerformEntropySelection();
                    break;

                case FilterMode.MinimumError:
                    PerformMinimumError();
                    break;

                case FilterMode.Otsu:
                    PerformOtsu();
                    break;

                case FilterMode.Niblack:
                    PerformNiblack();
                    break;

                case FilterMode.Sauvola:
                    PerformSauvola();
                    break;

                case FilterMode.Phansalkar:
                    PerformPhansalkar();
                    break;

                case FilterMode.Dilatation:
                    PerformMinkowskiSum(WHITE, BLACK);

                    break;

                case FilterMode.Erosion:
                    PerformMinkowskiSum(BLACK, WHITE);
                    break;

                case FilterMode.Opening:
                    PerformMinkowskiSum(BLACK, WHITE);
                    PerformMinkowskiSum(WHITE, BLACK);

                    break;

                case FilterMode.Closing:
                    PerformMinkowskiSum(WHITE, BLACK);
                    PerformMinkowskiSum(BLACK, WHITE);
                    break;

                case FilterMode.Thickening:
                    PerformThickening();
                    break;

                case FilterMode.Thinning:
                    PerformThinning();
                    break;

                default:
                    MessageBox.Show("Wybrano nieobsługiwany tryb filtru.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas stosowania filtru: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PerformStretching()
    {
        var bmp = GetWriteableBitmap();
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
        ImageBitmapSource = bmp;
    }

    private void PerformEqualization()
    {
        var bmp = GetWriteableBitmap();
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        bmp.Lock();
        unsafe
        {
            int[] rCnt = new int[256], gCnt = new int[256], bCnt = new int[256];
            for (int v = 0; v <= 255; ++v)
            {
                rCnt[v] = 0;
                gCnt[v] = 0;
                bCnt[v] = 0;
            } 
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    int argb = *p;
                    ++rCnt[(argb >> 16) & 255];
                    ++gCnt[(argb >> 8) & 255];
                    ++bCnt[argb & 255];
                    ++p;
                }
            for (int v = 1; v <= 255; ++v)
            {
                rCnt[v] = rCnt[v - 1] + rCnt[v];
                gCnt[v] = gCnt[v - 1] + gCnt[v];
                bCnt[v] = bCnt[v - 1] + bCnt[v];
            }
            int[] rDist = rCnt, gDist = gCnt, bDist = bCnt;
            int rDistMin = 0, gDistMin = 0, bDistMin = 0;
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
            double rMult = 255.0 / rDiv, gMult = 255.0 / gDiv, bMult = 255.0 / bDiv; 
            for (int v = 0; v <= 255; ++v)
            {
                rDist[v] = (int)Math.Round((rDist[v] - rDistMin) * rMult); // & 255
                gDist[v] = (int)Math.Round((gDist[v] - gDistMin) * gMult); // & 255
                bDist[v] = (int)Math.Round((bDist[v] - bDistMin) * bMult); // & 255
            }
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
        ImageBitmapSource = bmp;
    }

    private void Binarize(byte[] grayscaleBuffer, int threshold)
    {
        var bmp = GetWriteableBitmap();
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        int i = 0;
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int y = 0; y < hei; ++y)
                for (int x = 0; x < wid; ++x)
                {
                    if (grayscaleBuffer[i] <= threshold) *p = BLACK;
                    else *p = WHITE; 
                    ++i;
                    ++p;
                }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, wid, hei));
        bmp.Unlock();
        ImageBitmapSource = bmp;
    }

    private void PerformManual(int threshold)
    {
        var bmp = GetWriteableBitmap();
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
        var bmp = (WriteableBitmap)ImageBitmapSource;
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        var grayBuf = ToGrayscaleBuffer(bmp);
        var h = ComputeHistogram(grayBuf, wid, hei);
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
        var bmp = GetWriteableBitmap();
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        var grayBuf = ToGrayscaleBuffer(bmp);
        var h = ComputeHistogram(grayBuf, wid, hei);
        double[] fhistogram = new double[h.Length];
        int widHei = wid * hei;
        for (int i = 0; i < fhistogram.Length; ++i)
            fhistogram[i] = h[i] / (double)widHei;
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
        var bmp = GetWriteableBitmap();
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        var grayBuf = ToGrayscaleBuffer(bmp);
        var histogram = ComputeHistogram(grayBuf, wid, hei);
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
        for (int y = 0; y < height; ++y)
            for (int x = 0; x < width; ++x)
                ++hist[grayscaleBuffer[i++]];
        return hist;
    }

    private double Log2(double d)
    {
        if (d == 0) return double.MinValue;
        return Math.Log(d, 2);
    }

    private double Log(double f)
    {
        if (f <= 0) return 0;
        return Math.Log(f);
    }



    private void PerformMinkowskiSum(int expandedColor, int shrunkenColor)
    {
        var bmp = GetWriteableBitmap();
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        int[] outBuf = new int[wid * hei];
        bmp.Lock();
        byte expVal = (byte)(expandedColor & 255),
            shrVal = (byte)(shrunkenColor & 255);
        unsafe
        {
            int* pIBeg = (int*)bmp.BackBuffer;
            int i = 0;
            for (int iY = 0; iY < hei; ++iY)
            {
                for (int iX = 0; iX < wid; ++iX)
                {
                    outBuf[i] = shrunkenColor;
                    for (int seY = -1; seY <= 1; ++seY)
                    {
                        for (int seX = -1; seX <= 1; ++seX)
                        {
                            int x = iX + seX, y = iY + seY;
                            byte pixVal = shrVal;
                            if (x >= 0 && x < wid && y >= 0 && y < hei)
                            {
                                pixVal = (byte)(*(pIBeg + x + wid * y) & 255);
                            }
                           
                            if (pixVal == expVal)
                            {
                                outBuf[i] = expandedColor;
                                goto OUT_OF_STRUCTURED_ELEMENT;
                            }
                        }
                    }
                OUT_OF_STRUCTURED_ELEMENT:
                    ++i;
                }
            }
            Marshal.Copy(outBuf, 0, bmp.BackBuffer, outBuf.Length);
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, wid, hei));
        bmp.Unlock();
        ImageBitmapSource = bmp;
    }

    private void PerformThinning()
    {
        var bmp = GetWriteableBitmap();

        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        int[] outBuf = new int[wid * hei];
        const int seWid = 3, seHei = 3;
        const byte o = 255, z = 0, n = 128;
        byte[][] strEls = new byte[8][];
        strEls[0] = new byte[] { o, o, o, n, o, n, z, z, z }; // B1
        strEls[1] = new byte[] { o, n, z, o, o, z, o, n, z }; // B2
        strEls[2] = new byte[] { z, z, z, n, o, n, o, o, o }; // B3
        strEls[3] = new byte[] { z, n, o, z, o, o, z, n, o }; // B4
        strEls[4] = new byte[] { o, o, n, o, o, z, n, z, z }; // B5
        strEls[5] = new byte[] { n, z, z, o, o, z, o, o, n }; // B6
        strEls[6] = new byte[] { z, z, n, z, o, o, n, o, o }; // B7
        strEls[7] = new byte[] { n, o, o, z, o, o, z, z, n }; // B8
        strEls[4][0] = strEls[5][6] = strEls[6][8] = strEls[7][2] = n;
        bmp.Lock();
        unsafe
        {
            int* pIBeg = (int*)bmp.BackBuffer;
            for (int i = 0; i < strEls.Length; ++i)
            {
                HitOrMissTransform(strEls[i], seWid, seHei, pIBeg, outBuf, wid, hei,
                    BLACK);
                Marshal.Copy(outBuf, 0, bmp.BackBuffer, outBuf.Length);
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, wid, hei));
        bmp.Unlock();
        ImageBitmapSource = bmp;
    }

    private void PerformThickening()
    {
        var bmp = GetWriteableBitmap();
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
        int[] outBuf = new int[wid * hei];
        const int seWid = 3, seHei = 3;
        const byte o = 255, z = 0, n = 128;
        byte[][] strEls = new byte[8][];
     
        strEls[0] = new byte[] { o, o, n, o, z, n, o, n, z }; // B1
        strEls[1] = new byte[] { n, n, z, o, z, n, o, o, o }; // B2
        strEls[2] = new byte[] { z, n, o, n, z, o, n, o, o }; // B3
        strEls[3] = new byte[] { o, o, o, n, z, o, z, n, n }; // B4
        strEls[4] = new byte[] { n, o, o, n, z, o, z, n, o }; // B5
        strEls[5] = new byte[] { o, o, o, o, z, n, n, n, z }; // B6
        strEls[6] = new byte[] { o, n, z, o, z, n, o, o, n }; // B7
        strEls[7] = new byte[] { z, n, n, n, z, o, o, o, o }; // B8
        bmp.Lock();
        unsafe
        {
            int* pIBeg = (int*)bmp.BackBuffer;
            for (int i = 0; i < strEls.Length; ++i)
            {
                HitOrMissTransform(strEls[i], seWid, seHei, pIBeg, outBuf, wid, hei,
                    WHITE);
                Marshal.Copy(outBuf, 0, bmp.BackBuffer, outBuf.Length);
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, wid, hei));
        bmp.Unlock();
        ImageBitmapSource = bmp;
    }

    unsafe private void HitOrMissTransform(byte[] structuringElement,
        int seWid, int seHei, int* src, int[] dst, int wid, int hei,
        int expandedColor)
    {
        int halfSeWid = seWid / 2, halfSeHei = seHei / 2;
        int oI = 0;
        int* pSrc = src;
        for (int iY = 0; iY < hei; ++iY)
        {
            for (int iX = 0; iX < wid; ++iX)
            {
                dst[oI] = *(pSrc++);
                int seI = 0;
                for (int seY = -halfSeHei; seY <= halfSeHei; ++seY)
                {
                    for (int seX = -halfSeWid; seX <= halfSeWid; ++seX)
                    {
                        int x = iX + seX, y = iY + seY;
                        byte pixVal = 0;
                        if (x >= 0 && x < wid && y >= 0 && y < hei)
                            pixVal = (byte)(*(src + x + wid * y) & 255);
                        if (structuringElement[seI] == 128)
                        {
                            ++seI;
                            continue;
                        }
                        if (pixVal != structuringElement[seI++])
                            goto OUT_OF_STRUCTURED_ELEMENT;
                    }
                }
                dst[oI] = expandedColor;
            OUT_OF_STRUCTURED_ELEMENT:
                ++oI;
            }
        }
    }
    private void PerformOtsu()
    {
        var bmp = GetWriteableBitmap();
        var grayBuf = ToGrayscaleBuffer(bmp);
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;

        var histogram = ComputeHistogram(grayBuf, wid, hei);
        int totalPixels = wid * hei;

        double sum = 0;
        for (int i = 0; i < 256; i++) sum += i * histogram[i];

        double sumB = 0, maxVariance = 0;
        int threshold = 0, weightB = 0, weightF;
        for (int i = 0; i < 256; i++)
        {
            weightB += histogram[i];
            if (weightB == 0) continue;
            weightF = totalPixels - weightB;
            if (weightF == 0) break;

            sumB += i * histogram[i];
            double meanB = sumB / weightB;
            double meanF = (sum - sumB) / weightF;
            double variance = weightB * weightF * Math.Pow(meanB - meanF, 2);
            if (variance > maxVariance)
            {
                maxVariance = variance;
                threshold = i;
            }
        }

        Binarize(grayBuf, threshold);
    }
    private void PerformNiblack(double k = -0.2, int windowSize = 15)
    {
        var bmp = GetWriteableBitmap();
        var grayBuf = ToGrayscaleBuffer(bmp);
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;

        var resultBuf = new byte[wid * hei];
        int radius = windowSize / 2;

        for (int y = 0; y < hei; y++)
        {
            for (int x = 0; x < wid; x++)
            {
                int xmin = Math.Max(x - radius, 0), xmax = Math.Min(x + radius, wid - 1);
                int ymin = Math.Max(y - radius, 0), ymax = Math.Min(y + radius, hei - 1);

                double sum = 0, sqSum = 0;
                int count = 0;
                for (int j = ymin; j <= ymax; j++)
                    for (int i = xmin; i <= xmax; i++)
                    {
                        byte pixel = grayBuf[j * wid + i];
                        sum += pixel;
                        sqSum += pixel * pixel;
                        count++;
                    }

                double mean = sum / count;
                double variance = sqSum / count - mean * mean;
                double stdDev = Math.Sqrt(variance);
                double threshold = mean + k * stdDev;

                resultBuf[y * wid + x] = grayBuf[y * wid + x] > threshold ? (byte)255 : (byte)0;
            }
        }

        ApplyBinaryBufferToBitmap(resultBuf, wid, hei);
    }
    private void PerformSauvola(double k = 0.5, int windowSize = 15, double R = 128)
    {
        var bmp = GetWriteableBitmap();
        var grayBuf = ToGrayscaleBuffer(bmp);
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;

        var resultBuf = new byte[wid * hei];
        int radius = windowSize / 2;

        for (int y = 0; y < hei; y++)
        {
            for (int x = 0; x < wid; x++)
            {
                int xmin = Math.Max(x - radius, 0), xmax = Math.Min(x + radius, wid - 1);
                int ymin = Math.Max(y - radius, 0), ymax = Math.Min(y + radius, hei - 1);

                double sum = 0, sqSum = 0;
                int count = 0;
                for (int j = ymin; j <= ymax; j++)
                    for (int i = xmin; i <= xmax; i++)
                    {
                        byte pixel = grayBuf[j * wid + i];
                        sum += pixel;
                        sqSum += pixel * pixel;
                        count++;
                    }

                double mean = sum / count;
                double variance = sqSum / count - mean * mean;
                double stdDev = Math.Sqrt(variance);
                double threshold = mean * (1 + k * (stdDev / R - 1));

                resultBuf[y * wid + x] = grayBuf[y * wid + x] > threshold ? (byte)255 : (byte)0;
            }
        }

        ApplyBinaryBufferToBitmap(resultBuf, wid, hei);
    }
    private void PerformPhansalkar(double k = 0.25, double r = 0.5, double p = 2, double q = 10, int windowSize = 15)
    {
        var bmp = GetWriteableBitmap();
        var grayBuf = ToGrayscaleBuffer(bmp);
        int wid = bmp.PixelWidth, hei = bmp.PixelHeight;

        var resultBuf = new byte[wid * hei];
        int radius = windowSize / 2;

        for (int y = 0; y < hei; y++)
        {
            for (int x = 0; x < wid; x++)
            {
                int xmin = Math.Max(x - radius, 0), xmax = Math.Min(x + radius, wid - 1);
                int ymin = Math.Max(y - radius, 0), ymax = Math.Min(y + radius, hei - 1);

                double sum = 0, sqSum = 0;
                int count = 0;
                for (int j = ymin; j <= ymax; j++)
                    for (int i = xmin; i <= xmax; i++)
                    {
                        byte pixel = grayBuf[j * wid + i];
                        sum += pixel;
                        sqSum += pixel * pixel;
                        count++;
                    }

                double mean = sum / count;
                double variance = sqSum / count - mean * mean;
                double stdDev = Math.Sqrt(variance);
                double threshold = mean * (1 + p * Math.Exp(-q * mean) + k * (stdDev / r - 1));

                resultBuf[y * wid + x] = grayBuf[y * wid + x] > threshold ? (byte)255 : (byte)0;
            }
        }

        ApplyBinaryBufferToBitmap(resultBuf, wid, hei);
    }
    private void ApplyBinaryBufferToBitmap(byte[] buffer, int width, int height)
    {
        var bmp = GetWriteableBitmap();
        bmp.Lock();
        unsafe
        {
            int* p = (int*)bmp.BackBuffer;
            for (int i = 0; i < buffer.Length; i++)
            {
                *p = buffer[i] == 0 ? BLACK : WHITE;
                p++;
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
        bmp.Unlock();
        ImageBitmapSource = bmp;
    }


}