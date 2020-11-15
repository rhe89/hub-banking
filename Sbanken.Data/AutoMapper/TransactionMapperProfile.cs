using AutoMapper;
using Sbanken.Core.Entities;
using Sbanken.Core.Dto.Data;

namespace Sbanken.Data.AutoMapper
{
    public class TransactionMapperProfile : Profile
    {
        public TransactionMapperProfile()
        {
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.AccountName,
                    opt => opt.MapFrom(x => x.Account.Name))
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(x => x.Text))
                .ForMember(dest => dest.TransactionDate,
                    opt => opt.MapFrom(x => x.TransactionDate))
                .ForMember(dest => dest.TransactionIdentifier,
                    opt => opt.MapFrom(x => x.TransactionId))
                .ReverseMap();
        }
    }
}