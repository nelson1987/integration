using Adrian.Core.Consumers;
using Adrian.Core.Events;
using Adrian.Core.Handlers;
using Adrian.Core.Producers;
using Adrian.Core.Repositories;
using Adrian.Core.Repositories.Persistences;
using Adrian.Core.Repositories.Readers;
using Adrian.Core.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Adrian.Core;

public static class Dependencies
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        //var mongoConfiguration = new MongoSettings()
        //{
        //    MongoClient = "mongodb://root:example@localhost:27017/sales",
        //    Database = "sales"
        //};

        //var mongoUrl = new MongoUrl(mongoConfiguration.MongoClient);
        //var settings = MongoClientSettings.FromUrl(mongoUrl);
        ////services.AddMongoDbInstrumentation(settings);
        //var mongoClient = new MongoClient(settings);

        var settings = new MongoSettings()
        {
            MongoClient = "mongodb://root:example@localhost:27017/",
            Database = "sales"
        };
        var mongoClient = new MongoClient(settings.MongoClient);
        services.AddSingleton(mongoClient.GetDatabase(settings.Database));
        services.AddSingleton<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAlunoHandler, AlunoHandler>();
        services.AddScoped<IAlunoPersistence, AlunoPersistence>();
        services.AddScoped<IAlunoReader, AlunoReader>();
        services.AddScoped<IAlunoService, AlunoService>();
        services.AddScoped<IProducer<CriacaoCandidatoEvent>, ContaApiEventsProducer>();
        services.AddScoped<IConsumer<AlunoAprovadoEvent>, AlunoAprovadoConsumer>();
        services.AddScoped<IConsumer<AlunoCompareceuEvent>, RealizacaoProvaConsumer>();
        services.AddScoped<IConsumer<AlunoMatriculadoEvent>, MatriculaAlunoConsumer>();
        services.AddScoped<IConsumer<AlunoTurmaInseridaEvent>, MateriaEscolhidaConsumer>();
        services.AddScoped<IConsumer<AlunoInscritoEvent>, InscricaoTurmaConsumer>();
        services.AddScoped<IConsumer<AlunoAprovadoEvent>, AlunoAprovadoConsumer>();
        services.AddScoped<IConsumer<CriacaoCandidatoEvent>, CriacaoCandidatoConsumer>();
        return services;
    }

    public static IServiceCollection AddMassTransit(this IServiceCollection services)
    //, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<AlunoAprovadoConsumer>();
            x.AddConsumer<RealizacaoProvaConsumer>();
            x.AddConsumer<MatriculaAlunoConsumer>();
            x.AddConsumer<MateriaEscolhidaConsumer>();
            x.AddConsumer<InscricaoTurmaConsumer>();
            x.AddConsumer<AlunoAprovadoConsumer>();
            x.AddConsumer<CriacaoCandidatoConsumer>();
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