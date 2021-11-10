using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TemplateModuleWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.Run(async context =>
            {
                GmodInteropService gmodInteropService = context.RequestServices.GetRequiredService<GmodInteropService>();
                List<string> players = await gmodInteropService.GetPlayersList();
                string response_text = "Hello! \nPlayers on server:";
                foreach (string player in players)
                {
                    response_text += "\n" + player;
                }
                context.Response.StatusCode = 200;
                context.Response.Headers.Add("Content-Type", "text/plain");
                await context.Response.WriteAsync(response_text);
            });
        }
    }
}
