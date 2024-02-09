using MongoDB.Driver;

namespace Adrian.Core.Repositories
{
    public interface IUnitOfWork
    {
        IClientSessionHandle? Session { get; }
        IMongoCollection<T> Collection<T>(string name) where T : class;
        Task<ISessionInstance> StartSession(CancellationToken cancellationToken);
    }
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMongoDatabase _database;
        private IClientSessionHandle? _session;

        public IClientSessionHandle? Session => _session;

        public UnitOfWork(IMongoDatabase mongoDatabase) => _database = mongoDatabase;

        public IMongoCollection<T> Collection<T>(string name) where T : class => _database.GetCollection<T>(name);

        public async Task<ISessionInstance> StartSession(CancellationToken cancellationToken)
        {
            _session = await _database.Client.StartSessionAsync(cancellationToken: cancellationToken);

            return new SessionInstance(_session);
        }
    }
}
