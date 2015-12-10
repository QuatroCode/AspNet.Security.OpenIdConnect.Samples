﻿using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer.Providers;

namespace IdentityServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddCaching();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment environment, ILoggerFactory factory)
        {
            factory.AddConsole();
            factory.AddDebug();

            app.UseIISPlatformHandler();

            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add a new middleware issuing access tokens.
            app.UseOpenIdConnectServer(options =>
            {
                options.Provider = new AuthenticationProvider();
            });

            //app.UseWelcomePage();
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
