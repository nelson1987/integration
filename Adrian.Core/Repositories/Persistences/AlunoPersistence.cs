using Adrian.Core.Entities;
using MongoDB.Driver;
using static MongoDB.Driver.WriteConcern;

namespace Adrian.Core.Repositories.Persistences;

public interface IAlunoPersistence
{
    void Create(Aluno newBook);
    Task CreateAsync(Aluno newBook, CancellationToken cancellationToken = default);
    Task UpdateAsync(Aluno aluno, StatusAluno status, CancellationToken cancellationToken = default);
}
public class AlunoPersistence : IAlunoPersistence
{
    private readonly IMongoCollection<Aluno> _alunosCollection;
    
    public AlunoPersistence(IUnitOfWork unitOfWork)
    {
        //var settings = new MongoSettings()
        //{
        //    MongoClient = "mongodb://root:example@localhost:27017/",
        //    Database = "sales"
        //};
        //var mongoClient = new MongoClient(settings.MongoClient);
        //var database = mongoClient.GetDatabase(settings.Database);
        //_alunosCollection = database.GetCollection<Aluno>(nameof(Aluno));
        _alunosCollection = unitOfWork.Collection<Aluno>(nameof(Aluno));
    }

    public void Create(Aluno newBook) =>
        _alunosCollection.InsertOne(newBook);

    public async Task CreateAsync(Aluno newBook, CancellationToken cancellationToken = default) =>
        await _alunosCollection.InsertOneAsync(newBook, cancellationToken);

    public async Task UpdateAsync(Aluno aluno, StatusAluno status, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Aluno>.Filter
            .Eq(x => x.Id, aluno.Id);
        var update = Builders<Aluno>.Update
            .Set(x => x.Status, aluno.Status);
        var result = await _alunosCollection.UpdateOneAsync(filter, update);

        Console.WriteLine($"Updated documents: {result.ModifiedCount}");
        Console.WriteLine($"Result acknowledged? {result.IsAcknowledged}");
    }
}

