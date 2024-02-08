using Adrian.Core.Consumers;
using Adrian.Core.Events;
using Adrian.Core.Handlers;
using Adrian.Core.Producers;
using Adrian.Core.Repositories.Persistences;
using Adrian.Core.Repositories.Readers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Adrian.Core;

public static class Dependencies
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<ICriacaoAlunoHandler, CriacaoAlunoHandler>();
        services.AddScoped<IAlunoPersistence, AlunoPersistence>();
        services.AddScoped<IAlunoReader, AlunoReader>();
        services.AddScoped<IConsumer<AlunoCriadoEvent>, AlunoCriadoConsumer>();
        services.AddScoped<IProducer<AlunoCriadoEvent>, ContaApiEventsProducer>();
        return services;
    }
    public static IServiceCollection AddMassTransit(this IServiceCollection services)
    //, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<AlunoCriadoConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("amqp://guest:guest@localhost:5672");
                cfg.ConfigureEndpoints(ctx);
                //cfg.UseRawJsonSerializer();
            });
        });
        return services;
    }
}