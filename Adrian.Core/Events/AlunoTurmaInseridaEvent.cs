using Adrian.Core.Commands;

namespace Adrian.Core.Events;

public record AlunoTurmaInseridaEvent(Guid Id, string Nome, string Documento) : ICommand;