using Adrian.Core.Entities;

namespace Adrian.Core.Repositories.Persistences;

public interface IAlunoPersistence
{
    Task Insert(Aluno entidade, CancellationToken cancellationToken);
}
public class AlunoPersistence : IAlunoPersistence
{
    public async Task Insert(Aluno entidade, CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }
}

