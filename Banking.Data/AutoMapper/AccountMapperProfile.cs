using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking;

namespace Banking.Data.AutoMapper;

public class AccountMapperProfile : Profile
{
    public AccountMapperProfile()
    {
        CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(x => x.CurrentBalance))
            .ReverseMap();
    }
}