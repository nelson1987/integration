using Adrian.Core.Commands;

namespace Adrian.Core.Events;

public record AlunoAprovadoEvent(Guid Id, string Nome, string Documento) : ICommand;
