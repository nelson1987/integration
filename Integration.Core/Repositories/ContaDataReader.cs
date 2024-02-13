using Integration.Api.Features;
using Integration.Core.Features.Entities;

namespace Integration.Core.Repositories;

public class ContaDataReader : DataReader<Conta>
{
    public ContaDataReader(ILogger<Producer<Conta>> logger) : base(logger)
    {
    }
}
