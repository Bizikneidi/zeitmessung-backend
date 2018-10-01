using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeMeasurement_Backend.Handlers;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Make sure database is up and running
            using (var ctx = new TimeMeasurementDbContext())
            {
                ctx.Database.EnsureCreated();
            }

            app.UseWebSockets();
            //Register Custom Connection Handling
            app.Use(async (context, next) =>
            {
                //Get request path
                var path = context.Request.Path.Value.Split('/');
                //Check if request is WS and path requestPath has value
                if (context.WebSockets.IsWebSocketRequest && path.Length == 2)
                {
                    //admin / station / viewer
                    string requestPath = path[1].ToLower();
                    //get connected websocket
                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                    //Pass ws to correct handler
                    switch (requestPath)
                    {
                        case "admin":
                            await AdminHandler.Instance.SetAdminAsync(ws);
                            break;
                        case "station":
                            await StationHandler.Instance.SetStationAsync(ws);
                            break;
                        case "viewer":
                            await ViewerHandler.Instance.AddViewerAsync(ws);
                            break;
                    }
                }
                else
                {
                    //Pass to next handler (registered by ASP)
                    await next.Invoke();
                }
            });

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }
    }
}