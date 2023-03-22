using ChatAPI.Infrastructure;

namespace ChatAPI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ConnectionManager>();
            services.AddSingleton<ChatHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ChatHandler handler)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            };

            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<WebSocketMiddleware>(handler);
        }
    }
}
