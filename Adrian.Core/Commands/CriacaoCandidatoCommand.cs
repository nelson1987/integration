namespace Adrian.Core.Commands;
public record CriacaoCandidatoCommand(string Nome, string Documento) : ICommand;
