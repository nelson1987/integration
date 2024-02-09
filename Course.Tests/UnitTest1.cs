using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Xunit.Abstractions;

namespace Course.Tests
{
    public class UnitTest1
    {
        public ITestOutputHelper Output { get; set; }
        private readonly KafkaTeste kafka;
        public UnitTest1(ITestOutputHelper output)
        {
            Output = output;
            Output.WriteLine("Construtor Iniciado");
            kafka = new KafkaTeste();
        }
        [Fact]
        public async Task TesteIntegracaoKafka()
        {
            //Arrange
            string mensagem = "mensagem";
            //Act
            await kafka.Publicar(mensagem);
            //Assert
            var consumido = await kafka.Consumir();
            Assert.Equal(mensagem, consumido);
        }
    }
    public class KafkaTeste : IAsyncLifetime
    {
        private Dictionary<int, string> Mensagem { get; set; }
        public async Task Publicar(string message)
        {
            int offset = Mensagem.Count;
            Mensagem.Add(offset++, message);
        }
        public async Task<string> Consumir()
        {
            return Mensagem.Last().Value;
        }

        public Task InitializeAsync()
        {
            Mensagem = new Dictionary<int, string>();
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Mensagem = new Dictionary<int, string>();
            return Task.CompletedTask;
        }
    }
}