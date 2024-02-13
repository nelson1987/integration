using FluentValidation;
using Integration.Api.Features;
using Integration.Core.Features.Commands;
using Integration.Core.Features.Consumers;
using Integration.Core.Features.Entities;
using Integration.Core.Features.Events;
using Integration.Core.Features.Producers;
using Integration.Core.Features.Validators;
using Integration.Core.Repositories;
using Integration.Core.Utils.Observers;
using MassTransit;
using MongoDB.Driver;

namespace Integration.Api;

public static class Dependencies
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        //services.AddScoped<IMongoContext, ConsultaFinanceiraContext>();
        //services.AddMongoDb();
        services.AddRabbitMq();
        services.AddScoped<IValidator<InclusaoContaCommand>, InclusaoContaCommandValidator>();
        services.AddScoped<IProducer<ContaIncluidaEvent>, ContaApiEventsProducer>();
        services.AddScoped<IDataReader<Conta>, ContaDataReader>();
        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumeObserver<ConsumeObserver>();
            x.AddPublishObserver<PublishObserver>();

            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<ContaIncluidaEventConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("amqp://guest:guest@localhost:5672");
                cfg.ConfigureEndpoints(ctx);
                //cfg.UseRawJsonSerializer();
            });
        });
        return services;
    }
    private static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        var settings = new
        {
            MongoClient = "mongodb://root:example@localhost:27017/",
            Database = "sales"
        };
        var mongoClient = new MongoClient(settings.MongoClient);
        services.AddSingleton(mongoClient.GetDatabase(settings.Database));
        return services;
    }
}
