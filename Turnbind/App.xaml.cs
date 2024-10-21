using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Exceptions;
using Turnbind.Action;
using Turnbind.ViewModel;
using Turnbind.View;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Sinks.RichTextBox.Themes;
using System.Windows.Controls;
using Turnbind.Repository;

namespace Turnbind;

public partial class App : Application
{
    static readonly IHost m_host;

    public static bool IsInDebug { get; }

    public static T? GetService<T>() where T : class => m_host.Services.GetService<T>();

    public static T GetRequiredService<T>() where T : class => m_host.Services.GetRequiredService<T>();

    public static IConfiguration Configuration => GetRequiredService<IConfiguration>();

    static IServiceCollection AddSerilog(IServiceCollection services, RichTextBox? logTextBox, LogEventLevel logLvl) =>
        services.AddSerilog(
            config =>
            {
                {
                    var enrich = config.Enrich;
                    enrich.FromLogContext();
                    enrich.WithExceptionDetails();
                }

                config.MinimumLevel.Is(logLvl)
                    .WriteTo.Console()
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(),
                        $"logs.json",
                        fileSizeLimitBytes: 1_000_000,
                        rollOnFileSizeLimit: true
                    );

                if (logTextBox is { })
                    config.WriteTo.RichTextBox(
                        logTextBox,
                        theme: RichTextBoxConsoleTheme.Colored
                    );
            }
        );

    static Lazy<App> m_currentLazy = new();

    public static new App Current => m_currentLazy.Value;

    static App()
    {
        var builder = Host.CreateApplicationBuilder();

        SettingsRepository repo = new();
        var settings = repo.Settings;
        LogTextBlock? logTextBlock = null;
        var services = builder.Services
            .AddSingleton<InputAction>()
            .AddSingleton<ProcessWindowAction>()
            .AddSingleton<TurnTickAction>()
            .AddSingleton<TurnAction>()
            .AddSingleton(repo)
            .AddTransient<MainWindowViewModel>()
            .AddTransient<BindControl>();

        if (builder.Environment.IsDevelopment()) IsInDebug = true;

        if (settings.EnableConsole)
        {
            logTextBlock = new();
            services.AddSingleton(logTextBlock);
        }

        AddSerilog(services, logTextBlock?.LogTextBox, settings.LogEventLevel);

        m_host = builder.Build();
        m_host.Start();
    }

    public App() => m_currentLazy = new(this);

    void OnStartup(object sender, StartupEventArgs e) => 
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

    void OnExit(object sender, ExitEventArgs e) => m_host.StopAsync().ContinueWith(_ => m_host.Dispose()).Wait();

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        GetRequiredService<ILogger<App>>().LogError(e.Exception, "Unhandled exception");
        MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
