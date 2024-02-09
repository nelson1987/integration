using Adrian.Core.Entities;
using Adrian.Core.Settings;
using MongoDB.Driver;

namespace Adrian.Core.Repositories.Persistences;

public interface IAlunoPersistence
{
    Task CreateAsync(Aluno newBook, CancellationToken cancellationToken = default);
}
public class AlunoPersistence : IAlunoPersistence
{
    private readonly IMongoCollection<Aluno> _booksCollection;
    
    public AlunoPersistence()
    {
        var settings = new MongoSettings();
        var mongoClient = new MongoClient(settings.MongoClient)
            .GetDatabase(settings.Database);
        _booksCollection = mongoClient.GetCollection<Aluno>(nameof(Aluno));
    }

    public async Task CreateAsync(Aluno newBook, CancellationToken cancellationToken = default) =>
        await _booksCollection.InsertOneAsync(newBook, cancellationToken);

}

