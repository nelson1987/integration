using Adrian.Core.Commands;

namespace Adrian.Core.Events;

public record AlunoCompareceuEvent(Guid Id, string Nome, string Documento) : ICommand;
