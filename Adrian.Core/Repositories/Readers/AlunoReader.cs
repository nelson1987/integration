using Adrian.Core.Entities;
using Adrian.Core.Settings;
using MongoDB.Driver;

namespace Adrian.Core.Repositories.Readers;

public interface IAlunoReader
{
    Task<Aluno?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
public class AlunoReader : IAlunoReader
{
    private readonly IMongoCollection<Aluno> _booksCollection;

    public AlunoReader()
    {
        var settings = new MongoSettings();
        var mongoClient = new MongoClient(settings.MongoClient)
            .GetDatabase(settings.Database);
        _booksCollection = mongoClient.GetCollection<Aluno>(nameof(Aluno));
    }
    
    public async Task<Aluno?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

}

