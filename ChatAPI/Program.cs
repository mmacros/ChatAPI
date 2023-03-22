using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ChatAPI.Infrastructure;
using StackExchange.Redis;

public class Program
{

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        ChatAPI.Config config = new ChatAPI.Config();
        config.RedisHost = builder.Configuration.GetSection("Redis")["Host"].Trim();
        config.RedisPort = builder.Configuration.GetSection("Redis")["Port"].Trim();
        config.RedisUser = builder.Configuration.GetSection("Redis")["User"].Trim();
        config.RedisPass = builder.Configuration.GetSection("Redis")["Password"].Trim();
        config.RedisTimeout = Int32.Parse(builder.Configuration.GetSection("Redis")["MessageTimeout"].Trim());

        // Add services to the container.
        builder.Services.AddCors(options =>
        {
            var allowAnyOrigins = "allowAnyOrigins";
            options.AddPolicy(name: allowAnyOrigins,
                              builder =>
                              {
                                  builder.AllowAnyHeader();
                                  builder.AllowAnyOrigin();
                              });
        });
        builder.Services.AddTransient<ConnectionManager>();
        builder.Services.AddSingleton<WebSocketHandler, ChatHandler>();

        string connectionString = $"{config.RedisHost}:{config.RedisPort}";
        builder.Services.AddSingleton(config);
        var options = ConfigurationOptions.Parse(connectionString); // host1:port1, host2:port2, ...
        options.Password = config.RedisPass;
        IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(options);

        builder.Services.AddSingleton(multiplexer);


        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
            app.UseSwagger();
            app.UseSwaggerUI();
        // }

        // app.UseHttpsRedirection();
        app.UseRouting();

        app.UseWebSockets(new WebSocketOptions {
            KeepAliveInterval = TimeSpan.FromSeconds(120),
        });

        app.UseMiddleware<WebSocketMiddleware>();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();

    }
}