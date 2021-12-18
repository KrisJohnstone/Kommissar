using Kommissar.Model;
using Kommissar.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Kommissar;
public class Program
{
    public static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();

        try
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger.LogInformation("App Starting");
            
            // entry to run app
            await serviceProvider.GetService<App>().Run(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IServiceCollection ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var services = new ServiceCollection();
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"))
                .AddLogging(builder =>
                {
                    builder.AddSerilog();
                });

        // Add Services:
        services.AddTransient<IKubernetes, KubernetesService>();
        services.AddSingleton<KommissarRepo>();

        // Add App
        services.AddTransient<App>();
        return services;
    }
}
