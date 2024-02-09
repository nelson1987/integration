namespace Adrian.Core.Commands;

public record InscricaoTurmaCommand(Guid Id, string Nome, string Documento) : ICommand;
