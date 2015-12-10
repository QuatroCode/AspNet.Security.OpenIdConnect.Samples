using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignalrResourceServer.Connections;
using System.Threading.Tasks;

namespace SignalrResourceServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddCaching();
            services.AddSignalR();
            services.AddCors();
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

            app.UseCors(config =>
            {
                config.AllowAnyOrigin();
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowCredentials();
            });

            // Add a new middleware validating access tokens.
            app.UseJwtBearerAuthentication(options =>
            {
                // Automatic authentication must be enabled
                // for SignalR to receive the access token.
                options.AutomaticAuthenticate = true;

                // Automatically disable the HTTPS requirement for development scenarios.
                options.RequireHttpsMetadata = !environment.IsDevelopment();

                // Note: the audience must correspond to the address of the SignalR server.
                options.Audience = "http://localhost:5004/";

                // Note: the authority must match the address of the identity server.
                options.Authority = "http://localhost:5005/";

                options.Events = new JwtBearerEvents
                {
                    // Note: for SignalR connections, the default Authorization header does not work,
                    // because the WebSockets JS API doesn't allow setting custom parameters.
                    // To work around this limitation, the access token is retrieved from the query string.
                    OnReceivingToken = context =>
                    {
                        // Note: when the token is missing from the query string,
                        // context.Token is null and the JWT bearer middleware will
                        // automatically try to retrieve it from the Authorization header.
                        context.Token = context.Request.Query["access_token"];

                        return Task.FromResult(0);
                    }
                };
            });

            app.UseWebSockets();

            app.UseSignalR<SimpleConnection>("/signalr");
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
