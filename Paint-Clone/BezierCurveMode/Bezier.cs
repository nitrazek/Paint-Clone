﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Paint_Clone.BezierCurveMode.Utils
{
    public static class Bezier
    {
        // wersja z rekurencją ogonową
        /* public static void Draw(WriteableBitmap bmp, Point[] points, int color)
        {
            int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
            bmp.Lock();
            unsafe
            {
                int* p = (int*)bmp.BackBuffer;
                for (double t = 0.0; t < 1.0; t += DELTA_T)
                {
                    Point drawPt = GetDrawPoint(points, t);
                    int x = (int)drawPt.X, y = (int)drawPt.Y;
                    if (x >= 0 && x < wid && y >= 0 && y < hei)
                        *(p + x + wid * y) = color;
                    // bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                }
            }
            bmp.Unlock();
        }

        private static Point GetDrawPoint(Point[] pts, double t)
        {
            if (pts.Length == 2)
            {
                Vector vec = pts[1] - pts[0];
                return pts[0] + t * vec;
            }
            Point[] onePtLess = new Point[pts.Length - 1];
            for (int i = 0; i < onePtLess.Length; ++i)
            {
                Point pt = pts[i];
                Vector vec = pts[i + 1] - pts[i];
                onePtLess[i] = pt + t * vec;
            }
            return GetDrawPoint(onePtLess, t);
        } */

        // algorytm De Casteljau
        // https://javascript.info/bezier-curve
        public static void DrawWithDots(WriteableBitmap bmp, ICollection<HighlightablePoint> points,
            double deltaT, int color)
        {
            int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
            Point[] temp = new Point[points.Count];
            unsafe
            {
                int* p = (int*)bmp.BackBuffer;
                for (double t = 0.0; t < 1.0; t += deltaT)
                {
                    {
                        int i = 0;
                        foreach (var origP in points)
                        {
                            Point copy = temp[i];
                            temp[i].X = origP.X;
                            temp[i].Y = origP.Y;
                            ++i;
                        }
                    }
                    for (int lvlPtCnt = points.Count; lvlPtCnt >= 2; --lvlPtCnt)
                    {
                        for (int i = 0; i < lvlPtCnt - 1; ++i)
                        {
                            Point pt = temp[i];
                            Vector vec = temp[i + 1] - temp[i];
                            temp[i] = pt + t * vec;
                        }
                    }
                    Point drawPt = temp[0];
                    int x = (int)drawPt.X, y = (int)drawPt.Y;
                    /* if (x >= 0 && x < wid && y >= 0 && y < hei)
                    { */
                        *(p + x + wid * y) = color;
                        bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                    // }
                }
            }
        }

        public static void DrawWithLines(WriteableBitmap bmp, ICollection<HighlightablePoint> points,
            double deltaT, int color)
        {
            int wid = bmp.PixelWidth, hei = bmp.PixelHeight;
            Point[] temp = new Point[points.Count];
            Point prevDrawPt = new Point(-1, -1);
            unsafe
            {
                int* p = (int*)bmp.BackBuffer;
                double t = 0.0;
                Point curDrawPt = CalculateDrawPoint(temp, points, t);
                // rysujemy pojedynczy punkt
                int x = (int)curDrawPt.X, y = (int)curDrawPt.Y;
                // if (x >= 0 && x < wid && y >= 0 && y < hei)
                *(p + x + wid * y) = color;
                bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                for (t += deltaT; t <= 1.0; t += deltaT)
                {
                    prevDrawPt = curDrawPt;
                    curDrawPt = CalculateDrawPoint(temp, points, t);
                    // rysujemy odcinek od poprzedniego punktu do aktualnego
                    BresenhamLine2(bmp, prevDrawPt, curDrawPt, color);
                }
            }
        }

        private static Point CalculateDrawPoint(Point[] temp, ICollection<HighlightablePoint> points,
            double t)
        {
            {
                int i = 0;
                foreach (var origP in points)
                {
                    Point copy = temp[i];
                    temp[i].X = origP.X;
                    temp[i].Y = origP.Y;
                    ++i;
                }
            }
            for (int lvlPtCnt = points.Count; lvlPtCnt >= 2; --lvlPtCnt)
            {
                for (int i = 0; i < lvlPtCnt - 1; ++i)
                {
                    Point pt = temp[i];
                    Vector vec = temp[i + 1] - temp[i];
                    temp[i] = pt + t * vec;
                }
            }
            return temp[0];
        }

        public static void BresenhamLine1(WriteableBitmap bitmap, Point start, Point end, int color)
        {
            int x1 = (int)start.X, y1 = (int)start.Y, x2 = (int)end.X, y2 = (int)end.Y;
            // algorytm: https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
            int x = x1, y = y1;
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            int bmpW = bitmap.PixelWidth; // bmpH = bitmap.PixelHeight;
            int bmpWdy1 = bmpW * dy1, bmpWdy2 = bmpW * dy2;
            unsafe
            {
                int* pBackBuffer = (int*)bitmap.BackBuffer + x + bmpW * y;
                for (int i = 0; i <= longest; i++)
                {
                    // if (x >= 0 && x < bmpW && y >= 0 && y < bmpH)
                    *pBackBuffer = color;
                    numerator += shortest;
                    if (!(numerator < longest))
                    {
                        numerator -= longest;
                        pBackBuffer += dx1 + bmpWdy1;
                    }
                    else
                        pBackBuffer += dx2 + bmpWdy2;
                }
            }
            // Po zmianie pikseli jest konieczne wywołanie metody AddDirtyRect, która wizualnie aktualizuje bitmapę.
            if (w >= 0)
            {
                if (h >= 0)
                    bitmap.AddDirtyRect(new Int32Rect(x1, y1, w + 1, h + 1));
                else
                    bitmap.AddDirtyRect(new Int32Rect(x1, y2, w + 1, -h + 1));
            }
            else
            {
                if (h >= 0)
                    bitmap.AddDirtyRect(new Int32Rect(x2, y1, -w + 1, h + 1));
                else
                    bitmap.AddDirtyRect(new Int32Rect(x2, y2, -w + 1, -h + 1));
            }
        }

        // robi to samo co BresenhamLine1, ale wskazuje obiektowi WriteableBitmap do zaktualizowania tylko punkty należące do odcinka, a nie cały prostokąt, którego przekątną jest odcinek
        public static void BresenhamLine2(WriteableBitmap bitmap, Point start, Point end, int color)
        {
            int x1 = (int)start.X, y1 = (int)start.Y, x2 = (int)end.X, y2 = (int)end.Y;
            // algorytm: https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
            int x = x1, y = y1;
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            int bmpW = bitmap.PixelWidth; // bmpH = bitmap.PixelHeight;
            int bmpWdy1 = bmpW * dy1, bmpWdy2 = bmpW * dy2;
            unsafe
            {
                int* pBackBuffer = (int*)bitmap.BackBuffer + x + bmpW * y;
                for (int i = 0; i <= longest; i++)
                {
                    // if (x >= 0 && x < bmpW && y >= 0 && y < bmpH)
                    *pBackBuffer = color;
                    bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                    numerator += shortest;
                    if (!(numerator < longest))
                    {
                        numerator -= longest;
                        x += dx1;
                        y += dy1;
                        pBackBuffer += dx1 + bmpWdy1;
                    }
                    else
                    {
                        x += dx2;
                        y += dy2;
                        pBackBuffer += dx2 + bmpWdy2;
                    }
                }
            }
        }
    }
}
