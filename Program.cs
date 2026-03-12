using ECommerceRanking.Services;
using Microsoft.OpenApi;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerceRanking API", Version = "v1" });
});

// 配置 Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
Console.WriteLine($"Redis connection: {redisConnectionString}");
var redis = ConnectionMultiplexer.Connect(redisConnectionString);

// Register Redis service
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddScoped<IRedisService, RedisService>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// 默认路由到 index.html（必须在最后）
app.MapFallbackToFile("index.html");

// Health check endpoint for Kubernetes
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.Run();
