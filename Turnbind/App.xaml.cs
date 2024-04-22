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

namespace Turnbind;

public partial class App : Application
{
    IHost m_host = Host.CreateDefaultBuilder().Build();

    public static new App Current => (App)Application.Current;

    public static T? GetService<T>() where T : class => Current.m_host.Services.GetService<T>();

    public static T GetRequiredService<T>() where T : class => Current.m_host.Services.GetRequiredService<T>();

    public static IConfiguration Configuration => GetRequiredService<IConfiguration>();

    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetPriorityClass(nint hProcess, uint dwPriorityClass);

    void OnStartup(object sender, StartupEventArgs e)
    {
        SetPriorityClass(Process.GetCurrentProcess().Handle, 0x00000080);

        var builder = Host.CreateApplicationBuilder(e.Args);
        var config = builder.Configuration;

        {
            var services = builder.Services;
            LogTextBlock? logTextBlock = null;

            if (config["Console"] is { })
            { 
                logTextBlock = new LogTextBlock();
                services.AddSingleton(logTextBlock);
            }

            services.AddSerilog(
                loggerConfiguration =>
                {
                    loggerConfiguration.Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .WriteTo.Console()
                        .WriteTo.File(
                            new RenderedCompactJsonFormatter(),
                            $"logs.json",
                            fileSizeLimitBytes: 1_000_000,
                            rollOnFileSizeLimit: true
                        );

                    if (logTextBlock is { })                    
                        loggerConfiguration.WriteTo.RichTextBox(logTextBlock.LogTextBox);                    
                }
            )
                .AddSingleton<InputAction>()
                .AddSingleton<ProcessWindowAction>()
                .AddSingleton<TurnAction>()
                .AddSingleton(Settings.Load() ?? new())
                .AddSingleton<MainWindowViewModel>();
        }

        if (Enum.TryParse<LogLevel>(config["LogLevel"], out var level))
            builder.Logging.SetMinimumLevel(level);

        m_host = builder.Build();
        m_host.Start();

        var c = Configuration;
        Console.WriteLine(c.ToString());
    }

    void OnExit(object sender, ExitEventArgs e) => m_host.StopAsync()
        .ContinueWith(_ => m_host.Dispose())
        .Wait();

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        GetRequiredService<ILogger<App>>().LogError(e.Exception, "Unhandled exception");
        MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
