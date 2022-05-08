using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class RecurringTransactionMapperProfile : Profile
{
    public RecurringTransactionMapperProfile()
    {
        CreateMap<RecurringTransaction, RecurringTransactionDto>()
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(x => x.Text))
            .ReverseMap();
    }
}