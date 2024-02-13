using Integration.Core.Features.Entities;

namespace Integration.Core.Features.Commands;

public record InclusaoContaCommand(
    Guid Id, // Identificador único da conta (Guid)
    string NomeTitular, // Nome do titular da conta (string)
    decimal SaldoInicial, // Saldo inicial da conta (decimal)
    bool Ativo, // Indica se a conta está ativa (bool)
    TipoConta Tipo // Tipo da conta (enum)
);
