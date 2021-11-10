using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GmodNET.API;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Serilog;
using Serilog.Core;
using GmodNET.Serilog.Sink;
using Microsoft.Extensions.DependencyInjection;
using GmodNET.Extensions.Hosting;

namespace TemplateModuleWeb
{
    public class TemplateModuleWeb : IModule
    {
        public string ModuleName => nameof(TemplateModuleWeb);

        public string ModuleVersion => "1.0.0";

        Logger logger;
        IHost webHost;
        GmodInteropService gmodInteropService;

        public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
        {
            logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.GmodSink()
                .CreateLogger();

            gmodInteropService = new GmodInteropService(lua);

            try
            {
                logger.Information("Creating and starting web host.");
                webHost = CreateHostBuilder().Build();
                webHost.Start();
                logger.Information("Web host was started.");
            }
            catch (Exception e)
            {
                logger.Fatal(e, $"Error was thrown while loading module {nameof(TemplateModuleWeb)}.");
            }
        }

        public void Unload(ILua lua)
        {
            if (webHost is not null)
            {
                logger.Information("Stopping web host.");
                webHost.StopAsync().Wait();
                webHost.Dispose();
                logger.Information("Web host was stoped.");
                gmodInteropService.Dispose(lua);
            }
        }

        // Host configuration builder
        IHostBuilder CreateHostBuilder() =>
            GmodNetHostBuilder.CreateDefaultBuilder<TemplateModuleWeb>(logger)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseSetting(WebHostDefaults.PreventHostingStartupKey, "true");
                        webBuilder.UseStartup<Startup>();
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<GmodInteropService>(gmodInteropService);
                    });
    }
}
