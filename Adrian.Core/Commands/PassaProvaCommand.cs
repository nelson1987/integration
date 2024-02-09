namespace Adrian.Core.Commands;

public record PassaProvaCommand(Guid Id, string Nome, string Documento) : ICommand;
