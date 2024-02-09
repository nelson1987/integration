using MongoDB.Driver;

namespace Adrian.Core.Repositories
{
    public interface ISessionInstance : IAsyncDisposable
    {
        Task CommitTransaction(CancellationToken cancellationToken);
        void StartTransaction();
    }

    public sealed class SessionInstance : ISessionInstance
    {
        private readonly IClientSessionHandle _clientSessionHandle;

        // https://www.mongodb.com/docs/manual/reference/read-concern-majority
        private static readonly TransactionOptions TransactionOptions =
            new(ReadConcern.Majority, ReadPreference.Primary, WriteConcern.WMajority);

        public SessionInstance(IClientSessionHandle clientSessionHandle)
            => _clientSessionHandle = clientSessionHandle;

        public async Task CommitTransaction(CancellationToken cancellationToken)
            => await _clientSessionHandle.CommitTransactionAsync(cancellationToken);

        public void StartTransaction() => _clientSessionHandle.StartTransaction(TransactionOptions);

        public async ValueTask DisposeAsync()
        {
            if (_clientSessionHandle.IsInTransaction)
                await _clientSessionHandle.AbortTransactionAsync();

            _clientSessionHandle.Dispose();
        }
    }
}
