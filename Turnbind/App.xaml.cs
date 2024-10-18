using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Exceptions;
using Turnbind.Action;
using Turnbind.Model;
using Turnbind.ViewModel;
using Turnbind.View;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Sinks.RichTextBox.Themes;
using System.Windows.Controls;

namespace Turnbind;

public partial class App : Application
{
    static readonly IHost m_host;

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

    static App()
    {
        Settings settings = new();
        LogTextBlock? logTextBlock = null;
        var builder = Host.CreateApplicationBuilder();

        var config = builder.Configuration;
        var services = builder.Services
            .AddSingleton<InputAction>()
            .AddSingleton<ProcessWindowAction>()
            .AddSingleton<TurnTickAction>()
            .AddSingleton<TurnAction>()
            .AddScoped<Settings>()
            .AddTransient<MainWindowViewModel>()
            .AddTransient<BindControl>();

        config.Bind(settings);

        if (settings.Console)
        {
            logTextBlock = new();
            services.AddSingleton(logTextBlock);
        }

        AddSerilog(services, logTextBlock?.LogTextBox, settings.LogLevel);

        m_host = builder.Build();
        m_host.Start();
    }

    static Lazy<App> m_currentLazy = new();

    public static new App Current => m_currentLazy.Value;

    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetPriorityClass(nint hProcess, uint dwPriorityClass);

    public App() => m_currentLazy = new(this);

    void OnStartup(object sender, StartupEventArgs e) => SetPriorityClass(Process.GetCurrentProcess().Handle, 0x00000080);

    void OnExit(object sender, ExitEventArgs e) => m_host.StopAsync()
        .ContinueWith(_ => m_host.Dispose())
        .Wait();

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        GetRequiredService<ILogger<App>>().LogError(e.Exception, "Unhandled exception");
        MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
