namespace Integration.Tests.Units;

public class InclusaoContaCommandUnitTests
{
    [Fact]
    public void Deve_Criar_Comando_Com_Sucesso()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nomeTitular = "João Silva";
        var saldoInicial = 1000.00m;
        var ativo = true;
        var tipo = TipoConta.Corrente;

        // Act
        var comando = new InclusaoContaCommand(
            id,
            nomeTitular,
            saldoInicial,
            ativo,
            tipo
        );

        // Assert
        Assert.NotNull(comando);
        Assert.Equal(id, comando.Id);
        Assert.Equal(nomeTitular, comando.NomeTitular);
        Assert.Equal(saldoInicial, comando.SaldoInicial);
        Assert.Equal(ativo, comando.Ativo);
        Assert.Equal(tipo, comando.Tipo);
    }
}
