using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using URLShortener.DTOs;
using URLShortener.Infrastructure.cache;
using URLShortener.Infrastructure.Database;
using URLShortener.Repository;
using URLShortener.Services;
using URLShortener.Validators;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);


// Redis
var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
var options = ConfigurationOptions.Parse(redisConfiguration);
options.AbortOnConnectFail = false;
//options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
//options.ConnectTimeout = 10000; // 10 seconds
//options.ConnectRetry = 3; // Number of connection retry attempts
//options.SyncTimeout = 10000;
options.DefaultDatabase = 0;

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    try
    {
        var multiplexer = ConnectionMultiplexer.Connect(options);

        multiplexer.ConnectionFailed += (sender, args) =>
        {
            Console.WriteLine($"Redis Connection Failed: {args.Exception.Message}");
            // Log or handle connection failures
        };

        multiplexer.ConnectionRestored += (sender, args) =>
        {
            Console.WriteLine($"Redis Connection Restored: {args.ConnectionType}");
        };

        return multiplexer;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fatal Redis connection error: {ex.Message}");
        // Consider more robust error handling or logging
        throw;
    }
});

// Cache Service
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validators
builder.Services.AddScoped<IValidator<ShortcodeUrlRequestDto>, UrlShortCodeValidator>();
builder.Services.AddScoped<IValidator<ShortenedUrlDto>, OriginalUrlValidator>();

// Services
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

// Repository
builder.Services.AddScoped<IUrlShortenerRepository, UrlShortenerRepository>();

// Entity Framework   
builder.Services.AddDbContext<StoreContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoreConnection"));
});

// BaseUrl
builder.Services.AddSingleton(sp => builder.Configuration["BaseUrl"]);

// Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("NewPolicy", app =>
    {
        app.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StoreContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("NewPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
