using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MassTransit;
using Prometheus;
using TechChallenge.API.AutoMapper;
using TechChallenge.API.Configurations;
using TechChallenge.Contract.Contact;
using TechChallenge.Data.Context;
using TechChallenge.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<techchallengeDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configuração do AutoMapper
builder.Services.AddAutoMapper(typeof(MapperProfile), typeof(MapperProfile));

// Configuração de dependências personalizadas
builder.Services.ResolveDependencies();
builder.Services.AddControllers();

// Configuração do MassTransit com RabbitMQ
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // Host do RabbitMQ definido no appsettings.json
        cfg.Host(rabbitMqSettings["Host"], "/", h =>
        {
            h.Username(rabbitMqSettings["Username"]);
            h.Password(rabbitMqSettings["Password"]);
        });
        
        cfg.Message<AddContactMessage>(p => 
        {
            p.SetEntityName("tech.challenge.direct");
        });

        cfg.Publish<AddContactMessage>(p =>
        {
            p.ExchangeType = "direct";
        });

        cfg.Message<EditContactMessage>(p => 
        {
            p.SetEntityName("tech.challenge.direct");
        });

        cfg.Publish<EditContactMessage>(p =>
        {
            p.ExchangeType = "direct";
        });

        cfg.Message<DeleteContactMessage>(p => 
        {
            p.SetEntityName("tech.challenge.direct");
        });

        cfg.Publish<DeleteContactMessage>(p =>
        {
            p.ExchangeType = "direct";
        });


        
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddFluentValidation(v =>
{
    v.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});

var urls = builder.Configuration.GetSection("AllowOrigins").Get<string[]>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechChallenge 4 V1");
    });

}


//Executa as migrações do banco de dados
app.MigrateDatabase();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

// Configura o endpoint de métricas do Prometheus
app.UseMetricServer();

// Configura a coleta de métricas padrão de ASP.NET Core
app.UseHttpMetrics();

// Garante que o endpoint de métricas está mapeado
app.MapMetrics();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();