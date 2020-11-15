using AutoMapper;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;

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