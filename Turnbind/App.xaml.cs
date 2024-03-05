using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Exceptions;

namespace Turnbind;

public partial class App : Application
{
    static readonly IHost m_host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => c.SetBasePath(AppContext.BaseDirectory))
        .UseSerilog(
            (context, services, loggerConfiguration) => loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.File(
                    new RenderedCompactJsonFormatter(),
                    $"logs.json",
                    rollOnFileSizeLimit: true
                )
        )
        .Build();

    public static T GetRequiredService<T>() where T : class => m_host.Services.GetRequiredService<T>();

    void OnStartup(object sender, StartupEventArgs e) => m_host.Start();

    void OnExit(object sender, ExitEventArgs e)
    {
        m_host.StopAsync().Wait();
        m_host.Dispose();
    }

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) =>
        Log.Logger.Error(e.Exception, "Unhandled exception");
}
