using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Paint_Clone.BezierCurveMode.Utils
{
    public static class Bezier
    {
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
                        *(p + x + wid * y) = color;
                        bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
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
                int x = (int)curDrawPt.X, y = (int)curDrawPt.Y;
                *(p + x + wid * y) = color;
                bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                for (t += deltaT; t <= 1.0; t += deltaT)
                {
                    prevDrawPt = curDrawPt;
                    curDrawPt = CalculateDrawPoint(temp, points, t);
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
            int bmpW = bitmap.PixelWidth;
            int bmpWdy1 = bmpW * dy1, bmpWdy2 = bmpW * dy2;
            unsafe
            {
                int* pBackBuffer = (int*)bitmap.BackBuffer + x + bmpW * y;
                for (int i = 0; i <= longest; i++)
                {
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

        public static void BresenhamLine2(WriteableBitmap bitmap, Point start, Point end, int color)
        {
            int x1 = (int)start.X, y1 = (int)start.Y, x2 = (int)end.X, y2 = (int)end.Y;
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
            int bmpW = bitmap.PixelWidth; 
            int bmpWdy1 = bmpW * dy1, bmpWdy2 = bmpW * dy2;
            unsafe
            {
                int* pBackBuffer = (int*)bitmap.BackBuffer + x + bmpW * y;
                for (int i = 0; i <= longest; i++)
                {
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
