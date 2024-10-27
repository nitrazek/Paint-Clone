using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Paint_Clone.ColorSpacesMode.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Paint_Clone.ColorSpacesMode.ViewModels;

public partial class ColorSpaceViewModel : ObservableObject
{
    readonly static int VALUE_MIN = 0;
    readonly static int RGB_MAX = 255;
    readonly static int CMYK_MAX = 100;
    readonly static int HSV_MAX = 100;
    readonly static int HSV_HUE_MAX = 360;

    bool isChanging = false;

    [ObservableProperty]
    int rgbRed;
    [ObservableProperty]
    int rgbGreen;
    [ObservableProperty]
    int rgbBlue;

    [ObservableProperty]
    int cmykCyan;
    [ObservableProperty]
    int cmykMagenta;
    [ObservableProperty]
    int cmykYellow;
    [ObservableProperty]
    int cmykBlack = 100;

    [ObservableProperty]
    int hsvHue;
    [ObservableProperty]
    int hsvSaturation;
    [ObservableProperty]
    int hsvValue;

    [ObservableProperty]
    SolidColorBrush previewColor = new SolidColorBrush(Colors.Black);

    partial void OnRgbRedChanged(int value) { RgbRed = ValidateValue(value, VALUE_MIN, RGB_MAX); UpdateFromRGB(); }
    partial void OnRgbGreenChanged(int value) { RgbGreen = ValidateValue(value, VALUE_MIN, RGB_MAX); UpdateFromRGB(); }
    partial void OnRgbBlueChanged(int value) { RgbBlue = ValidateValue(value, VALUE_MIN, RGB_MAX); UpdateFromRGB(); }

    partial void OnCmykCyanChanged(int value) { CmykCyan = ValidateValue(value, VALUE_MIN, CMYK_MAX); UpdateFromCMYK(); }
    partial void OnCmykMagentaChanged(int value) { CmykMagenta = ValidateValue(value, VALUE_MIN, CMYK_MAX); UpdateFromCMYK(); }
    partial void OnCmykYellowChanged(int value) { CmykYellow = ValidateValue(value, VALUE_MIN, CMYK_MAX); UpdateFromCMYK(); }
    partial void OnCmykBlackChanged(int value) { CmykBlack = ValidateValue(value, VALUE_MIN, CMYK_MAX); UpdateFromCMYK(); }

    partial void OnHsvHueChanged(int value) { HsvHue = ValidateValue(value, VALUE_MIN, HSV_HUE_MAX); UpdateFromHSV(); }
    partial void OnHsvSaturationChanged(int value) { HsvSaturation = ValidateValue(value, VALUE_MIN, HSV_MAX); UpdateFromHSV(); }
    partial void OnHsvValueChanged(int value) { HsvValue = ValidateValue(value, VALUE_MIN, HSV_MAX); UpdateFromHSV(); }
    
    int ValidateValue(int value, int minValue, int maxValue)
    {
        if (value < minValue) return minValue;
        if (value > maxValue) return maxValue;
        return value;
    }

    void UpdateFromRGB()
    {
        if (isChanging) return;
        isChanging = true;

        (int cyan, int magenta, int yellow, int black) = ColorModelConverter.FromRgbToCmyk(RgbRed, RgbGreen, RgbBlue);
        CmykCyan = cyan;
        CmykMagenta = magenta;
        CmykYellow = yellow;
        CmykBlack = black;
        (int hue, int saturation, int value) = ColorModelConverter.FromCmykToHSV(cyan, magenta, yellow, black);
        HsvHue = hue;
        HsvSaturation = saturation;
        HsvValue = value;

        PreviewColor.Color = Color.FromScRgb(1, (float)RgbRed / 255, (float)RgbGreen / 255, (float)RgbBlue / 255);

        isChanging = false;
    }

    void UpdateFromCMYK()
    {
        if (isChanging) return;
        isChanging = true;

        (int hue, int saturation, int value) = ColorModelConverter.FromCmykToHSV(CmykCyan, CmykMagenta, CmykYellow, CmykBlack);
        HsvHue = hue;
        HsvSaturation = saturation;
        HsvValue = value;
        (int red, int green, int blue) = ColorModelConverter.FromHSVToRGB(hue, saturation, value);
        RgbRed = red;
        RgbGreen = green;
        RgbBlue = blue;

        PreviewColor.Color = Color.FromScRgb(1, (float)RgbRed / 255, (float)RgbGreen / 255, (float)RgbBlue / 255);

        isChanging = false;
    }

    void UpdateFromHSV()
    {
        if (isChanging) return;
        isChanging = true;

        (int red, int green, int blue) = ColorModelConverter.FromHSVToRGB(HsvHue, HsvSaturation, HsvValue);
        RgbRed = red;
        RgbGreen = green;
        RgbBlue = blue;
        (int cyan, int magenta, int yellow, int black) = ColorModelConverter.FromRgbToCmyk(RgbRed, RgbGreen, RgbBlue);
        CmykCyan = cyan;
        CmykMagenta = magenta;
        CmykYellow = yellow;
        CmykBlack = black;

        PreviewColor.Color = Color.FromScRgb(1, (float)RgbRed/255, (float)RgbGreen/255, (float)RgbBlue/255);

        isChanging = false;
    }
}
