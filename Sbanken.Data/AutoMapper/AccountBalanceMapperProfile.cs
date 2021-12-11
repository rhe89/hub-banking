using AutoMapper;
using Hub.Shared.DataContracts.Banking;
using Sbanken.Data.Entities;

namespace Sbanken.Data.AutoMapper
{
    public class AccountBalanceMapperProfile : Profile
    {
        public AccountBalanceMapperProfile()
        {
            CreateMap<AccountBalance, AccountBalanceDto>()
                .ReverseMap();
        }
    }
}