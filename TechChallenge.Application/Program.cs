using System;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using TechChallenge.API.AutoMapper;
using TechChallenge.Application.Consumers;
using TechChallenge.Data.Context;
using TechChallenge.Data.Repository;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Domain.Services;

namespace TechChallenge.Application
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configuração de AutoMapper
                    services.AddAutoMapper(typeof(MapperProfile), typeof(MapperProfile));

                    // Injeção de dependências para serviços e repositórios
                    services.AddScoped<IContactService, ContactService>();
                    services.AddScoped<IContactRepository, ContactRepository>();

                    // Configuração do contexto de banco de dados
                    var connectionString = hostContext.Configuration.GetConnectionString("SqlConnection");
                    services.AddDbContext<techchallengeDbContext>(options => options.UseSqlServer(connectionString));

                    // Configuração do RabbitMQ com MassTransit
                    var rabbitMqSettings = hostContext.Configuration.GetSection("RabbitMq");
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumers(Assembly.GetEntryAssembly());

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            // Configurações do RabbitMQ lidas do appsettings.json
                            cfg.Host(rabbitMqSettings["Host"], "/", h =>
                            {
                                h.Username(rabbitMqSettings["Username"]);
                                h.Password(rabbitMqSettings["Password"]);
                            });

                            const string exchangeName = "tech.challenge.direct";
                            
                            cfg.ReceiveEndpoint("add-contact", e =>
                            {
                                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                                e.ConfigureConsumeTopology = false;

                                e.Bind(exchangeName, s =>
                                { 
                                    s.RoutingKey = "add.contact";
                                    s.ExchangeType = ExchangeType.Direct;
                                    s.Durable = true;
                                });
                                
                                e.ConfigureConsumer<AddContactConsumer>(context);
                            });
                            
                        
                            cfg.ReceiveEndpoint("update-contact", e =>
                            {
                                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                                e.ConfigureConsumeTopology = false;

                                e.Bind(exchangeName, s =>
                                {
                                    s.RoutingKey = "update.contact";
                                    s.ExchangeType = ExchangeType.Direct;
                                    s.Durable = true;
                                });
                                
                                e.ConfigureConsumer<EditContactConsumer>(context);
                            });
                            
                 
                            cfg.ReceiveEndpoint("delete-contact", e =>
                            {
                                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                                e.ConfigureConsumeTopology = false;

                                e.Bind(exchangeName, s =>
                                {
                                    s.RoutingKey = "delete.contact";
                                    s.ExchangeType = ExchangeType.Direct;
                                    s.Durable = true;
                                });
                                
                                e.ConfigureConsumer<DeleteContactConsumer>(context);
                            });
                        });
                    });
                });
    }
}
