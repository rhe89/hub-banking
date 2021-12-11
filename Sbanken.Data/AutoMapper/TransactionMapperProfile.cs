using AutoMapper;
using Hub.Shared.DataContracts.Sbanken;
using Sbanken.Data.Entities;

namespace Sbanken.Data.AutoMapper;

public class TransactionMapperProfile : Profile
{
    public TransactionMapperProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(x => x.Text))
            .ReverseMap();
    }
}