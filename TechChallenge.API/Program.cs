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

        // Configuração das mensagens sem uso de ExchangeName
        cfg.Publish<AddContactMessage>(p => p.ExchangeType = "direct");
        cfg.Publish<EditContactMessage>(p => p.ExchangeType = "direct");
        cfg.Publish<DeleteContactMessage>(p => p.ExchangeType = "direct");
    });
});

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do FluentValidation para validações
builder.Services.AddControllers().AddFluentValidation(v =>
{
    v.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});

// Configuração de CORS
var urls = builder.Configuration.GetSection("AllowOrigins").Get<string[]>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechChallenge 4 V1");
    });
}

// Executa as migrações do banco de dados automaticamente ao iniciar
app.MigrateDatabase();

// Configuração de CORS com origens permitidas
app.UseCors(x => x
    .WithOrigins(urls)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

// Configuração de métricas para Prometheus
app.UseMetricServer();
app.UseHttpMetrics();
app.MapMetrics();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
