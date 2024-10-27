using System;

namespace Paint_Clone.ColorSpacesMode.Utils
{
    public class ColorModelConverter
    {
        public static (int cyan, int magenta, int yellow, int black) FromRgbToCmyk(int red, int green, int blue)
        {
            int max = Math.Max(red, Math.Max(green, blue));
            if (max == 0)
                return (0, 0, 0, 100); // Zwraca czarny

            double black = 1.0 - max / 255.0; // Zmiana na double
            double c = (1.0 - red / 255.0 - black) / (1.0 - black) * 100;
            double m = (1.0 - green / 255.0 - black) / (1.0 - black) * 100;
            double y = (1.0 - blue / 255.0 - black) / (1.0 - black) * 100;

            return ((int)Math.Round(c), (int)Math.Round(m), (int)Math.Round(y), (int)Math.Round(black * 100));
        }

        public static (int hue, int saturation, int value) FromCmykToHSV(int cyan, int magenta, int yellow, int black)
        {
            // Przeliczenie CMYK na RGB
            double r = 255 * (1 - cyan / 100.0) * (1 - black / 100.0);
            double g = 255 * (1 - magenta / 100.0) * (1 - black / 100.0);
            double b = 255 * (1 - yellow / 100.0) * (1 - black / 100.0);

            // Normalizacja
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            double delta = max - min;

            // Obliczanie Hue
            int hue = 0;
            if (delta > 0)
            {
                if (max == rNorm)
                    hue = (int)(60 * ((gNorm - bNorm) / delta) % 6);
                else if (max == gNorm)
                    hue = (int)(60 * ((bNorm - rNorm) / delta) + 120);
                else if (max == bNorm)
                    hue = (int)(60 * ((rNorm - gNorm) / delta) + 240);
            }

            // Upewnienie się, że hue jest w odpowiednim zakresie
            if (hue < 0)
                hue += 360;

            // Obliczanie Saturation
            int saturation = (max == 0) ? 0 : (int)((delta / max) * 100);

            // Obliczanie Value
            int value = (int)(max * 100);

            return (hue, saturation, value);
        }




        public static (int red, int green, int blue) FromHSVToRGB(int hue, int saturation, int value)
        {
            double s = saturation / 100.0;
            double v = value / 100.0;

            double c = v * s; // Chroma
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

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
