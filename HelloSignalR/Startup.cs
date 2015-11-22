using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Hosting;
using HelloSignalR.Connections;

namespace HelloSignalR
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();

            if (env.IsDevelopment())
            {
                // In case any errors occur.
                app.UseDeveloperExceptionPage();
            }

            // Add WebSockets handling for SignalR to support it.
            app.UseWebSockets();

            app.UseSignalR<SimpleConnection>("/signalr");

            // Configure the HTTP request pipeline for our front-end files.
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
