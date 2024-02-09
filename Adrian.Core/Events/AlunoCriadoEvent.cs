using Adrian.Core.Commands;

namespace Adrian.Core.Events;

public record AlunoCriadoEvent(Guid Id, string Nome) : ICommand;
