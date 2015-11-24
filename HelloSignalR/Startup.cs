using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Hosting;
using HelloSignalR.Connections;
using Microsoft.Extensions.Logging;
using HelloSignalR.Providers;
using Microsoft.AspNet.Http;
using System;
using Microsoft.AspNet.Authentication.JwtBearer;
using System.Threading.Tasks;

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

            var hostname = "http://localhost:5000/";

            // Add token issuing middleware.
            app.UseOpenIdConnectServer(config =>
            {
                // Your Auhentication server.
                config.Provider = new AuthenticationProvider();
                config.Issuer = new Uri(hostname);

                if (env.IsDevelopment())
                {
                    // For not requiring HTTPS in localhost.
                    config.AllowInsecureHttp = true;
                    config.ApplicationCanDisplayErrors = true;
                }
            });

            app.UseJwtBearerAuthentication(options =>
            {
                // We need this to enable authentication for all requests.
                options.AutomaticAuthenticate = true;

                // Your audience that has to be allowed by your Authority server.
                options.Audience = hostname;

                // Authority that issued your token (i.e. your identity server).
                options.Authority = hostname;

                options.Events = new JwtBearerEvents
                {
                    // For SignalR connections default Authorization header does not work,
                    // Because there are no Headers in WebSockets specification
                    // This is why we have to implement our own way to retrieve token
                    // Which can be as easy as adding it to query string
                    OnReceivingToken = context =>
                    {
                        string token = null;
                        // TODO: Maybe these constants are defined somewhere in framework already?
                        const string AuthorizationKey = "Authorization";
                        const string BearerKey = "Bearer";

                        // First, try to extract token from 'Authorization: Bearer {token}' header
                        if (context.Request.Headers.ContainsKey(AuthorizationKey))
                        {
                            var bearer = context.Request.Headers[AuthorizationKey].ToString();

                            var start = $"{BearerKey} ";
                            if (bearer.StartsWith(start) && bearer.Length > start.Length)
                            {
                                token = bearer.Substring(start.Length);
                            }
                        }

                        // If the token is not there, look at '?token={token}' query string value
                        token = token ?? context.Request.Query["token"];

                        // Whichever was found, assign it to the context
                        context.Token = token;
                        return Task.FromResult(true);
                    }
                };

                if (env.IsDevelopment())
                {
                    // For not requiring HTTPS in localhost.
                    options.RequireHttpsMetadata = false;
                }
            });

            // Add WebSockets handling for SignalR to support it.
            app.UseWebSockets();

            app.UseSignalR<SimpleConnection>("/signalr");

            app.Use(async (context, next) =>
             {
                 var identity = context.User.Identity;
                 if (!identity.IsAuthenticated)
                 {
                     await next();
                 }
                 else {
                     await context.Response.WriteAsync($"Username: {identity.Name}");
                 }
             });

            // Configure the HTTP request pipeline for our front-end files.
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
