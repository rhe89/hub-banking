using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class AccumulatedAccountBalanceMapperProfile : Profile
{
    public AccumulatedAccountBalanceMapperProfile()
    {
        CreateMap<AccumulatedAccountBalance, AccountBalanceDto>()
            .ReverseMap();
    }
}