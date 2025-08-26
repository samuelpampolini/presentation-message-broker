using Microsoft.AspNetCore.SignalR;
using MessageBroker.Example.CrossCut.Interfaces;
using MessageBroker.Presentation.Api.Support;
using MessageBroker.Example.CrossCut.Settings;
using RabbitMQ.Client;
using FastEndpoints;
using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddSignalR();

// Enable CORS for localhost (React dev server)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.WithOrigins(
                            "http://localhost:5042",
                            "http://localhost:5173",
                            "https://localhost:5173"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                      });
});

// Register API-specific input/output providers
builder.Services.AddSingleton<IExampleInputProvider, ApiInputProvider>();
builder.Services.AddSingleton<IExampleOutputHandler, ApiOutputHandler>();

// Register ExampleFactory and dependencies
builder.Services.AddMessageBrokerCrossCut();
builder.Services.AddSingleton<ExampleFactory>();

// Register RabbitMQ ConnectionFactory from config
builder.Services.AddSingleton<IConnectionFactory>(serviceProvider =>
{
    RabbitMQSettings settings = config.GetRequiredSection("MessageBroker:RabbitMQ").Get<RabbitMQSettings>()!;
    var connectionFactory = new ConnectionFactory
    {
        HostName = settings.Host,
        Port = settings.Port,
        UserName = settings.UserName,
        Password = settings.Password
    };
    return connectionFactory;
});


// Add FastEndpoints and OpenAPI
builder.Services.AddFastEndpoints();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



app.UseCors();
app.UseFastEndpoints();
app.MapHub<MessageHub>("/messageHub");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});

await app.RunAsync();
