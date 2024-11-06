using Microsoft.Extensions.DependencyInjection;
using Paint_Clone.AppWindow.ViewModels;
using Paint_Clone.AppWindow.Views;
using Paint_Clone.BasicDrawingMode.ViewModels;
using Paint_Clone.BasicDrawingMode.Views;
using Paint_Clone.ColorSpacesMode.ViewModels;
using Paint_Clone.ColorSpacesMode.Views;
using Paint_Clone.FileFormatsMode.Viewmodels;
using Paint_Clone.FileFormatsMode.Views;
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

        //Viewmodels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<BasicDrawingViewModel>();
        services.AddSingleton<ColorSpaceViewModel>();
        services.AddSingleton<FileFormatsViewModel>();
    }
}
