using Adrian.Core.Entities;
using MongoDB.Driver;

namespace Adrian.Core.Repositories.Readers;

public interface IAlunoReader
{
    Task<Aluno?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
public class AlunoReader : IAlunoReader
{
    private readonly IMongoCollection<Aluno> _booksCollection;

    public AlunoReader(IUnitOfWork _unitOfWork)
    {
        //var settings = new MongoSettings()
        //{
        //    MongoClient = "mongodb://root:example@localhost:27017/",
        //    Database = "sales"
        //};
        //var mongoClient = new MongoClient(settings.MongoClient)
        //    .GetDatabase(settings.Database);
        //_booksCollection = mongoClient.GetCollection<Aluno>(nameof(Aluno));
        _booksCollection = _unitOfWork.Collection<Aluno>(nameof(Aluno));
    }

    public async Task<Aluno?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

}

