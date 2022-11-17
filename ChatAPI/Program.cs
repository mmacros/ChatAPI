using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

public class Program
{

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
        builder.Configuration.AddEnvironmentVariables();


        ChatAPI.Config config = new ChatAPI.Config();
        config.RedisHost = builder.Configuration.GetSection("Redis")["Host"].Trim();
        config.RedisPort = builder.Configuration.GetSection("Redis")["Port"].Trim();
        config.RedisUser = builder.Configuration.GetSection("Redis")["User"].Trim();
        config.RedisPass = builder.Configuration.GetSection("Redis")["Password"].Trim();
        config.RedisTimeout = Int32.Parse(builder.Configuration.GetSection("Redis")["MessageTimeout"].Trim());

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

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}