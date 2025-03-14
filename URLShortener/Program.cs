using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using URLShortener.DTOs;
using URLShortener.Infrastructure.cache;
using URLShortener.Infrastructure.Database;
using URLShortener.Models;
using URLShortener.Repository;
using URLShortener.Services;
using URLShortener.Validators;

var builder = WebApplication.CreateBuilder(args);


// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration);
});

// Cache Service
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validators
builder.Services.AddScoped<IValidator<ShortenedUrlDto>, UrlShortCodeValidator>();

// Services
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

// Repository
builder.Services.AddScoped<IUrlShortenerRepository, UrlShortenerRepository>();

// Entity Framework   
builder.Services.AddDbContext<StoreContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoreConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
