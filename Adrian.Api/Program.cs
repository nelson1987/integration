using Adrian.Core;
using Adrian.Core.Commands;
using Adrian.Core.Handlers;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDomain()
                .AddMassTransit();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async ([FromServices] ICriacaoAlunoHandler handler, [FromQuery] string nome, CancellationToken cancellationToken) =>
{
    var result = await handler.GetAsync(new BuscaAlunoQuery(Guid.NewGuid(), nome), cancellationToken);
    return result.ValueOrDefault;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast", async ([FromServices] ICriacaoAlunoHandler handler, CriacaoAlunoCommand command, CancellationToken cancellationToken) =>
{
    await handler.PostAsync(command, cancellationToken);
})
.WithName("PostWeatherForecast")
.WithOpenApi();

app.Run();

public partial class Program { }