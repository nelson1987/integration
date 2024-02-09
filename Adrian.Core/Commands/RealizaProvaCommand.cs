namespace Adrian.Core.Commands;

public record RealizaProvaCommand(Guid Id, string Nome, string Documento) : ICommand;
