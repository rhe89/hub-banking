using AutoMapper;
using Hub.Shared.DataContracts.Sbanken;
using Sbanken.Data.Entities;

namespace Sbanken.Data.AutoMapper
{
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
}