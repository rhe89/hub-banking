using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class BankMapperProfile : Profile
{
    public BankMapperProfile()
    {
        CreateMap<Bank, BankDto>()
            .ReverseMap();
    }
}