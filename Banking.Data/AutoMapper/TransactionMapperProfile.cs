using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

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