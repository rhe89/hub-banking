using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class AccountMapperProfile : Profile
{
    public AccountMapperProfile()
    {
        CreateMap<Account, AccountDto>()
            .ForMember(dto => dto.Balance, opt => opt.Ignore())
            .ForMember(dto => dto.BalanceDate, opt => opt.Ignore())
            .ForMember(dto => dto.BalanceIsAccumulated, opt => opt.Ignore())
            .ForMember(dto => dto.NoBalanceForGivenPeriod, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.AccountBalance, opt => opt.Ignore())
            .ForMember(dest => dest.Bank, opt => opt.Ignore());

    }
}