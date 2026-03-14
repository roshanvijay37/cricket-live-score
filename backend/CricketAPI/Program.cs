using CricketAPI.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Register Redis
var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
if (!string.IsNullOrEmpty(redisUrl))
{
    try
    {
        // Parse redis:// URL format from Railway
        var uri = new Uri(redisUrl);
        var host = uri.Host;
        var port2 = uri.Port;
        var password = uri.UserInfo.Split(':').ElementAtOrDefault(1);

        var options = new ConfigurationOptions
        {
            EndPoints = { $"{host}:{port2}" },
            Password = password,
            AbortOnConnectFail = false,
            ConnectTimeout = 5000
        };
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Redis connection failed: {ex.Message}");
    }
}

// Register cricket service
builder.Services.AddScoped<ICricketService, CricketService>();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
