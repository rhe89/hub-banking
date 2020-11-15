using System;
using AutoMapper;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;

namespace Sbanken.Data.AutoMapper
{
    public class AccountMapperProfile : Profile
    {
        public AccountMapperProfile()
        {
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.Balance,
                    opt => opt.MapFrom(x => x.CurrentBalance))
                .ForMember(dest => dest.Transactions,
                    opt => opt.Condition(src => src.CreatedDate > DateTime.Now.AddDays(-30)))
                .ReverseMap();

        }
    }
}