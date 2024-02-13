namespace Integration.Core.Features.Entities;

public class Conta
{
    public Guid Id { get; set; }
    public required string Numero { get; set; }
    public required string Documento { get; set; }
    public decimal Saldo { get; set; }
    public required string Titular { get; set; }
    public required List<Transacao> Transacoes { get; set; }
}
