namespace Integration.Core.Features.Entities;

public class Transacao
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public required string Tipo { get; set; }
    public required string Descricao { get; set; }
}
