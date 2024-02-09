using Adrian.Core.Commands;

namespace Adrian.Core.Events;

public record AlunoMatriculadoEvent(Guid Id, string Nome, string Documento) : ICommand;
