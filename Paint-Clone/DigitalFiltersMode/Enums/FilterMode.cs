using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint_Clone.DigitalFiltersMode.Enums;

public enum FilterMode
{
    Addition,
    Substract,
    Multiply,
    Divide,
    Brightness,
    GrayScaleAverage,
    GrayScaleMax,
    Smoothing,
    Median,
    Sobel,
    HighPass,
    Gaussian,
    Mask
}