﻿using Integration.Api;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace Integration.Tests.IntegrationTests;
public class ContaControllerIntegrationTests// : IAsyncLifetime
{
    /*
    private readonly WebApplicationFactory<Program> _applicationFactory;

    public ContaControllerIntegrationTests(WebApplicationFactory<Program> applicationFactory)
    {
        _applicationFactory = applicationFactory;
    }

    [Fact]
    public async void Given_Valid_Request_Return_Success()
    {
        var httpClient = _applicationFactory.CreateClient();
        var inclusaoConta = new InclusaoContaCommand(
            Guid.NewGuid(),
            "NomeTitular",
            10.00M,
            true,
            TipoConta.Corrente);
        var response = await httpClient.PostAsJsonAsync("/weatherforecast", inclusaoConta);

        var contaInclusa = response.Content.ReadFromJsonAsync<InclusaoContaCommand>();

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(contaInclusa);
        Assert.Equal(contaInclusa.Id, contaInclusa.Id);

    }

    [Fact]
    public async void Given_Invalid_Request_Return_Fail()
    {
        var httpClient = _applicationFactory.CreateClient();
        var inclusaoConta = new InclusaoContaCommand(
            Guid.Empty,
            "NomeTitular",
            10.00M,
            true,
            TipoConta.Corrente);
        var response = await httpClient.PostAsJsonAsync("/weatherforecast", inclusaoConta);

        var contaInclusa = response.Content.ReadFromJsonAsync<InclusaoContaCommand>();

        Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(contaInclusa);
        Assert.Equal(contaInclusa.Id, contaInclusa.Id);

    }
    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync()
    {
        //Implementar Deletar =>
        return Task.CompletedTask;
    }



    */

    [Fact]
    public async Task Should_have_the_submitted_status()
    {

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .ConfigureServices(services =>
                {
                    services.AddMassTransitTestHarness(x =>
                    {
                        //x.SetKebabCaseEndpointNameFormatter();
                        //x.AddConsumer<ContaIncluidaEventConsumer>();
                        //x.UsingRabbitMq((ctx, cfg) =>
                        //{
                        //    //cfg.Host("amqp://guest:guest@localhost:5672");
                        //    cfg.ConfigureEndpoints(ctx);
                        //    //cfg.UseRawJsonSerializer();
                        //});
                    });
                }));
        var testHarness = application.Services.GetTestHarness();
        using var client = application.CreateClient();
        const string submitOrderUrl = "/conta";
        var orderId = Guid.NewGuid();
        var inclusaoConta = new InclusaoContaCommand(
    orderId,
    "NomeTitular",
    10.00M,
    true,
    TipoConta.Corrente);

        var submiteOrderResponse = await client.PostAsJsonAsync(submitOrderUrl, inclusaoConta, CancellationToken.None);

        submiteOrderResponse.EnsureSuccessStatusCode();

        Assert.True(await testHarness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var sagatestharness = testHarness.GetConsumerHarness<ContaIncluidaEventConsumer>();

        Assert.True(await sagatestharness.Consumed.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var mensgaem = await sagatestharness.Consumed.ConsumedValue<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId);

        Assert.Equal(orderId, mensgaem.Id);
    }
    [Fact]
    public async Task ASampleTest()
    {

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<ContaIncluidaEventConsumer>();
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.ConfigureEndpoints(ctx);
                });
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();

        var inclusaoConta = new ContaIncluidaEvent()
        {
            Id = orderId,
            Numero = "NomeTitular"
        };

        Assert.False(await harness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        await harness.Bus.Publish(inclusaoConta, CancellationToken.None);

        Assert.True(await harness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var sagaHarness = harness.GetConsumerHarness<ContaIncluidaEventConsumer>();

        Assert.True(await sagaHarness.Consumed.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var mensgaem = await sagaHarness.Consumed.ConsumedValue<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId);

        Assert.Equal(inclusaoConta.Id, mensgaem.Id);
        Assert.Equal(inclusaoConta.Numero, mensgaem.Numero);

        await harness.Stop();
    }
}
