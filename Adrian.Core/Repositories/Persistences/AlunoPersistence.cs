using Adrian.Core.Entities;
using MongoDB.Driver;

namespace Adrian.Core.Repositories.Persistences;

public interface IAlunoPersistence
{
    Task<Aluno?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(Aluno newBook, CancellationToken cancellationToken = default);
}
public class AlunoPersistence : IAlunoPersistence
{
    private readonly IMongoCollection<Aluno> _booksCollection;

    public AlunoPersistence()
    {
        var mongoClient = new MongoClient("");

        var mongoDatabase = mongoClient.GetDatabase("");

        _booksCollection = mongoDatabase.GetCollection<Aluno>(nameof(Aluno));
    }
    public async Task<Aluno?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

    [Obsolete]
    public async Task CreateAsync(Aluno newBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.InsertOneAsync(newBook, cancellationToken);

}

