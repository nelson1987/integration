namespace Adrian.Core.Commands;

public record EscolheMateriaCommand(Guid Id, string Nome, string Documento) : ICommand;
