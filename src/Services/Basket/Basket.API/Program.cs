using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Basket.API.Data;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Marten;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Application Services
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

//Data Services
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
//builder.Services.AddScoped<IBasketRepository, CachedBasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
//builder.Services.AddScoped<IBasketRepository>(provider =>
//{
//    var basketRepository=provider.GetRequiredService<BasketRepository>();
//    return new CachedBasketRepository(basketRepository,provider.GetRequiredService<IDistributedCache>());
//});


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");

});


//Async Communication Services

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
     .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
     .AddRedis(builder.Configuration.GetConnectionString("Redis")!);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapCarter();
app.UseExceptionHandler(options => { });
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter=UIResponseWriter.WriteHealthCheckUIResponse
} );

app.Run();