using Adrian.Core.Commands;

namespace Adrian.Core.Events;

public record CriacaoCandidatoEvent(string Nome, string Documento) : ICommand;