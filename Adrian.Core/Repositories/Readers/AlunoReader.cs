using Adrian.Core.Commands;
using Adrian.Core.Entities;

namespace Adrian.Core.Repositories.Readers;

public interface IAlunoReader
{
    Task<List<Aluno>> FindAsync(BuscaAlunoQuery command, CancellationToken cancellationToken);
}
public class AlunoReader : IAlunoReader
{
    public async Task<List<Aluno>?> FindAsync(BuscaAlunoQuery command, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new List<Aluno>());
    }
}

