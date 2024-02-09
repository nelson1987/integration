using Adrian.Core.Commands;

namespace Adrian.Core.Events;

public record AlunoInscritoEvent(Guid Id, string Nome, string Documento) : ICommand;
