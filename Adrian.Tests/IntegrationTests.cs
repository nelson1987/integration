using Adrian.Core.Commands;
using Adrian.Core.Consumers;
using Adrian.Core.Events;
using Adrian.Core.Producers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace Adrian.Tests;
public class Api : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Test")
               .ConfigureTestServices(services =>
               {
                   services.AddMassTransitTestHarness(x =>
                   {
                       x.AddDelayedMessageScheduler();
                       x.AddConsumer<AlunoCriadoConsumer>();
                       //x.UsingInMemory((context, cfg) =>
                       //{
                       //    cfg.UseDelayedMessageScheduler();

                       //    cfg.ConfigureEndpoints(context);
                       //});
                   });
               });
}

public class IntegrationTest
{
    public HttpClient HttpClient;
    public Api Api;
    public ITestHarness TestHarness;
    public IProducer<AlunoCriadoEvent> Producer;
    public IntegrationTest()
    {
        Api = new Api();
        HttpClient = Api.CreateClient();
        TestHarness = Api.Services.GetTestHarness();
        Producer = Api.Services.GetRequiredService<IProducer<AlunoCriadoEvent>>();
    }
}

public class IntegrationTests : IntegrationTest
{
    public IntegrationTests() : base()
    {
    }

    [Fact]
    public async Task Integration_Api()
    {
        var response = await HttpClient.GetAsync("/weatherforecast");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Integration_To_PubSub()
    {
        var orderId = NewId.NextGuid();

        var submitOrderResponse = await HttpClient.PostAsync("/weatherforecast",
            JsonContent.Create(new CriacaoAlunoCommand(orderId, "teste")));
        submitOrderResponse.EnsureSuccessStatusCode();

        var sagaTestHarness = TestHarness.GetConsumerHarness<AlunoCriadoConsumer>();

        Assert.True(await TestHarness.Published.Any<AlunoCriadoEvent>());
        Assert.True(await sagaTestHarness.Consumed.Any<AlunoCriadoEvent>(x => x.Context.Message.Id == orderId));
    }

    [Fact]
    public async Task Integration_PubSub()
    {
        var orderId = NewId.NextGuid();
        AlunoCriadoEvent @event = new AlunoCriadoEvent(orderId, "Teste");

        using var tokenSource = ExpiringCancellationToken();
        await Producer.Send(@event, tokenSource.Token);
        Assert.True(await TestHarness.Published.Any<AlunoCriadoEvent>());
    }

    private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
    {
        var timeout = TimeSpan.FromMilliseconds(msTimeout);
        return new CancellationTokenSource(timeout);
    }
}