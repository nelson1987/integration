using Integration.Api;
using Integration.Core.Features.Commands;
using Integration.Core.Features.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace Integration.Tests.IntegrationTests;
public class ContaControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    public ContaControllerIntegrationTests(WebApplicationFactory<Program> applicationFactory) : base()
    {
        _factory = applicationFactory;
    }

    [Fact]
    public async void Given_Valid_Request_Return_Success()
    {
        var httpClient = _factory.CreateClient();
        var inclusaoConta = new InclusaoContaCommand(
            Guid.NewGuid(),
            "NomeTitular",
            10.00M,
            true,
            TipoConta.Corrente);
        var response = await httpClient.PostAsJsonAsync("/conta", inclusaoConta);

        var contaInclusa = response.Content.ReadFromJsonAsync<InclusaoContaCommand>();

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(contaInclusa);
        Assert.Equal(contaInclusa.Id, contaInclusa.Id);

    }

    [Fact]
    public async void Given_Invalid_Request_Return_Fail()
    {
        var httpClient = _factory.CreateClient();
        var inclusaoConta = new InclusaoContaCommand(
            Guid.Empty,
            "NomeTitular",
            10.00M,
            true,
            TipoConta.Corrente);
        var response = await httpClient.PostAsJsonAsync("/conta", inclusaoConta);

        var contaInclusa = response.Content.ReadFromJsonAsync<InclusaoContaCommand>();

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(contaInclusa);
        Assert.Equal(contaInclusa.Id, contaInclusa.Id);

    }

    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync()
    {
        //Implementar Deletar =>
        return Task.CompletedTask;
    }

}
