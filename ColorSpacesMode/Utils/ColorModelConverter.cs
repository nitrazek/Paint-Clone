using System;

namespace Paint_Clone.ColorSpacesMode.Utils
{
    public class ColorModelConverter
    {
        public static (int cyan, int magenta, int yellow, int black) FromRgbToCmyk(int red, int green, int blue)
        {
            int max = Math.Max(red, Math.Max(green, blue));
            if (max == 0)
                return (0, 0, 0, 100); 

            double black = 1.0 - max / 255.0; 
            double c = (1.0 - red / 255.0 - black) / (1.0 - black) * 100;
            double m = (1.0 - green / 255.0 - black) / (1.0 - black) * 100;
            double y = (1.0 - blue / 255.0 - black) / (1.0 - black) * 100;

            return ((int)Math.Round(c), (int)Math.Round(m), (int)Math.Round(y), (int)Math.Round(black * 100));
        }

        public static (int hue, int saturation, int value) FromCmykToHSV(int cyan, int magenta, int yellow, int black)
        {
            double r = 255 * (1 - cyan / 100.0) * (1 - black / 100.0);
            double g = 255 * (1 - magenta / 100.0) * (1 - black / 100.0);
            double b = 255 * (1 - yellow / 100.0) * (1 - black / 100.0);

            r = r / 255;
            g = g / 255;
            b = b / 255;

            double cmax = Math.Max(r, Math.Max(g, b));
            double cmin = Math.Min(r, Math.Min(g, b));
            double diff = cmax - cmin;
            double h = -1, s = -1;

            if (cmax == cmin) 
                h = 0;

            else if (cmax == r) 
                h = (60 * ((g - b) / diff) + 360) % 360;

            else if (cmax == g)
                h = (60 * ((b - r) / diff) + 120) % 360;

            else if (cmax == b)
                h = (60 * ((r - g) / diff) + 240) % 360;

            if (cmax == 0)
                s = 0;

            else
                s = (diff / cmax) * 100;

            double v = cmax * 100;

            return ((int)h, (int)s, (int)v);

        }




        public static (int red, int green, int blue) FromHSVToRGB(int hue, int saturation, int value)
        {
            double s = saturation / 100.0;
            double v = value / 100.0;

            double c = v * s;
            double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
            double m = v - c;

            (double r, double g, double b) result;

            switch (hue)
            {
                case < 60: result = (c, x, 0); break;
                case < 120: result = (x, c, 0); break;
                case < 180: result = (0, c, x); break;
                case < 240: result = (0, x, c); break;
                case < 300: result = (x, 0, c); break;
                default: result = (c, 0, x); break;
            }

            return ((int)((result.r + m) * 255), (int)((result.g + m) * 255), (int)((result.b + m) * 255));
        }
    }
}
