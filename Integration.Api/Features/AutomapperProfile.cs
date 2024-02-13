using AutoMapper;

namespace Integration.Api.Features;


public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<InclusaoContaCommand, Conta>()
.ForMember(x => x.Numero, y => y.Ignore())
.ForMember(x => x.Documento, y => y.Ignore())
.ForMember(x => x.Saldo, y => y.Ignore())
.ForMember(x => x.Titular, y => y.Ignore())
.ForMember(x => x.Transacoes, y => y.Ignore());
        CreateMap<Conta, ContaIncluidaEvent>();

        //.ForMember(x => x.Id, y => y.MapFrom());
    }
}
