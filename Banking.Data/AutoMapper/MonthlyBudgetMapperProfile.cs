using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class MonthlyBudgetMapperProfile : Profile
{
    public MonthlyBudgetMapperProfile()
    {
        CreateMap<MonthlyBudget, MonthlyBudgetDto>()
            .ReverseMap();
    }
}