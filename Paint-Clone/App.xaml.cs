using Microsoft.Extensions.DependencyInjection;
using Paint_Clone.AppWindow.ViewModels;
using Paint_Clone.AppWindow.Views;
using Paint_Clone.BasicDrawingMode.ViewModels;
using Paint_Clone.BasicDrawingMode.Views;
using Paint_Clone.BezierCurveMode.Views;
using Paint_Clone.ColorSpacesMode.ViewModels;
using Paint_Clone.ColorSpacesMode.Views;
using Paint_Clone.DigitalFiltersMode.Viewmodels;
using Paint_Clone.DigitalFiltersMode.Views;
using Paint_Clone.FileFormatsMode.Viewmodels;
using Paint_Clone.FileFormatsMode.Views;
using Paint_Clone.MorphologicalFiltersMode.Viewmodels;
using Paint_Clone.MorphologicalFiltersMode.Views;
using Paint_Clone.Transform2d.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Paint_Clone;

public partial class App : Application
{
    public App()
    {
        ServiceCollection serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        //Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<BasicDrawingView>();
        services.AddSingleton<ColorSpacesView>();
        services.AddSingleton<FileFormatsView>();
        services.AddSingleton<DigitalFiltersView>();
        services.AddSingleton<MorphologicalFiltersView>();
        services.AddSingleton<BezierCurveView>();
        services.AddSingleton<Transform2dView>();

        //Viewmodels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<BasicDrawingViewModel>();
        services.AddSingleton<ColorSpaceViewModel>();
        services.AddSingleton<FileFormatsViewModel>();
        services.AddSingleton<DigitalFiltersViewModel>();
        services.AddSingleton<MorphologicalFiltersViewModel>();
    }
}
