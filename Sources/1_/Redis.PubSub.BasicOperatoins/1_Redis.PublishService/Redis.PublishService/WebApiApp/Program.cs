using CommonResources.CacheOptions;
using CommonResources.Models;
using CommonResources.Services;
using CommonResources.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebApiApp.Context;
using WebApiApp.Repositories;
using WebApiApp.Repositories.Interfaces;
using WebApiApp.Services;
using WebApiApp.Services.Interfaces;

namespace WebApiApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddOptions<RedisCacheOptions>().BindConfiguration(nameof(RedisCacheOptions));
            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("SqliteDB") ?? throw new InvalidOperationException("Connection string 'ApplicationContext' not found.")));
            builder.Services.AddScoped(typeof(IGenericRedisCacheService<>), typeof(GenericRedisCacheService<>));
            builder.Services.AddScoped(typeof(IGenericRedisMessageBrokerService<>), typeof(GenericRedisMessageBrokerService<>));
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            var cacheOptions = builder.Services?.BuildServiceProvider().GetService<IOptions<RedisCacheOptions>>()?.Value;
            if (cacheOptions == null) throw new NullReferenceException("RedisCacheOptions is NULL");
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp => {
                return ConnectionMultiplexer.Connect(string.IsNullOrWhiteSpace(cacheOptions.Configuration) ? string.Empty : cacheOptions.Configuration);
            });
            // добавление кэширования
            builder.Services.AddStackExchangeRedisCache(options => {                
                options.Configuration = string.IsNullOrWhiteSpace(cacheOptions.Configuration) ? string.Empty : cacheOptions.Configuration;
                options.InstanceName = cacheOptions.InstanceName;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            
            
            app.MapGet("/", () => "Hello World!");

            app.MapGet("/Product/{id}", async (Guid id, IGenericRepository<Product> productRepository) =>
            {
                var product = await productRepository.GetByIdAsinc(id);
                if (product != null) return $"Product: Id-{product.Id} Name-{product.Name} Price={product.Price}";
                return "Product not found";
            })
            .WithName("ProductById")
            .WithOpenApi();

            app.MapPost("/Product", async ([FromBody] ProductRequest productRequest, ILogger<Program> logger, IGenericRedisMessageBrokerService<Product> productRedisMessageBrokerService) =>
            {
                if (productRequest == null)
                {
                    throw new ArgumentNullException(nameof(productRequest));
                }

                var newProduct = new Product { Name = productRequest.Name, Price = productRequest.Price };
                newProduct.Id = Guid.NewGuid();

                var result = await productRedisMessageBrokerService.PublishAsync(newProduct);

                logger.LogInformation($"CreateProduct: Id-{newProduct.Id}, Name-{newProduct.Name}, Price-{newProduct.Price}. {DateTimeOffset.Now}");

                return Results.Ok(result);
            })
            .WithName("CreateProduct")
            .WithOpenApi();

            app.Run();
        }
    }
}
