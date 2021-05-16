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
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(x => x.Text))
                .ReverseMap();
        }
    }
}