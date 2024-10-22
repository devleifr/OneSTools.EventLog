using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneSTools.EventLog.Exporter.Core;
using OneSTools.EventLog.Exporter.Core.ClickHouse;
using OneSTools.EventLog.Exporter.Core.ElasticSearch;
using OneSTools.EventLog.Exporter.Core.StdOut;

namespace OneSTools.EventLog.Exporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration(c => {
                //    c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                //    c.AddJsonFile("appsettings.json");
                //})
                .UseWindowsService()
                .UseSystemd()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var logPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, "log.txt");
                    logging.AddFile(logPath);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var storageType = hostContext.Configuration.GetValue("Exporter:StorageType", StorageType.None);

                    switch (storageType)
                    {
                        case StorageType.ClickHouse:
                            services.AddTransient<IEventLogStorage, ClickHouseStorage>();
                            break;
                        case StorageType.ElasticSearch:
                            services.AddTransient<IEventLogStorage, ElasticSearchStorage>();
                            break;
                        case StorageType.StdOut:
                            services.AddTransient<IEventLogStorage, JSONConsoleExporter>();
                            break;
                        case StorageType.None:
                            throw new Exception("You must set StorageType parameter before starting the exporter");
                        default:
                            throw new Exception($"{storageType} is not available value of StorageType enum");
                    }

                    services.AddTransient<EventLogExporter>();
                    services.AddHostedService<EventLogExporterService>();
                });
        }
    }
}
