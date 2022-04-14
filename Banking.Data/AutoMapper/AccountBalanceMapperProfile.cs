using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking;

namespace Banking.Data.AutoMapper;

public class AccountBalanceMapperProfile : Profile
{
    public AccountBalanceMapperProfile()
    {
        CreateMap<AccountBalance, AccountBalanceDto>()
            .ReverseMap();
    }
}