namespace Adrian.Core.Commands;

public record MatriculaAlunoCommand(Guid Id, string Nome, string Documento) : ICommand;
