using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Paint_Clone.Transform2d.Views;
using Paint_Clone.BasicDrawingMode.Views;
using Paint_Clone.BezierCurveMode.Views;
using Paint_Clone.ColorSpacesMode.Views;
using Paint_Clone.DigitalFiltersMode.Views;
using Paint_Clone.FileFormatsMode.Views;
using Paint_Clone.MorphologicalFiltersMode.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Paint_Clone.AppWindow.ViewModels;

public partial class MainWindowViewModel: ObservableObject
{
    readonly IServiceProvider serviceProvider;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        CurrentView = serviceProvider.GetRequiredService<BasicDrawingView>();
    }

    [ObservableProperty]
    UserControl currentView;

    [RelayCommand]
    void ShowBasicDrawingView() => CurrentView = serviceProvider.GetRequiredService<BasicDrawingView>();

    [RelayCommand]
    void ShowColorSpacesView() => CurrentView = serviceProvider.GetRequiredService<ColorSpacesView>();

    [RelayCommand]
    void ShowFileFormatsView() => CurrentView = serviceProvider.GetRequiredService<FileFormatsView>();

    [RelayCommand]
    void ShowDigitalFiltersView() => CurrentView = serviceProvider.GetRequiredService<DigitalFiltersView>();

    [RelayCommand]
    void ShowMorphologicalFiltersView() => CurrentView = serviceProvider.GetRequiredService<MorphologicalFiltersView>();

    [RelayCommand]
    void ShowBezierCurveModeView() => CurrentView = serviceProvider.GetRequiredService<BezierCurveView>();

    [RelayCommand]
    void Show2dTransformModeView() => CurrentView = serviceProvider.GetRequiredService<Transform2dView>();
}
