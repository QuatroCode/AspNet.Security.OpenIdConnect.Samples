using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Hosting;
using HelloSignalR.Connections;
using Microsoft.Extensions.Logging;
using HelloSignalR.Providers;

namespace HelloSignalR
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddSignalR();
            services.AddCaching();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();

            if (env.IsDevelopment())
            {
                // In case any errors occur.
                app.UseDeveloperExceptionPage();

                if (env.IsDevelopment())
                {
                    loggerFactory.MinimumLevel = LogLevel.Information;
                    loggerFactory.AddConsole();
                    loggerFactory.AddDebug();
                }
            }

            logger.LogInformation($"Environment: {env.EnvironmentName}");

            // Add token issuing middleware.
            app.UseOpenIdConnectServer(config =>
            {
                config.Provider = new AuthenticationProvider();

                if (env.IsDevelopment())
                {
                    config.ApplicationCanDisplayErrors = true;
                    config.AllowInsecureHttp = true;
                }
            });

            // Add WebSockets handling for SignalR to support it.
            app.UseWebSockets();

            app.UseSignalR<SimpleConnection>("/signalr");

            // Configure the HTTP request pipeline for our front-end files.
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
