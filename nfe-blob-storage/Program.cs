using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using System;
using System.IO;

namespace nfe_blob_storage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string loggerTemplate = @"{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u4}]<{ThreadId}> [{SourceContext:l}] {Message:lj}{NewLine}{Exception}";

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var logfile = Path.Combine(baseDir, "App_Data", "logs", "log.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.FromLogContext()
                .WriteTo.File(logfile, LogEventLevel.Information, loggerTemplate,
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90)
                .CreateLogger();

            try
            {
                Log.Information("====================================================================");
                Log.Information($"Application Starts. Version: {System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version}");
                Log.Information($"Application Directory: {baseDir}");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application terminated unexpectedly");
            }
            finally
            {
                Log.Information("====================================================================\r\n");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
                })
                .UseSerilog();
    }
}
