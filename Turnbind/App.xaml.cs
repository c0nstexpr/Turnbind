using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Exceptions;
using Turnbind.Action;
using Turnbind.Model;
using Autofac.Extensions.DependencyInjection;
using Turnbind.ViewModel;
using Turnbind.View;

namespace Turnbind;

public partial class App : Application
{
    static readonly LogTextBlock m_logTextBlock = new();

    static readonly IHost m_host = Host.CreateDefaultBuilder()
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureAppConfiguration(c => c.SetBasePath(AppContext.BaseDirectory))
        .UseSerilog(
            (context, services, loggerConfiguration) => loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.RichTextBox(m_logTextBlock.LogTextBox)
                .WriteTo.File(
                    new RenderedCompactJsonFormatter(),
                    $"logs.json",
                    fileSizeLimitBytes: 1_000_000,
                    rollOnFileSizeLimit: true
                )
        )
        .ConfigureServices(
            (_, services) =>
            {
                services.AddSingleton<InputAction>();
                services.AddSingleton<ProcessWindowAction>();
                services.AddSingleton<TurnAction>();
                services.AddSingleton(Settings.Load() ?? new());
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton(m_logTextBlock);
            }
        )
        .Build();

    public static T GetService<T>() where T : class => m_host.Services.GetRequiredService<T>();

    void OnStartup(object sender, StartupEventArgs e) => m_host.Start();

    void OnExit(object sender, ExitEventArgs e) => m_host.StopAsync()
        .ContinueWith(_ => m_host.Dispose())
        .Wait();

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Logger.Error(e.Exception, "Unhandled exception");
        MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
