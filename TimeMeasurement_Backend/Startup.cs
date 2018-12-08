using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeMeasurement_Backend.Networking;
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

            //Create handlers for certain types of websocket connections
            var adminHandler = new AdminHandler();
            var stationHandler = new StationHandler();
            var viewerHandler = new ViewerHandler();
            var participantHandler = new ParticipantHandler();

            app.UseHttpsRedirection();
            app.UseHsts();
            app.UseWebSockets();

            //Register Custom Connection Handling
            app.Use(async (context, next) =>
            {
                //Get request path
                var path = context.Request.Path.Value.Split('/');
                //Check if request is WS and path requestPath has value
                if (context.WebSockets.IsWebSocketRequest && path.Length == 2)
                {
                    //admin / station / viewer / participant
                    string requestPath = path[1].ToLower();
                    //get connected websocket
                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                    //Pass ws to correct handler
                    switch (requestPath)
                    {
                        case "admin":
                            await adminHandler.SetAdminAsync(ws);
                            break;
                        case "station":
                            await stationHandler.SetStationAsync(ws);
                            break;
                        case "viewer":
                            await viewerHandler.AddViewerAsync(ws);
                            break;
                        case "participant":
                            await participantHandler.AddPotentialParticipant(ws);
                            break;
                    }
                }
                else
                {
                    //Pass to next handler (registered by the runtime)
                    await next.Invoke();
                }
            });

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddHttpsRedirection(options => { options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect; });
        }
    }
}
