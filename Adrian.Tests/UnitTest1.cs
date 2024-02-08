using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Adrian.Tests;
public class Api : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Test")
               .ConfigureTestServices(services =>
               {
               });
}

public class UnitTest1
{
    public HttpClient HttpClient;
    public Api Api;

    public UnitTest1()
    {
        Api = new Api();
        HttpClient = Api.CreateClient();
    }

    [Fact]
    public async Task Test1()
    {
        //var evento = await Api.Services.GetRequiredService<AlunoConsumer>();
        var response = await HttpClient.GetAsync("/weatherforecast");
        response.EnsureSuccessStatusCode();
    }
}