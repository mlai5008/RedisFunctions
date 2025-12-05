using CommonResources.CacheOptions;
using CommonResources.Models;
using CommonResources.Services;
using CommonResources.Services.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebApiApp.Services;
using WebApiApp.Services.Interfaces;

namespace WebApiApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddOptions<RedisCacheOptions>().BindConfiguration(nameof(RedisCacheOptions));
            builder.Services.AddSingleton(typeof(IGenericHandlerService<>), typeof(GenericHandlerService<>));
            builder.Services.AddSingleton(typeof(IGenericRedisMessageBrokerService<>), typeof(GenericRedisMessageBrokerService<>));
            var cacheOptions = builder.Services?.BuildServiceProvider().GetService<IOptions<RedisCacheOptions>>()?.Value;
            if (cacheOptions == null) throw new NullReferenceException("RedisCacheOptions is NULL");
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(string.IsNullOrWhiteSpace(cacheOptions.Configuration) ? string.Empty : cacheOptions.Configuration);
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            var handlerService = app.Services.GetRequiredService<IGenericHandlerService<Product>>();
            var messageBroker = app.Services.GetRequiredService<IGenericRedisMessageBrokerService<Product>>();
            await messageBroker.SubscribeAsync((channel, message) =>
            {
                handlerService.HandlerMessage(channel, message);
            });

            app.UseAuthorization();
            app.Run();
        }
    }
}
