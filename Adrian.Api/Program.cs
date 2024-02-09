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

app.MapGet("/weatherforecast", async ([FromServices] IAlunoHandler handler, [FromQuery] string nome, CancellationToken cancellationToken) =>
{
    var result = await handler.GetAsync(new BuscaAlunoQuery(Guid.NewGuid(), nome), cancellationToken);
    return result.ValueOrDefault;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/weatherforecast", async ([FromServices] IAlunoHandler handler, CriacaoCandidatoCommand command, CancellationToken cancellationToken) =>
{
    await handler.CreateCandidateAsync(command, cancellationToken);
})
.WithName("PostWeatherForecast")
.WithOpenApi();

app.Run();

public partial class Program { }